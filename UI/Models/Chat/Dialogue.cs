using System.Text.Json.Serialization;

namespace UI.Models
{
    public class Messenger
    {
        public int SheetId { get; set; }

        public SheetInfo Sheet { get; set; }

        public string Cursor { get; set; }

        public List<Dialogue> Dialogs { get; set; }
    }

    public class Dialogue
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

        public bool IsPremium { get; set; }

        public Message LastMessage { get; set; }

        public string UserName { get; set; }

        public string Avatar { get; set; }

        public Status Status { get; set; }

        public List<Message> Messages { get; set; }

        public object Age { get; set; }

        public int Operator { get; set; }
    }

    public class Message
    {
        public long? Id { get; set; }

        public Content Content { get; set; }

        public DateTime DateCreated { get; set; }

        public int IdUserFrom { get; set; }

        public int IdUserTo { get; set; }

        public MessageType Type { get; set; }

        public MessageTimer Timer { get; set; }
    }

    public class SendMessage
    {
        public long? IdMessage { get; set; }
    }

    public class Content
    {
        public int Id { get; set; }

        public string Message { get; set; }

        public List<PhotoVideo> Photos { get; set; }

        public List<PhotoVideo> Videos { get; set; }

        public int PhotosTotalCount { get; set; }

        public string Url { get; set; }

        public string Key { get; set; }

        public int IdPhoto { get; set; }

        public string Text { get; set; }

        //----- Gift -----
        // Используются так же поля: Id; Message.
        public string AnimationSrc { get; set; }

        public string ImageSrc { get; set; }

        //----- Post =====
        // Используются так же поля: Photos; Videos.
        public int IdPost { get; set; }

        public bool IsPurchased {get; set;}

        public string TextPreview { get; set; }

        public int Price { get; set; }

        public int DiscountPercent { get; set; }

        public int PriceWithDiscount { get; set; }

    }

    public class PhotoVideo
    {
        public long Id { get; set; }

        public string Key { get; set; }

        public string Url { get; set; }
    }

    //При добавлении нового типа необходимо обновить данные:
    //- _MessagesMensPartial.cshtml --> data-filters
    //- allNewMessagesFromAllMen.js метод countNewMessages
    //- Chats\Components\AllNewMessagesFromAllMen\Default.cshtml
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
        Photo,
        Video,
        Post,
        Virtual_Gift,
        Mail
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
