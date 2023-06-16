namespace UI.Models
{
    public class MessageTimer
    {
        public DateTime DateViewed { get; set; }

        public DateTime DateUTC { get; set; }

        public MessageType MessageType { get; set; }

        public int TimeLeft()
        {
            var timeLeftSeconds = (DateUTC - DateViewed).TotalSeconds;
            switch (MessageType)
            {
                case MessageType.System:
                    if(timeLeftSeconds > TimeSpan.FromMinutes(5).TotalSeconds)
                    {
                        return 0;
                    }
                    return (int)(TimeSpan.FromMinutes(5).TotalSeconds - timeLeftSeconds);

                default:
                    if (timeLeftSeconds > TimeSpan.FromMinutes(2).TotalSeconds)
                    {
                        return 0;
                    }
                    return (int)(TimeSpan.FromMinutes(2).TotalSeconds - timeLeftSeconds);
            }
        }        
    }
}
