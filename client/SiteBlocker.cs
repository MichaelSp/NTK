using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Timers;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Media;
using System.Windows.Threading;

namespace NTK
{

    public interface ShowMessage {
        public void ShowMessage(string message);
        public void HideMessage();
        
        void ShowImageFromUrl(Uri uri);
    }

    public class SiteBlocker : DispatcherObject
    {

        public Timer BlockSitesTimer;
        public List<string> BlockSites = new List<string>();

        private ShowMessage ui;


        public SiteBlocker(ShowMessage ui)
        {
            this.ReadBlockSites();

            this.ui = ui;
            BlockSitesTimer = new Timer();
            BlockSitesTimer.Interval = 1000;
            BlockSitesTimer.Elapsed += BlockSitesTimerElapsed;
            BlockSitesTimer.Start();

        }
        
        private void ReadBlockSites()
        {
            if (File.Exists($@"{Startup.AppRoot}\block.txt"))

            {
                BlockSites.AddRange(File.ReadLines($@"{Startup.AppRoot}\block.txt"));
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
                                        this.ui.ShowMessage( $"you are block because of entering prohibited web sites.");
                                    });
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}