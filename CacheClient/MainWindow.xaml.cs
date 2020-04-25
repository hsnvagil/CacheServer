using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json;

namespace CacheClient {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private TcpClient _client;
        private NetworkStream _stream;
        private BinaryReader _br;
        private BinaryWriter _bw;
        public MainWindow() {
            InitializeComponent();
        }

        private bool IsValidateIP(string address) {
            const string pattern = @"^([1-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])(\.([0-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])){3}$";
            var check = new Regex(pattern);

            return !string.IsNullOrEmpty(address) && check.IsMatch(address, 0);
        }

        private void WriteCache(KeyValuePair<string, string> keyValuePair) {
            var post = new Post() {
                Operation = "write",
                Data = keyValuePair
            };

            var output = JsonConvert.SerializeObject(post);
            _bw.Write(output);
        }

        private string ReadCache(KeyValuePair<string, string> keyValuePair) {
            var post = new Post {
                Operation = "read",
                Data = keyValuePair
            };

            var output = JsonConvert.SerializeObject(post);
            _bw.Write(output);

            return _br.ReadString();
        }

        private void Disconnected() {
            _bw.Write("disconnected");
            _client.Close();

            ReadKeyTxtBox.Text = "";
            ResultValueTxtBlock.Text = "";
            MainGrid.Visibility = Visibility.Hidden;
            Panel.Visibility = Visibility.Visible;
        }
        private void ConnectBtn_OnClick(object sender, RoutedEventArgs e) {
            var address = IpAddressTxtBox.Text;
            if (!IsValidateIP(address)) {
                MessageBox.Show("invalid ip address");
                return;
            }

            _client = new TcpClient();
            try {
                _client.Connect(address, 12345);
            } catch (Exception exception) {
                MessageBox.Show(exception.Message);
            }
            
            if (_client.Connected) {
                Panel.Visibility = Visibility.Hidden;
                MainGrid.Visibility = Visibility.Visible;

                _stream = _client.GetStream();

                _bw = new BinaryWriter(_stream);
                _br = new BinaryReader(_stream);
            }
        }

        private void WriteBtn_OnClick(object sender, RoutedEventArgs e) {
            var key = WriteKeyTxtBox.Text;
            var value = WriteValueTxtBox.Text;

            var keyValuePair = new KeyValuePair<string,string>(key,value);

            WriteCache(keyValuePair);
            WriteKeyTxtBox.Text = WriteValueTxtBox.Text = "";
        }
        private void ReadBtn_OnClick(object sender, RoutedEventArgs e) {
            var key = ReadKeyTxtBox.Text;
            var keyValuePair = new KeyValuePair<string,string>(key, "");


            ResultValueTxtBlock.Text = ReadCache(keyValuePair);
        }

        private void DisconnectedBtn_OnClick(object sender, RoutedEventArgs e) {
            Disconnected();
        }

        private void MainWindow_OnClosed(object sender, EventArgs e) {
            Disconnected();
            Close();
        }

    }
}
