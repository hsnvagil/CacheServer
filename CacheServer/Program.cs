using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CacheServer {
    class Program {
        private static readonly Dictionary<string, string> Posts = new Dictionary<string, string>();
        private static IPEndPoint _clientEp;
        static void Main(string[] args) {
            var ip = IPAddress.Parse("127.0.0.1");
            var listener = new TcpListener(ip, 12345);
            listener.Start(100);

            while (true) {
                var client = listener.AcceptTcpClient();
                _clientEp = (client.Client.RemoteEndPoint as IPEndPoint);
                Console.WriteLine($"{_clientEp.Address} : {_clientEp.Port} Connected");
                var stream = client.GetStream();
                var br = new BinaryReader(stream);
                var bw = new BinaryWriter(stream);
                while (true) {
                    var input = br.ReadString();
                    if (input == "disconnected") {
                        client.Close();
                        Console.WriteLine($"{_clientEp.Address} : {_clientEp.Port} Disconnected");
                        break;
                    }

                    Post data;
                    try {
                        data = JsonConvert.DeserializeObject<Post>(input);

                    } catch (Exception) {
                        continue;
                    }


                    switch (data.Operation) {
                        case "write":
                            SetCache(data.Data.Key, data.Data.Value);
                            break;
                        case "read":
                            var response = GetCache(data.Data.Key);
                            bw.Write(response);
                            if (response != "null") {
                                Console.WriteLine($"{_clientEp.Address} : {_clientEp.Port} read post : {data.Data.Key} | {response}");
                            }
                            break;
                    }

                }

            }
        }

        private static void SetCache(string key, string value) {
            if (Posts.ContainsKey(key)) {
                Console.WriteLine($"{_clientEp.Address} : {_clientEp.Port} updated {Posts[key]} to {value}");
                Posts[key] = value;
            }
            else {
                Posts.Add(key, value);
                Console.WriteLine($"{_clientEp.Address} : {_clientEp.Port} added new post : {key} | {value}");
            }
        }

        private static string GetCache(string key) {
            return Posts.ContainsKey(key) ? Posts[key] : "null";
        }
    }
}
