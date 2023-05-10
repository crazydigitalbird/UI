namespace UI.Models
{
    public class Post
    {
        public int Id { get; set; }

        public long idMessage { get; set; }

        public DateTime DateSent { get; set; }

        public int DiscountPercent { get; set; }

        public bool IsPurchased { get; set; }

        public decimal Price { get; set; }

        public int PriceWithDiscount { get; set; }

        public string TextPreview { get; set; }

        public List<PostMedia> Photos { get; set; } = new List<PostMedia>();

        public List<PostMedia> Videos { get; set; } = new List<PostMedia>();
    }

    public class PostMedia
    {
        public int Id { get; set; }

        public int Duration { get; set; }

        public string Url { get; set; }
    }

    public class LimitPost
    {
        public bool AllowSendPost { get; set; }

        public List<string> PhotoForbiddenTags { get; set; }

        public List<string> postPhotoForbiddenTags { get; set; }

        public List<string> postVideoForbiddenTags { get; set; }

        public List<string> videoForbiddenTags { get; set; }
    }
}
