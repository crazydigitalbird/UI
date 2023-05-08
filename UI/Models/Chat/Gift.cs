using System.Text.Json.Serialization;

namespace UI.Models
{
    public class GiftData
    {
        public string Cursor { get; set; }

        [JsonPropertyName("items")]
        public List<Gift> Gifts { get; set; }
    }

    public class Gift
    {
        public int Id { get; set; }

        public string ImageSrc { get; set; }

        public string Name { get; set; }

        public object AnimationSrc { get; set; }

        public CategoryGifts Category { get; set; }

        public decimal Cost { get; set; }
    }

    public class CategoryGifts
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class LimitGifts
    {
        public int Limit { get; set; }
    }
}
