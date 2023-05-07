using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace UI.Models
{
    public class SheetInfo
    {
        public int Id { get; set; }

        [JsonIgnore]
        public int SheetId { get; set; }

        [JsonPropertyName("billing_verification")]
        public int? BillingVerification { get; set; }

        [JsonPropertyName("date_created")]
        public string DateCreated { get; set; }

        [JsonPropertyName("date_updated")]
        public string DateUpdated { get; set; }

        [JsonPropertyName("id_city")]
        public int? IdCity { get; set; }

        [JsonPropertyName("id_country")]
        public int? IdCountry { get; set; }

        [JsonPropertyName("id_user")]
        public int? IdUser { get; set; }

        [JsonPropertyName("is_abused")]
        public bool IsAbused { get; set; }

        [JsonPropertyName("is_blocked")]
        public bool IsBlocked { get; set; }

        [JsonPropertyName("is_online")]
        public bool IsOnline { get; set; }

        [JsonPropertyName("is_regular_user")]
        public bool IsRegularUser { get; set; }

        public string MirrorName { get; set; }

        public string Name { get; set; }

        public string Partner { get; set; }

        public Personal Personal { get; set; }

        public Preferences Preferences { get; set; }

        public Splits Splits { get; set; }
    }

    public class Personal
    {
        public object Age { get; set; }

        [JsonPropertyName("avatar_small")]
        public string AvatarSmall { get; set; }

        [JsonPropertyName("avatar_large")]
        public string AvatarLarge { get; set; }

        [JsonPropertyName("Avatar_xl")]
        public string Avatar { get; set; }

        [JsonPropertyName("Avatar_xxl")]
        public string AvatarXXL { get; set; }

        [JsonPropertyName("birth_day")]
        public string BirthDay { get; set; }

        [JsonPropertyName("birth_month")]
        public string BirthMonth { get; set; }

        [JsonPropertyName("birth_year")]
        public string BirthYear { get; set; }

        [JsonPropertyName("body_type")]
        public string BodyType { get; set; }

        public string City { get; set; }

        [JsonPropertyName("color_eye")]
        public string ColorEye { get; set; }

        [JsonPropertyName("color_hair")]
        public string ColorHair { get; set; }

        [JsonPropertyName("count_children")]
        public int CountChildren { get; set; }

        [JsonPropertyName("count_photos")]
        public int CountPhotos { get; set; }

        [JsonPropertyName("count_videos")]
        public int CountVideos { get; set; }

        public string Country { get; set; }

        [JsonPropertyName("date_birth")]
        public string DateBirth { get; set; }

        public string Description { get; set; }

        public YesNo Drinking { get; set; }

        public string Education { get; set; }

        [JsonPropertyName("eng_level")]
        public string EngLevel { get; set; }

        [JsonPropertyName("field_of_work")]
        public string FieldOfWork { get; set; }

        public string Gender { get; set; }

        public List<string> Goal { get; set; }

        public int Height { get; set; }

        public List<string> Hobbies { get; set; }

        [JsonPropertyName("level_of_english")]
        public string LevelOfEnglish { get; set; }

        [JsonPropertyName("looking_for")]
        public string LookingFor { get; set; }

        [JsonPropertyName("marital_status")]
        public string MaritalStatus { get; set; }

        [JsonPropertyName("movie_genres")]
        public List<string> MovieGenres { get; set; }

        [JsonPropertyName("music_genres")]
        public List<string> MusicGenres { get; set; }

        public string LastName { get; set; }

        public string Name { get; set; }

        public string Occupation { get; set; }

        [JsonPropertyName("other_langs")]
        public string OtherLangs { get; set; }

        [JsonPropertyName("other_languages")]
        public List<string> OtherLanguages { get; set; }

        public string Religion { get; set; }

        public string Role { get; set; }

        public YesNo Smoking { get; set; }

        public List<string> Traits { get; set; }

        [JsonPropertyName("want_more_children")]
        public YesNo WantMoreChildren { get; set; }

        public int Weight { get; set; }
    }

    public class Preferences
    {
        [JsonPropertyName("age_from")]
        public int AgeFrom { get; set; }

        [JsonPropertyName("age_to")]
        public int AgeTo { get; set; }

        [JsonPropertyName("body_type")]
        public string BodyType { get; set; }

        public YesNo Drinks { get; set; }

        [JsonPropertyName("has_children")]
        public int HasChildren { get; set; }

        [JsonPropertyName("height_from")]
        public int HeightFrom { get; set; }

        [JsonPropertyName("height_to")]
        public int HeightTo { get; set; }

        [JsonPropertyName("join_reasons")]
        public string JoinReasons { get; set; }

        [JsonPropertyName("more_children")]
        public int MoreChildren { get; set; }

        [JsonPropertyName("pref_personality_type")]
        public string PrefPersonalityType { get; set; }

        public YesNo Smokes { get; set; }

        [JsonPropertyName("weight_from")]
        public int WeightFrom { get; set; }

        [JsonPropertyName("weight_to")]
        public int WeightTo { get; set; }

        public string Age()
        {
            if (AgeFrom > 0 && AgeTo > 0)
            {
                return $"{AgeFrom}-{AgeTo}";
            }
            else if (AgeFrom > 0)
            {
                return $"от {AgeFrom}";
            }
            else if(AgeTo > 0)
            {
                return $"до {AgeTo}";
            }
            return "Не указано";
        }

        public string Height()
        {
            if (HeightFrom > 0 && HeightTo > 0)
            {
                return $"{HeightFrom}-{HeightTo}";
            }
            else if (HeightFrom > 0)
            {
                return $"от {HeightFrom}";
            }
            else if (HeightTo > 0)
            {
                return $"до {HeightTo}";
            }
            return "Не указано";
        }

        public string Weight()
        {
            if (WeightFrom > 0 && WeightTo > 0)
            {
                return $"{WeightFrom}-{WeightTo}";
            }
            else if (WeightFrom > 0)
            {
                return $"от {WeightFrom}";
            }
            else if (WeightTo > 0)
            {
                return $"до {WeightTo}";
            }
            return "Не указано";
        }
    }

    public class Splits
    {
        [JsonPropertyName("cannibal_banners")]
        public int? CannibalBanners { get; set; }

        [JsonPropertyName("chat_can_read_2")]
        public int? ChatCanRead2 { get; set; }

        [JsonPropertyName("chat_can_write_2")]
        public int? ChatCanWrite2 { get; set; }

        [JsonPropertyName("main_page_tu_profile_redesign")]
        public int? MainPageTuProfileRedesign { get; set; }

        [JsonPropertyName("new_payment_popups")]
        public int? NewPaymentPopups { get; set; }

        [JsonPropertyName("non_effective_tu_old_users")]
        public int? NonEffectiveTuOldUsers { get; set; }

        [JsonPropertyName("open_learn")]
        public int? OpenLearn { get; set; }

        [JsonPropertyName("paid_read_mail")]
        public int? PaidReadMail { get; set; }

        [JsonPropertyName("price_test")]
        public int? PriceTest { get; set; }

        [JsonPropertyName("ru_chat_highlights_old")]
        public int? RuChatHighlightsOld { get; set; }

        [JsonPropertyName("trigger_letters_old_users")]
        public int? TriggerLettersOldUsers { get; set; }

        [JsonPropertyName("trusted_user_visibility")]
        public int? TrustedUserVisibility { get; set; }
    }

    public enum YesNo
    {
        [Display(Name="Не указано")]
        None = 0,
        [Display(Name = "Да")]
        Yes = 1,
        [Display(Name = "Нет")]
        No = 2
    }
}
