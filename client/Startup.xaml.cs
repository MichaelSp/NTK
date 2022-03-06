using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace NTK
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Startup : Window, ShowMessage, TimeControl
    {
        public static string AppRoot = $@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\NTK";

        public string LogsFolder = $@"{AppRoot}\logs";

        int _timeConsumed = 0;
        public int TimeConsumed { get { return _timeConsumed; } }

        public Config Config;

        public FileInfo timeConsumedFile;

        public FileInfo configFile;

        public DispatcherTimer DispatcherTimer;

        public DispatcherTimer CountdownTimer;

        public TimeSpan CountDownTime;

        public int CurrentRunningTime = 1;

        private SiteBlocker siteBlocker;
        private UDPServer udpServer;

        public Startup()
        {
            InitializeComponent();
            FixFolders();
            CheckConfig();
            CheckLogin();

            siteBlocker = new SiteBlocker(this);
            udpServer = new UDPServer(this);

            DispatcherTimer = new DispatcherTimer();
            DispatcherTimer.Interval = TimeSpan.FromSeconds(1);
            DispatcherTimer.Tick += TimerTick_Tick;
            DispatcherTimer.Start();

            CountDownTime = TimeSpan.FromSeconds(60);
            CountdownTimer = new DispatcherTimer();
            CountdownTimer.Interval = TimeSpan.FromSeconds(1);
            CountdownTimer.Tick += CountdownTimer_Tick;
        }

        public void ShowMessage(string message)
        {
            FrameGrid.Background = new SolidColorBrush(Colors.White);
            this.lblMessage.Visibility = Visibility.Visible;
            this.lblMessage.Content = message;
            this.lblShutdownTimer.Visibility = Visibility.Hidden;
            this.Visibility = Visibility.Visible;
        }

        public void HideMessage()
        {
            FrameGrid.Background = new SolidColorBrush(Colors.White);
            this.lblMessage.Visibility = Visibility.Hidden;
            this.lblShutdownTimer.Visibility = Visibility.Hidden;
            this.Visibility = Visibility.Hidden;
            this.lblMessage.Content = "";
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
                writeConfig();
            }
        }

        private void writeConfig()
        {
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
                TimeConsumed consumed = JsonConvert.DeserializeObject<TimeConsumed>(File.ReadAllText(timeConsumedFile.FullName));
                if (consumed == null)
                {
                    UpdateTimeConsumed(Config.Limit / 2);
                }
                else
                {
                    CurrentRunningTime = TimeConsumed;
                }
            }
            else
            {
                UpdateTimeConsumed(0);
            }
        }

        /// <summary>
        /// Update Time Consumed File
        /// </summary>
        /// <param name="newTime">the time consumed</param>
        public void UpdateTimeConsumed(int newTime)
        {
            this._timeConsumed = newTime;
            JsonSerializer serializer = new JsonSerializer();
            serializer.NullValueHandling = NullValueHandling.Ignore;
            serializer.Formatting = Formatting.Indented;
            using (StreamWriter sw = new StreamWriter(timeConsumedFile.FullName))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, new TimeConsumed() { Time = newTime });
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
            if (CurrentRunningTime > GetTimeAllowed())
            {
                this.Visibility = Visibility.Visible;
                CountdownTimer.Start();
                DispatcherTimer.Stop();
            }
            UpdateTimeConsumed(CurrentRunningTime);
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

        public void UpdateTimeAllowed(int newTime)
        {
            this.Config.Limit = newTime;
            writeConfig();
        }

        public int GetTimeAllowed()
        {
            return Config.Limit;
        }

        public void ShowImageFromUrl(Uri uri)
        {
            this.Visibility = Visibility.Visible;
            this.lblMessage.Visibility = Visibility.Hidden;
            this.lblShutdownTimer.Visibility = Visibility.Hidden;
            FrameGrid.Background = new SolidColorBrush(Colors.Black);
            ImageBrush b = new ImageBrush();
            b.ImageSource = new BitmapImage(uri);
            FrameGrid.Background = b;
        }

        int TimeControl.GetTimeAllowed()
        {
            return Config.Limit;
        }
    }
}
