using System.Text.Json.Serialization;

namespace UI.Models
{
    public class Messanger
    {
        public int OwnerId { get; set; }

        public string Avatar { get; set; }

        public string Cursor { get; set; }

        public List<Dialog> Dialogs { get; set; }
    }

    public class Dialog
    {
        public DateTime DateUpdated { get; set; }

        public bool HasNewMessage { get; set; }

        public string HighlightType { get; set; }

        public int IdInterlocutor { get; set; }

        public long? IdInterlocutorLastReadMsg { get; set; }

        public long? IdLastReadMsg { get; set; }

        public int IdUser { get; set; }

        public bool IsActive { get; set; }

        public bool IsBlocked { get; set; }

        public bool IsBookmarked { get; set; }

        public bool IsHidden { get; set; }

        public bool IsPinned { get; set; }

        public Message LastMessage { get; set; }

        public string UserName { get; set; }

        public string Avatar { get; set; }

        public Status Status { get; set; }

        public List<Message> Messages { get; set; }

        public int Age { get; set; }
    }

    public class Message
    {
        public long? Id { get; set; }

        public Content Content { get; set; }

        public DateTime DateCreated { get; set; }

        public int IdUserFrom { get; set; }

        public int IdUserTo { get; set; }

        public MessageType Type { get; set; }
    }

    public class Content
    {
        public string Message { get; set; }

        public List<Photo> Photos { get; set; }

        public int PhotosTotalCount { get; set; }

        public string Url { get; set; }

        public int Id { get; set; }

        public string Key { get; set; }

        public int IdPhoto { get; set; }

        public string Text { get; set; }
    }

    public class Photo
    {
        public long Id { get; set; }

        public string Key { get; set; }

        public string Url { get; set; }
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum MessageType
    {
        Message,
        Sticker,
        System,
        LikePhoto,
        Wink,
        Photo_batch,
        Like_NewsFeed_Post,
        Photo
    }

    public class MessagesAndMailsLeft
    {
        public int IdInterlocutor { get; set; }

        public string UserName { get; set; }

        public string Avatar { get; set; }

        public int MessagesLeft { get; set; }

        [JsonPropertyName("lettersLeft")]
        public int MailsLeft { get; set; }
    }


    //public class CustomDateTimeConverter : IsoDateTimeConverter
    //{
    //    public CustomDateTimeConverter()
    //    {
    //        DateTimeFormat = "dd-MM-yyyy";
    //    }
    //}
}
