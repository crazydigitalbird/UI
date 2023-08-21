using System.Text.Json.Serialization;

namespace UI.Models
{
    public class Autoresponder
    {
        public AutoresponderData Data { get; set; }
    }

    public class AutoresponderData
    {
        [JsonPropertyName("user_id")]
        public int UserId { get; set; }

        [JsonPropertyName("stack_type")]
        public StackType StackType { get; set; }
    }

    public class StackType
    {
        public AutoresponderType LikePhoto { get; set; }

        public AutoresponderType System { get; set; }

        public AutoresponderType Wink { get; set; }
    }

    public class AutoresponderType
    {
        [JsonPropertyName("converse_finish")]
        public int ConverseFinish { get; set; }

        [JsonPropertyName("converse_start")]
        public int ConverseStart { get; set; }

        [JsonPropertyName("first_interval")]
        public int FirstInterval { get; set; }

        [JsonPropertyName("first_interval_finish")]
        public int FirstIntervalFinish { get; set; }

        [JsonPropertyName("first_limit_count")]
        public int FirstLimitCount { get; set; }

        [JsonPropertyName("first_message")]
        public string FirstMessage { get; set; }

        [JsonPropertyName("last_limit_count")]
        public int LastLimitCount { get; set; }

        [JsonPropertyName("last_message")]
        public string LastMessage { get; set; }

        [JsonPropertyName("second_interval")]
        public int SecondInterval { get; set; }

        [JsonPropertyName("second_interval_finish")]
        public int SecondIntervalFinish { get; set; }

        [JsonPropertyName("second_limit_count")]
        public int SecondLimitCount { get; set; }

        [JsonPropertyName("second_message")]
        public string  SecondMessage { get; set; }

        [JsonPropertyName("stack_id")]
        public int StackId { get; set; }
    }

    public enum AutoresponderMessages
    {
        first_message = 1,
        second_message, 
        last_message
    }

    public class AutoresponderInfoBody
    {
        public List<Dictionary<string, AutoresponderInfo>> Data { get; set; }
    }

    public class AutoresponderInfo
    {
        [JsonPropertyName("system")]
        public bool IsSystem { get; set; }

        [JsonPropertyName("likephoto")]
        public bool IsLike { get; set; }

        [JsonPropertyName("wink")]
        public bool IsWink { get; set; }

        //[JsonPropertyName("")]
        public bool IsSpam { get; set; }
    }
}
