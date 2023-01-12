namespace UI.Models
{
    public class Note
    {
        public string Name { get; set; }

        public DateTime Created { get; set; } = DateTime.Now;

        public string Text { get; set; }

        public bool IsRead { get; set; }
    }
}
