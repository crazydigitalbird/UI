namespace UI.Models
{
    public class Cabinet
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public ICollection<Profile> Profiles { get; set; }

        public ICollection<ApplicationUser> Users { get; set; }
    }
}
