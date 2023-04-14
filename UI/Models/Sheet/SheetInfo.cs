using System.Text.Json.Serialization;

namespace UI.Models
{
    public class SheetInfo
    {
        public int Id { get; set; }

        [JsonPropertyName("is_online")]
        public bool IsOnline { get; set; }

        public Personal Personal { get; set; }
    }

    public class Personal
    {
        [JsonPropertyName("Avatar_xl")]
        public string Avatar { get; set; }

        [JsonPropertyName("avatar_small")]
        public string AvatarSmall { get; set; }

        public string LastName { get; set; }

        public string Name { get; set; }

        public int Age { get; set; }
    }
}
