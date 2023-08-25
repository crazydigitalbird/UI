using System.Text.Json.Serialization;

namespace UI.Models
{
    public class AverageResponseTime
    {
        [JsonPropertyName("average_time_current_month")]
        public double AverageTimeCurrentMonth { get; set; }

        [JsonPropertyName("average_time_previous_month")]
        public double AverageTimePreviousMonth { get; set; }

        public override string ToString()
        {
            var averageTimeCurrentMonthTimeSpan = TimeSpan.FromSeconds(AverageTimeCurrentMonth);
            return new DateTime(averageTimeCurrentMonthTimeSpan.Ticks).ToString("m мин. ss сек.");
        }

        public string Increment()
        {
            if (AverageTimePreviousMonth != 0)
            {
                int increment = (int)(AverageTimeCurrentMonth * 100 / AverageTimePreviousMonth - 100);
                if (increment > 0)
                {
                    return $"+{increment}";
                }
                else if (increment < 0)
                {
                    return $"-{increment}";
                }
            }
            return "0";
        }
    }
}
