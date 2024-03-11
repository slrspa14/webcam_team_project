using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Net.Sockets;
using System.IO;

// OpenCV 사용을 위한 using
using OpenCvSharp;
using Point = OpenCvSharp.Point;
// Timer 사용을 위한 using
using System.Windows.Threading;

namespace Figure
{
    public partial class webcam_page : Page
    {
        // 필요한 변수 선언
        
        VideoCapture cam;
        Mat frame, test;
        DispatcherTimer timer;
        bool is_initCam, is_initTimer;
        string save, tor;
        string work_detail = start_page.work_detail;//전 페이지에서 받은 작업내용 사용하려고
        string shape = "unidentified";
        int cnt = 0;

        //public NetworkStream stream;
        //public TcpClient client;
        public webcam_page()
        {
            InitializeComponent();
        }

        private void windows_loaded(object sender, RoutedEventArgs e)
        {
            // 카메라, 타이머(0.01ms 간격) 초기화
            is_initCam = init_camera();
            is_initTimer = init_Timer(0.01);

            // 초기화 완료면 타이머 실행
            if (is_initTimer && is_initCam) timer.Start();
        }

        private bool init_Timer(double interval_ms)
        {
            try
            {
                timer = new DispatcherTimer();

                timer.Interval = TimeSpan.FromMilliseconds(interval_ms);
                timer.Tick += new EventHandler(timer_tick);

                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool init_camera()
        {

            try
            {
                // 0번 카메라로 VideoCapture 생성 (카메라가 없으면 안됨)
                cam = new VideoCapture(0);
                cam.FrameHeight = (int)Cam.Height;
                cam.FrameWidth = (int)Cam.Width;

                // 카메라 영상을 담을 Mat 변수 생성
                frame = new Mat();

                return true;
            }
            catch
            {
                return false;
            }
        }

        private string GetShape(Point[] c)
        {
            //string shape = "unidentified";
            double peri = Cv2.ArcLength(c, true);
            Point[] approx = Cv2.ApproxPolyDP(c, 0.04 * peri, true);


            if (approx.Length == 3) //if the shape is a triangle, it will have 3 vertices
            {
                shape = "Triangle";
            }
            else if (approx.Length == 4)    //if the shape has 4 vertices, it is either a square or a rectangle
            {
                OpenCvSharp.Rect rect;
                rect = Cv2.BoundingRect(approx);
                double ar = rect.Width / (double)rect.Height;

                if (ar >= 0.95 && ar <= 1.05) shape = "Square";
                else shape = "Square";
            }
            else if (approx.Length == 5)    //if the shape has 5 vertice, it is a pantagon
            {
                shape = "Pentagon";
            }
            else   //otherwise, shape is a circle
            {
                shape = "Circle";
            }
            return shape;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)//문제없을때의 전송내용버튼
        {
            string message = "2 / Red Square / 2023 - 01 - 23 - 16 - 32 - 12 / 1번라인 / 박철두 / pass";
            byte[] data = Encoding.Default.GetBytes(message);
            
            start_page.stream = start_page.client.GetStream();
            start_page.stream.Write(data, 0, data.Length);
            //MessageBox.Show("2하잉2");//확인용
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            string img_message = "4/오류 사진 보냅니다.";

            //----------------------------------------------------------------------------------
            //오류 보고 메시지 한 번 보내고
            try
            {
                //stream null값인지 확인해보기
                byte[] errorReport = Encoding.Default.GetBytes(img_message);
                start_page.stream.Write(errorReport, 0, errorReport.Length);//4/오류사진

                //데이터 크기 보내고
                byte[] size = new byte[4];
                FileStream filestr = new FileStream("testtest.png", FileMode.Open, FileAccess.Read);
                int fileLength = (int)filestr.Length;
                size = Encoding.Default.GetBytes(fileLength.ToString());
                //string a = fileLength.ToString();
                //size = Encoding.Default.GetBytes(a);
                start_page.stream.Write(size, 0, 4);
                System.Windows.MessageBox.Show("송신 데이터 크기 : " + BitConverter.ToInt32(size, 0).ToString());

                byte[] imageData = new byte[fileLength];

                filestr.Read(imageData, 0, imageData.Length);//파일읽어서 배열에 넣고
                start_page.stream.Write(imageData, 0, imageData.Length);//송신

                System.Windows.MessageBox.Show("파일전송완료");
                filestr.Close();//닫기추가
            }
            catch (SocketException file)
            {
                Console.Write(file);
                System.Windows.MessageBox.Show(file.ToString());
            }

        }//파일전송버튼

        private void Button_Click(object sender, RoutedEventArgs e)//캡처버튼
        {
            Cam_cpature.Source = OpenCvSharp.WpfExtensions.WriteableBitmapConverter.ToWriteableBitmap(frame);//화면안나와서 오류
            save = DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss");            // 현재 시간
            Cv2.ImWrite("../../" + save + ".png", frame);
            //VideoWriter recodetest = new VideoWriter("./" + save + ".avi", FourCC.XVID, 24, frame.Size());
        }
        private void timer_tick(object sender, EventArgs e)
        {
            frame = new Mat(); //
            cam.Read(frame); //입력할거

            test = new Mat(); //출력 할거
            Cv2.CvtColor(frame, test, ColorConversionCodes.BGR2HSV);
            //             입력   출력  변환식              

            Mat mask1 = new Mat();
            Cv2.InRange(test, new Scalar(20, 50, 50), new Scalar(30, 255, 255), mask1); //노랑
            Mat mask2 = new Mat();
            Cv2.InRange(test, new Scalar(40, 70, 80), new Scalar(70, 255, 255), mask2); //초록
            Mat mask3 = new Mat();
            Cv2.InRange(test, new Scalar(0, 50, 120), new Scalar(10, 255, 255), mask3); //빨강
            Mat mask4 = new Mat();
            Cv2.InRange(test, new Scalar(90, 60, 0), new Scalar(121, 255, 255), mask4); //파랑

            Cv2.FindContours(mask1, out var contours1, out var hierarchy1, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple);
            Cv2.FindContours(mask2, out var contours2, out var hierarchy2, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple);
            Cv2.FindContours(mask3, out var contours3, out var hierarchy3, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple);
            Cv2.FindContours(mask4, out var contours4, out var hierarchy4, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple);

            int yel = 0, gre = 0, red = 0, blu = 0;

            foreach (var c in contours1)
            {
                var area = Cv2.ContourArea(c);
                if (area > 8000) //픽셀단위 8000이상일때만
                {
                    Moments m = Cv2.Moments(c);
                    Point pnt = new Point(m.M10 / m.M00, m.M01 / m.M00); //center point
                    Cv2.DrawContours(frame, new[] { c }, -1, Scalar.Yellow, 3); //윤곽선 그리는거 필요없음
                    string shape = GetShape(c); //형상구분함수 밑에 있음 string을 반환함
                    var M = Cv2.Moments(c);
                    var cx = (int)(M.M10 / M.M00);
                    var cy = (int)(M.M01 / M.M00); //이 3놈 중앙 찾음
                    tor = "Yellow_" + shape;
                    Cv2.PutText(frame, pnt + "Yellow" + shape, new Point(cx, cy), HersheyFonts.HersheySimplex, 0.5, Scalar.Yellow, 2);
                    //화면에 표기 출력할놈 //텍스트           //좌표            //폰트                             글자색
                    yel++;
                    cnt++;//개수 구별용
                }
            }
            foreach (var c in contours2)
            {
                var area = Cv2.ContourArea(c);
                if (area > 8000)
                {
                    Moments m = Cv2.Moments(c);
                    Point pnt = new Point(m.M10 / m.M00, m.M01 / m.M00); //center point
                    Cv2.DrawContours(frame, new[] { c }, -1, Scalar.Green, 3);
                    string shape = GetShape(c); //*형상구분                   
                    var M = Cv2.Moments(c);
                    var cx = (int)(M.M10 / M.M00);
                    var cy = (int)(M.M01 / M.M00);
                    tor = "Green_" + shape;
                    Cv2.PutText(frame, pnt + "Green " + shape, new Point(cx, cy), HersheyFonts.HersheySimplex, 0.5, Scalar.Green, 2);
                    gre++;
                    cnt++;//개수 구별용
                }
            }
            foreach (var c in contours3)
            {
                var area = Cv2.ContourArea(c);
                if (area > 8000)
                {
                    Moments m = Cv2.Moments(c);
                    Point pnt = new Point(m.M10 / m.M00, m.M01 / m.M00); //center point
                    Cv2.DrawContours(frame, new[] { c }, -1, Scalar.Red, 3);
                    string shape = GetShape(c); //*형상구분
                    var M = Cv2.Moments(c);
                    var cx = (int)(M.M10 / M.M00);
                    var cy = (int)(M.M01 / M.M00);
                    tor = pnt + "Red_" + shape;
                    Cv2.PutText(frame, pnt + " Red " + shape, new Point(cx, cy), HersheyFonts.HersheySimplex, 0.5, Scalar.Red, 2);
                    red++;
                    cnt++;//개수 구별용
                }
            }
            foreach (var c in contours4)
            {
                var area = Cv2.ContourArea(c);
                if (area > 8000)
                {

                    Moments m = Cv2.Moments(c);
                    Point pnt = new Point(m.M10 / m.M00, m.M01 / m.M00); //center point

                    Cv2.DrawContours(frame, new[] { c }, -1, Scalar.Blue, 3);
                    string shape = GetShape(c); //*형상구분

                    var M = Cv2.Moments(c);
                    var cx = (int)(M.M10 / M.M00);
                    var cy = (int)(M.M01 / M.M00);
                    tor = "Blue_" + shape;
                    Cv2.PutText(frame, pnt + " Blue " + shape, new Point(cx, cy), HersheyFonts.HersheySimplex, 0.5, Scalar.Blue, 2);
                    blu++;//색
                    cnt++;//개수 구별용
                }

            }
            Cam.Source = OpenCvSharp.WpfExtensions.WriteableBitmapConverter.ToWriteableBitmap(frame);
        }
        private void Work()
        {
            switch (work_detail)
            {
                case "1/":
                    if (shape == "Pentagon" && tor == "Red" && cnt == 4)//조건이랑 맞다면
                    {
                        string message = "2 / Red Square / 2023 - 01 - 23 - 16 - 32 - 12 / 1번라인 / 박철두 / pass";
                        byte[] data = Encoding.Default.GetBytes(message);

                        start_page.stream = start_page.client.GetStream();
                        start_page.stream.Write(data, 0, data.Length);
                    }
                    else //오류일때 캡처하고 전송하기
                    {
                        
                        string save_1 = DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss"); //캡처한거 저장
                        Cv2.ImWrite("../../" + save_1 + ".png", frame);//파일 이름설정
                        try
                        {
                            string img_message = "4/오류 사진 보냅니다.";
                            //stream null값인지 확인해보기
                            byte[] errorReport = Encoding.Default.GetBytes(img_message);
                            start_page.stream.Write(errorReport, 0, errorReport.Length);//4/오류사진

                            //데이터 크기 보내고
                            byte[] size = new byte[4];
                            FileStream filestr = new FileStream("../../" + save_1 + ".png", FileMode.Open, FileAccess.Read);
                            int fileLength = (int)filestr.Length;
                            size = Encoding.Default.GetBytes(fileLength.ToString());
                            //string a = fileLength.ToString();
                            //size = Encoding.Default.GetBytes(a);
                            start_page.stream.Write(size, 0, 4);
                            MessageBox.Show("송신 데이터 크기 : " + BitConverter.ToInt32(size, 0).ToString());

                            byte[] imageData = new byte[fileLength];

                            filestr.Read(imageData, 0, imageData.Length);//파일읽어서 배열에 넣고
                            start_page.stream.Write(imageData, 0, imageData.Length);//송신

                            MessageBox.Show("파일전송완료");
                            filestr.Close();//닫기추가
                        }
                        catch (SocketException file)
                        {
                            Console.Write(file);
                            System.Windows.MessageBox.Show(file.ToString());
                        }
                        
                    }
                    break;

                case "2/":

                    break;

                case "3/":

                    break;

                case "4/":

                    break;

                default:
                    break;
            }
        }

        
    }
}
