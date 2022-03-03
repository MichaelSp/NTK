using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Automation;
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
        public static string AppRoot = $@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\NTK";

        public string LogsFolder = $@"{AppRoot}\logs";

        public TimeConsumed TimeConsumed;

        public Config Config;

        public FileInfo timeConsumedFile;

        public FileInfo configFile;

        public DispatcherTimer DispatcherTimer;

        public DispatcherTimer CountdownTimer;

        public Timer UdpTimer;

        public Timer BlockSitesTimer;

        private Timer ServerTimer { get; set; }


        private ServerRequest ServerRequest = new ServerRequest();

        private const int PORT = 6868;

        public UdpClient UdpClient = new UdpClient();

        public TimeSpan CountDownTime;

        public int CurrentRunningTime = 1;

        public const string SHOWUI = "SHOW_UI";

        public const string HIDEUI = "HIDE_UI";

        public const string BLOCKTODAY = "BLOCK";

        public const string RESETTIME = "RESET_TIME";

        public const string ADDTIME = "ADD_TIME";

        public const string PRANK = "PRANK";

        public List<string> BlockSites = new List<string>();

        public Startup()
        {
            InitializeComponent();
            FixFolders();
            CheckConfig();
            CheckLogin();
            ReadBlockSites();
            UdpClient.Client.Bind(new IPEndPoint(IPAddress.Any, PORT));
            DispatcherTimer = new DispatcherTimer();
            DispatcherTimer.Interval = TimeSpan.FromSeconds(1);
            DispatcherTimer.Tick += TimerTick_Tick;
            DispatcherTimer.Start();

            ServerTimer = new Timer();
            ServerTimer.Interval = 500;
            ServerTimer.Elapsed += ServerTimerElapsed;
            ServerTimer.Start();

            CountDownTime = TimeSpan.FromSeconds(60);
            CountdownTimer = new DispatcherTimer();
            CountdownTimer.Interval = TimeSpan.FromSeconds(1);
            CountdownTimer.Tick += CountdownTimer_Tick;

            BlockSitesTimer = new Timer();
            BlockSitesTimer.Interval = 1000;
            BlockSitesTimer.Elapsed += BlockSitesTimerElapsed;
            BlockSitesTimer.Start();

            UdpTimer = new Timer();
            UdpTimer.Interval = 10000;
            UdpTimer.Elapsed += UdpTimerElapsed;
            UdpTimer.Start();
        }

        private void UdpTimerElapsed(object sender, ElapsedEventArgs e)
        {
            HelloEvent evt = new HelloEvent();
            evt.Uptime = TimeConsumed.Time.ToString();
            evt.User = System.Security.Principal.WindowsIdentity.GetCurrent().Name;

            byte[] sendBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(evt).ToCharArray());

            Console.WriteLine(sendBytes.ToString());
            try
            {
                UdpClient.Send(sendBytes, sendBytes.Length, "255.255.255.255", 6868);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void BlockSitesTimerElapsed(object sender, ElapsedEventArgs e)
        {
            Process[] procsEdge = Process.GetProcessesByName("msedge");
            foreach (Process Edge in procsEdge)
            {
                if (Edge.MainWindowHandle != IntPtr.Zero)
                {
                    AutomationElement root = AutomationElement.FromHandle(Edge.MainWindowHandle);
                    var tabs = root.FindAll(TreeScope.Descendants, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.TabItem));
                    var elmUrl = root.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.NameProperty, "Address and search bar"));
                    foreach (AutomationElement tabitem in tabs)
                    {
                        if (elmUrl != null)
                        {
                            AutomationPattern[] patterns = elmUrl.GetSupportedPatterns();
                            if (patterns.Length > 0)
                            {
                                ValuePattern val = (ValuePattern)elmUrl.GetCurrentPattern(patterns[0]);
                                string url = val.Current.Value;
                                bool block = BlockSites.Contains(url);
                                if (block)
                                {
                                    this.Dispatcher.Invoke(() =>
                                    {
                                        Block();
                                    });
                                }
                            }
                        }
                    }
                }
            }
        }

        public void ReadBlockSites()
        {
            if (File.Exists($@"{Startup.AppRoot}\block.txt"))
            {
                BlockSites.AddRange(File.ReadLines($@"{Startup.AppRoot}\block.txt"));
            }
        }

        private void ServerTimerElapsed(object sender, ElapsedEventArgs e)
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
                    this.Dispatcher.Invoke(() =>
                    {
                        ProcessIt(ServerRequest);
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("error : " + ex.Message);
            }
        }

        private void Block()
        {
            FrameGrid.Background = new SolidColorBrush(Colors.White);
            this.lblMessage.Visibility = Visibility.Visible;
            this.lblMessage.Content = $"you are block because of entering prohibited web sites.";
            this.lblShutdownTimer.Visibility = Visibility.Hidden;
            this.Visibility = Visibility.Visible;
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
                TimeConsumed = new TimeConsumed() { Time = Config.Limit };
                UpdateTimeConsumed(TimeConsumed);
            }
            else if (serverRequest.Action == ADDTIME)
            {
                int time = Int32.Parse(serverRequest.Message);
                TimeConsumed = new TimeConsumed() { Time = (TimeConsumed.Time - time) };
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
            lblShutdownTimer.Content = $"Computer Will Shutdown in {CountDownTime.ToString("c")}";
            if (CountDownTime == TimeSpan.Zero)
            {
                Process.Start("Shutdown", "-s -t 1");
            }
            CountDownTime = CountDownTime.Add(TimeSpan.FromSeconds(-1));
        }

        /// <summary>
        /// Check Configuration File config.json.
        /// </summary>
        public void CheckConfig()
        {
            configFile = new FileInfo($@"{AppRoot}\ntk-config.json");
            if (configFile.Exists)
            {
                Config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(configFile.FullName));
                lblMessage.Content = Config.Message;
            }
            else
            {
                Config = new Config() { Limit = 10800, Message = "You have used your alloted time for today, Come back tomorrow." };
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
            new StreamWriter($"{LogsFolder}/{DateTime.Now.ToString("MM_dd_yyyy")}.log").WriteLine("Application Start");
        }

        /// <summary>
        /// Check login, if exist just consume the save file, if not then create a new file.
        /// </summary>
        public void CheckLogin()
        {
            timeConsumedFile = new FileInfo($"{LogsFolder}/{DateTime.Now.ToString("MM_dd_yyyy")}.json");
            if (timeConsumedFile.Exists)
            {
                TimeConsumed = JsonConvert.DeserializeObject<TimeConsumed>(File.ReadAllText(timeConsumedFile.FullName));
                if (TimeConsumed == null)
                {
                    TimeConsumed = new TimeConsumed() { Time = Config.Limit / 2 };
                    UpdateTimeConsumed(TimeConsumed);
                }
                else
                {
                    CurrentRunningTime = TimeConsumed.Time;
                }
            }
            else
            {
                TimeConsumed = new TimeConsumed() { Time = 0 };
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

            CurrentRunningTime++;
            TimeConsumed = new TimeConsumed() { Time = CurrentRunningTime };
            if (CurrentRunningTime > Config.Limit)
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
    public class HelloEvent
    {
        public string User { get; set; }

        public string Uptime { get; set; }

    }
}
