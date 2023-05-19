namespace UI.Models
{
    public class MessageTimer
    {
        public DateTime DateViewed { get; set; }

        //public DateTime DateUTC { get; set; }

        public MessageType MessageType { get; set; }

        public int TimeLeft()
        {
            var timeLeftSeconds = (DateTime.UtcNow - DateViewed).TotalSeconds;
            switch (MessageType)
            {
                case MessageType.System:
                    if(timeLeftSeconds > TimeSpan.FromMinutes(5).TotalSeconds)
                    {
                        return 0;
                    }
                    break;

                default:
                    if (timeLeftSeconds > TimeSpan.FromMinutes(2).TotalSeconds)
                    {
                        return 0;
                    }
                    break;
            }
            return (int)timeLeftSeconds;
        }        
    }
}
