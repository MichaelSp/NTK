using System;
using System.Windows;
using System.Windows.Forms;

namespace NTK
{

    public partial class TimerWindow : Window
    {
        internal TimerWindow(){
            InitializeComponent();
            Top = Screen.PrimaryScreen.WorkingArea.Height - 350;
        }

        internal void setTimeRemaining(int timeRemaining)
        {
            this.lblMessage.Content =TimeSpan.FromSeconds(timeRemaining).ToString("c");
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
}