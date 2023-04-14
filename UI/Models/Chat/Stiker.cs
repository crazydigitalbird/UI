namespace UI.Models
{
    public class StickerGroup
    {
        public string Name { get; set; }

        public List<Stiker> Stickers { get; set; }
    }
    public class Stiker
    {
        public int Id { get; set; }

        public string Url { get; set; }
    }
}
