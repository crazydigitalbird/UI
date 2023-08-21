namespace UI.Models
{
    public class OriginalUrlPhoto
    {
        public OriginalUrlPhotoData Data { get; set; }
    }

    public class OriginalUrlPhotoData 
    {
        public string Url { get; set; }
    }

    public class OriginalUrlVideo
    {
        public List<OriginalUrlVideoData> Data { get; set; }
    }

    public class OriginalUrlVideoData
    {
        public string Src { get; set; }

        public string Label { get; set; }
    }
}
