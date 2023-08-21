using Core.Models.Balances;

namespace UI.Models
{
    public class AdminAgencyStatistics
    {
        public int SheetsCount { get; set; }

        public int SheetsIncrement { get; set; }

        public int SheetsCountInWork { get; set; }

        public int NumberMessagesSent { get; set; }

        public int NumberMessagesSentIncrement { get; set; }

        public int NumberMailsSent { get; set; }

        public int NumberMailsSentIncrement { get; set; }

        public decimal Balance { get; set; }

        public decimal BalanceIncrement { get; set; }

        public decimal BalanceToday { get; set; }

        public decimal BalanceTodayIncrement { get; set; }

        public AverageResponseTime AverageResponseTime { get; set; }

        public string Downtime { get; set; }

        public string DowntimeIncrement { get; set; }

        public decimal[] BalancesLastMonth { get; set; }

        public decimal[] BalancesCurrentMonth { get; set; }

        public Dictionary<string, BalanceType> BalancesType { get; set; } = new Dictionary<string, BalanceType>
        {
            { "chat_message", new BalanceType{ CSSClass = "message-chart", Text = "Чаты" } },
            { "chat_photo", new BalanceType{ CSSClass = "photo-chart", Text = "Чаты фото" } },
            { "chat_video", new BalanceType{ CSSClass = "video-chart", Text = "Чаты видео" } },
            { "chat_audio", new BalanceType{ CSSClass = "audio-chart", Text = "Чаты аудио" } },
            { "chat_sticker", new BalanceType{ CSSClass = "sticker-chart", Text = "Стикеры" } },
            { "read_mail", new BalanceType{ CSSClass = "mail-chart", Text = "Письма" } },
            { "Другое", new BalanceType{ CSSClass = "other-chart", Text = "Другое" } }
        };

        public List<AgencyBalanceStatistic> OperatorAgencyBalanceStatistics { get; set; }

        public List<ApplicationUser> AdminSAgency { get; set; }
    }

    public class BalanceType
    {
        public int ChangePercent { get; set; }

        public string CSSClass { get; set; }

        public string Text { get; set; }
    }
}
