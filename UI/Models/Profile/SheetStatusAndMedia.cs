using System.Text.Json.Serialization;

namespace UI.Models
{
    public class SheetStatusAndMedia
    {
        public int Photo { get; set; }

        [JsonPropertyName("photo_private")]
        public int PrivatePhoto { get; set; }

        public int Video { get; set; }            

        public bool Status { get; set; }
    }
}
