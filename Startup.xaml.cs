using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace NTK
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Startup : Window
    {
        public string LogsFolder = @"logs";

        public TimeConsumed TimeConsumed;

        public Config Config;

        public FileInfo timeConsumedFile;

        public FileInfo configFile;

        public DispatcherTimer DispatcherTimer;

        public DispatcherTimer CountdownTimer;

        private System.Timers.Timer Timer { get; set; }

        private ServerRequest ServerRequest = new ServerRequest();

        private const int PORT = 6868;

        public UdpClient UdpClient = new UdpClient();

        public TimeSpan countDownTime;

        public int currentRunningTime = 1;

        public const string SHOWUI = "SHOW_UI";

        public const string HIDEUI = "HIDE_UI";

        public const string BLOCKTODAY = "BLOCK";

        public const string RESETTIME = "RESET_TIME";

        public const string ADDTIME = "ADD_TIME";

        public const string PRANK = "PRANK";

        public Startup()
        {
            InitializeComponent();
            FixFolders();
            CheckConfig();
            CheckLogin();
            UdpClient.Client.Bind(new IPEndPoint(IPAddress.Any, PORT));
            DispatcherTimer = new DispatcherTimer();
            DispatcherTimer.Interval = TimeSpan.FromSeconds(1);
            DispatcherTimer.Tick += TimerTick_Tick;
            DispatcherTimer.Start();

            Timer = new System.Timers.Timer();
            Timer.Interval = 500;
            Timer.Elapsed += Timer_Elapsed;
            Timer.Start();

            countDownTime = TimeSpan.FromSeconds(60);
            CountdownTimer = new DispatcherTimer();
            CountdownTimer.Interval = TimeSpan.FromSeconds(1);
            CountdownTimer.Tick += CountdownTimer_Tick;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                var from = new IPEndPoint(0, 0);
                var recvBuffer = UdpClient.Receive(ref from);
                string dataBuff = (Encoding.UTF8.GetString(recvBuffer));
                ServerRequest content = JsonConvert.DeserializeObject<ServerRequest>(dataBuff);

                if (content.UUIDv4 != ServerRequest.UUIDv4)
                {
                    ServerRequest = content;
                    this.Dispatcher.Invoke(() => {
                        ProcessIt(ServerRequest);
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("error : " + ex.Message);
            }
        }

        private void ProcessIt(ServerRequest serverRequest)
        {
            if (serverRequest.Action == SHOWUI)
            {
                FrameGrid.Background = new SolidColorBrush(Colors.White);
                this.lblMessage.Visibility = Visibility.Visible;
                this.lblShutdownTimer.Visibility = Visibility.Hidden;
                this.Visibility = Visibility.Visible;
                this.lblMessage.Content = serverRequest.Message;
            }
            else if (serverRequest.Action == HIDEUI)
            {
                FrameGrid.Background = new SolidColorBrush(Colors.White);
                this.lblMessage.Visibility = Visibility.Hidden;
                this.lblShutdownTimer.Visibility = Visibility.Hidden;
                this.Visibility = Visibility.Hidden;
                this.lblMessage.Content = "";
            }
            else if (serverRequest.Action == RESETTIME)
            {
                TimeConsumed = new TimeConsumed() { Time = 0 };
                UpdateTimeConsumed(TimeConsumed);
            } 
            else if (serverRequest.Action == BLOCKTODAY)
            {
                TimeConsumed = new TimeConsumed() { Time = Config.Limit};
                UpdateTimeConsumed(TimeConsumed);
            } else if (serverRequest.Action == ADDTIME)
            {
                int time = Int32.Parse(serverRequest.Message);
                TimeConsumed = new TimeConsumed() { Time = (TimeConsumed.Time - time)};
                UpdateTimeConsumed(TimeConsumed);
            } 
            else if (serverRequest.Action == PRANK)
            {
                this.Visibility = Visibility.Visible;
                this.lblMessage.Visibility = Visibility.Hidden;
                this.lblShutdownTimer.Visibility = Visibility.Hidden;
                string url = serverRequest.Message;
                FrameGrid.Background = new SolidColorBrush(Colors.Black);
                ImageBrush b = new ImageBrush();
                b.ImageSource = new BitmapImage(new Uri(url));
                FrameGrid.Background = b;
            }
        }

        private void CountdownTimer_Tick(object sender, EventArgs e)
        {
            FrameGrid.Background = new SolidColorBrush(Colors.White);
            lblShutdownTimer.Visibility = Visibility.Visible;
            lblMessage.Visibility = Visibility.Visible;
            lblMessage.Content = Config.Message;
            lblShutdownTimer.Content = $"Computer Will Shutdown in {countDownTime.ToString("c")}";
            if (countDownTime == TimeSpan.Zero)
            {
                Process.Start("Shutdown", "-s -t 1");
            }
            countDownTime = countDownTime.Add(TimeSpan.FromSeconds(-1));
        }

        /// <summary>
        /// Check Configuration File config.json.
        /// </summary>
        public void CheckConfig()
        {
            configFile = new FileInfo(@"ntk-config.json");
            if (configFile.Exists)
            {
                Config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(configFile.FullName));
                lblMessage.Content = Config.Message;
            } 
            else
            {
                Config = new Config() { Limit= 10800, Message= "You have used your alloted time for today, Come back tomorrow." };
                JsonSerializer serializer = new JsonSerializer();
                serializer.NullValueHandling = NullValueHandling.Ignore;
                serializer.Formatting = Formatting.Indented;
                using (StreamWriter sw = new StreamWriter(configFile.FullName))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, Config);
                    writer.Close();
                    sw.Close();
                }
            }
        }

        /// <summary>
        /// Create logs folders if not exist.
        /// </summary>
        public void FixFolders()
        {
            if (!Directory.Exists(LogsFolder))
            {
                Directory.CreateDirectory(LogsFolder);
            }
            if (!Directory.Exists($@"{LogsFolder}/{Environment.UserName}"))
            {
                Directory.CreateDirectory($@"{LogsFolder}/{Environment.UserName}");
            }
        }

        /// <summary>
        /// Check login, if exist just consume the save file, if not then create a new file.
        /// </summary>
        public void CheckLogin()
        {
            timeConsumedFile = new FileInfo($"{LogsFolder}/{Environment.UserName}/{DateTime.Now.ToString("MM_dd_yyyy")}.json");
            if (timeConsumedFile.Exists)
            {
                TimeConsumed = JsonConvert.DeserializeObject<TimeConsumed>(File.ReadAllText(timeConsumedFile.FullName));
                if (TimeConsumed == null)
                {
                    TimeConsumed = new TimeConsumed() { Time = Config.Limit/2 };
                    UpdateTimeConsumed(TimeConsumed);
                }
                else
                {
                    currentRunningTime = TimeConsumed.Time;
                }
            }
            else
            {
                TimeConsumed = new TimeConsumed() { Time = 0};
                UpdateTimeConsumed(TimeConsumed);
            }
        }

        /// <summary>
        /// Update Time Consumed File
        /// </summary>
        /// <param name="conf">the time consumed</param>
        public void UpdateTimeConsumed(TimeConsumed conf)
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.NullValueHandling = NullValueHandling.Ignore;
            serializer.Formatting = Formatting.Indented;
            using (StreamWriter sw = new StreamWriter(timeConsumedFile.FullName))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, conf);
                writer.Close();
                sw.Close();
            }
        }

        /// <summary>
        /// Updates running time every seconds. if time exceeds the time limit, display the UI and start the countdown timer.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimerTick_Tick(object sender, EventArgs e)
        {

            currentRunningTime++;
            TimeConsumed = new TimeConsumed() { Time = currentRunningTime};
            if (currentRunningTime > Config.Limit)
            {
                this.Visibility = Visibility.Visible;
                CountdownTimer.Start();
                DispatcherTimer.Stop();
            }
            UpdateTimeConsumed(TimeConsumed);
        }

        /// <summary>
        /// Prevents the user to cancel the window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
        }

    }
    public class TimeConsumed
    {
        /// <summary>
        /// The current time
        /// </summary>
        public int Time { get; set; }
    }

    public class Config
    {
        /// <summary>
        /// The time limits
        /// </summary>
        public int Limit { get; set; }

        /// <summary>
        /// The message
        /// </summary>
        public string Message { get; set; }
    }

    public class ServerRequest
    {
        public string UUIDv4 { get; set; }
        public string Message { get; set; }

        public string Action { get; set; }

    }
}
