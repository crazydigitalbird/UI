using System.Text.Json.Serialization;

namespace UI.Models
{
    public class PhotosPersonData
    {
        [JsonPropertyName("data")]
        public PhotosPerson PhotosPerson { get; set; }
    }

    public class PhotosPerson
    {
        [JsonPropertyName("is-trusted")]
        public int IsTrusted { get; set; }

        public List<PhotoPerson> Private { get; set; }

        public List<PhotoPerson> Public { get; set; }
    }

    public class PhotoPerson
    {
        [JsonPropertyName("can_be_main")]
        public int CanBeMain { get; set; }

        [JsonPropertyName("date_created")]
        public string DateCreated { get; set; }

        [JsonPropertyName("date_updated")]
        public string DateUpdated { get; set; }

        public long Id { get; set; }

        [JsonPropertyName("id_image")]
        public long IdImage { get; set; }

        [JsonPropertyName("id_media")]
        public long IdMedia { get; set; }

        [JsonPropertyName("id_type")]
        public long IdType { get; set; }

        [JsonPropertyName("ionthe_key")]
        public string IontheKey { get; set; }

        public int IsMain { get; set; }

        [JsonPropertyName("is_blocked")]
        public int IsBlocked { get; set; }

        [JsonPropertyName("is_main")]
        public int Is_Main { get; set; }

        public string Privacy { get; set; }

        [JsonPropertyName("url_large")]
        public string UrlLarge { get; set; }

        [JsonPropertyName("url_medium")]
        public string UrlMedium { get; set; }

        [JsonPropertyName("url_original")]
        public string urlOriginal { get; set; }

        [JsonPropertyName("url_small")]
        public string UrlSmall { get; set; }

        [JsonPropertyName("url_standart")]
        public string UrlStandart { get; set; }

        [JsonPropertyName("url_xl")]
        public string UrlXL { get; set; }

        [JsonPropertyName("url_xs")]
        public string UrlXS { get; set; }

        [JsonPropertyName("url_xxl")]
        public string UrlXXL { get; set; }

        [JsonPropertyName("url_xxs")]
        public string UrlXXS { get; set; }
    }
}
