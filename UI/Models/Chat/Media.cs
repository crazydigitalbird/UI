namespace UI.Models
{
    public class Media
    {
        public string Cursor { get; set; }

        public List<MediaPhoto> Photos { get; set; }

        public List<MediaVideo> Videos { get; set; }
    }

    public class MediaVideo
    {
        public int IdVideo{ get; set; }
        public int idUser { get; set; }
        public int Duration { get; set; }
        public string Comment { get; set; }
        public List<StatusMediaPhoto> DeclineReasons { get; set; }
        public StatusMediaPhoto Status { get; set; }
        public List<StatusMediaPhoto> Tags { get; set; }
        public UrlsMediaVideo Urls {get; set;}
    }

    public class MediaPhoto
    {
        public int IdPhoto { get; set; }
        public int idUser { get; set; }
        public string Comment { get; set; }
        public List<StatusMediaPhoto> DeclineReasons { get; set; }
        public StatusMediaPhoto Status { get; set; }
        public List<StatusMediaPhoto> Tags { get; set; }
        public UrlsMediaPhoto Urls { get; set; }
    }

    public class StatusMediaPhoto
    {
        public string Code { get; set; }
        public string Description { get; set; }
    }

    public class UrlsMediaPhoto
    {
        public string UrlOriginal { get; set; }
        public string UrlPreview { get; set; }
        public string UrlStandard { get; set; }
    }

    public class UrlsMediaVideo
    {
        public string UrlMp4Hd { get; set; }
        public string UrlMp4Sd { get; set; }
        public string UrlThumbnail { get; set; }
    }
}
