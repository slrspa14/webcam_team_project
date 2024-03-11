using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Net.Sockets;
using System.IO;


namespace Figure
{
    /// <summary>
    /// start_page.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class start_page : Page
    {
        static public NetworkStream stream;
        static public TcpClient client;
        public start_page()
        {
            InitializeComponent();
            ////서버 연결용
            string message = "1 / 1번라인";//서버 라인 확인용
            try
            {
                //IPEndPoint clientAddress = new IPEndPoint(IPAddress.Parse(bindIp), bindPort);
                //IPEndPoint serverAddress = new IPEndPoint(IPAddress.Parse(serverIp), serverPort);

                client = new TcpClient();
                //client.Connect("192.168.219.105", 33323);      // 연결
                //client.Connect("192.168.35.105", 9195);//노트북
                client.Connect("10.10.20.106", 9191);//개발원
                System.Windows.MessageBox.Show("1");
                byte[] data = Encoding.Default.GetBytes(message);
                stream = client.GetStream();
                stream.Write(data, 0, data.Length);// 메세지 보냄
                System.Windows.MessageBox.Show("2");
            }
            catch (SocketException ex)
            {
                System.Windows.MessageBox.Show("오류");
                Console.WriteLine(ex);
            }
        }

        private void Start_btn_Click(object sender, RoutedEventArgs e)
        {
            Uri uri = new Uri("/webcam_page.xaml", UriKind.Relative);
            NavigationService.Navigate(uri);
        }
    }
}
