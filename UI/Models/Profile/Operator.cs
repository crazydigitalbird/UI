namespace UI.Models
{
    public class Operator
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Time { get; set; }

        public string Team { get; set; }

        public List<Profile> Profiles { get; set; }
    }
}
