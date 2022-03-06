namespace NTK {
    
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

        /// <summary>
        /// Show a small window that counts down the remaining time
        /// </summary>
        public bool ShowTimerWindow{ get; set; } 
    }
    
}