using System.Text.Json.Serialization;

namespace UI.Models
{
    public class SheetInfo
    {
        public int Id { get; set; }

        public Personal Personal { get; set; }
    }

    public class Personal
    {
        [JsonPropertyName("Avatar_xl")]
        public string Avatar { get; set; }

        public string LastName { get; set; }

        public string Name { get; set; }
    }
}
