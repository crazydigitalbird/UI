using Core.Models.Balances;

namespace UI.Models
{
    public class BalanceStatisticAgency
    {
        public decimal Balance { get; set; }

        public decimal BalanceIncrement { get; set; }

        public decimal BalanceToday { get; set; }

        public decimal BalanceTodayIncrement { get; set; }

        public decimal[] BalancesLastMonth { get; set; }

        public decimal[] BalancesCurrentMonth { get; set; }

        public List<AgencyBalanceStatistic> Operators { get; internal set; }

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
    }
}
