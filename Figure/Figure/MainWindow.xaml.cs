using System.Windows;
using System;
using System.Text;
using System.Net.Sockets;


namespace WPF
{
    // OpenCvSharp 설치 시 Window를 명시적으로 사용해 주어야 함 (window -> System.Windows.Window)
    public partial class MainWindow : System.Windows.Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private void windows_loaded(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("클라이언트 접속");
        }
    }
}


