namespace UI.Models
{
    public class Profile
    {
        public int Id { get; set; }

        public Status Status { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public int Balance { get; set; }

        public int Photo { get; set; }

        public int Video { get; set; }

        public int Audio { get; set; }

        public int Operators { get; set; }

        public Group Group { get; set; }

        public Shift Shift { get; set; }

        public Cabinet Cabinet { get; set; }
    }
}
