using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Timers;
using System.Windows.Threading;

namespace NTK
{
    public interface TimeControl : ShowMessage
    {
        public int TimeConsumed { get; }

        public int GetTimeAllowed();

        public void UpdateTimeConsumed(int newTime);

        public void UpdateTimeAllowed(int newTime);
    }

    public class UDPServer : DispatcherObject
    {
        public const string SHOWUI = "SHOW_UI";

        public const string HIDEUI = "HIDE_UI";

        public const string BLOCKTODAY = "BLOCK";

        public const string RESETTIME = "RESET_TIME";

        public const string ADDTIME = "ADD_TIME";

        public const string SHOW_IMAGE = "SHOW_IMAGE";

        public Timer UdpTimer;

        private Timer ServerTimer { get; set; }
        public TimeControl ui { get; }

        private ServerRequest ServerRequest = new ServerRequest();

        private const int PORT = 6868;

        public UdpClient UdpClient = new UdpClient();



        public UDPServer(TimeControl timeControl)
        {
            UdpClient.Client.Bind(new IPEndPoint(IPAddress.Any, PORT));

            ServerTimer = new Timer();
            ServerTimer.Interval = 500;
            ServerTimer.Elapsed += ServerTimerElapsed;
            ServerTimer.Start();

            UdpTimer = new Timer();
            UdpTimer.Interval = 10000;
            UdpTimer.Elapsed += UdpTimerElapsed;
            UdpTimer.Start();
            ui = timeControl;
        }

        private void UdpTimerElapsed(object sender, ElapsedEventArgs e)
        {
            HelloEvent evt = new HelloEvent();
            evt.Uptime = this.ui.TimeConsumed.ToString();
            evt.AllowedTime = this.ui.GetTimeAllowed().ToString();
            evt.User = System.Security.Principal.WindowsIdentity.GetCurrent().Name;

            string jsonString = JsonConvert.SerializeObject(evt);
            byte[] sendBytes = Encoding.UTF8.GetBytes(jsonString.ToCharArray());

            Console.WriteLine(">" + jsonString);
            try
            {
                UdpClient.Send(sendBytes, sendBytes.Length, "255.255.255.255", 6868);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void ProcessIt(ServerRequest serverRequest)
        {
            if (serverRequest.Action == SHOWUI)
            {
                this.ui.ShowMessage(serverRequest.Message);
            }
            else if (serverRequest.Action == HIDEUI)
            {
                this.ui.HideMessage();
            }
            else if (serverRequest.Action == RESETTIME)
            {
                this.ui.UpdateTimeConsumed(0);
            }
            else if (serverRequest.Action == BLOCKTODAY)
            {
                this.ui.UpdateTimeConsumed(this.ui.GetTimeAllowed());
            }
            else if (serverRequest.Action == ADDTIME)
            {
                int time = Int32.Parse(serverRequest.Message);
                this.ui.UpdateTimeAllowed(this.ui.GetTimeAllowed() + time);
            }
            else if (serverRequest.Action == SHOW_IMAGE)
            {
                this.ui.ShowImageFromUrl(new Uri(serverRequest.Message));
            }
        }


        private void ServerTimerElapsed(object sender, ElapsedEventArgs e)
        {
             try
            {
                var from = new IPEndPoint(0, PORT);
                var recvBuffer = UdpClient.Receive(ref from);
                string dataBuff = (Encoding.UTF8.GetString(recvBuffer));
                ServerRequest content = JsonConvert.DeserializeObject<ServerRequest>(dataBuff);

                if (content.Action != "")
                {
                    Console.WriteLine("<" + dataBuff);
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
    }
}