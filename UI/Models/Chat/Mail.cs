using System.Text.Json.Serialization;

namespace UI.Models
{
    public class Mailbox
    {
        public MailSection Data { get; set; }
    }

    public class MailSection
    {
        public Counters Counters { get; set; }

        public Counters InboxCounters { get; set; }

        public List<Mail> Mails { get; set; }
    }

    public class Counters
    {
        public int CountNew { get; set; }

        public int CountTotal { get; set; }
    }

    public class Mail
    {
        public int Id { get; set; }

        public int IdRegularUser { get; set; }

        public bool IsRegularUserAbused { get; set; }

        public bool IsTrustedUserAbused { get; set; }

        public string Uid { get; set; }
    }

    public class Correspondence
    {
        public int Count { get; set; }

        public bool HasAttachments { get; set; }

        public bool HasRegularUserNew { get; set; }

        public bool HasTrustedUserNew { get; set; }

        public bool IsDeletedByRegularUser { get; set; }

        public bool IsDeletedByTrustedUser { get; set; }

        public MailMessage Last { get; set; }

        public int RegularUserCount { get; set; }

        public int TrustedUserCount { get; set; }
    }

    public class MailMessage
    {
        public string Content { get; set; }

        [JsonPropertyName("conversation_index")]
        public int ConversationIndex { get; set; }

        [JsonPropertyName("conversation_remark_index")]
        public int ConversationRemarkIndex { get; set; }

        [JsonPropertyName("date_created")]
        public DateTime DateCreated { get; set; }

        [JsonPropertyName("date_read")]
        public string DateRead { get; set; }

        public long Id { get; set; }

        [JsonPropertyName("id_correspondence")]
        public long IdCorrespondence { get; set; }

        [JsonPropertyName("id_user_from")]
        public int IdUserFrom { get; set; }

        [JsonPropertyName("id_user_to")]
        public int IdUserTo { get; set; }

        [JsonPropertyName("is_deleted")]
        public object IsDeleted { get; set; }

        [JsonPropertyName("is_paid")]
        public bool IsPaid { get; set; }

        [JsonPropertyName("is_sent")]
        public object Is_Sent { get; set; }

        public string Status { get; set; }

        public string Title { get; set; }

        public Attachments Attachments { get; set; }
    }

    public class Attachments
    {
        public List<Attachment> Images { get; set; }

        public List<Attachment> Videos { get; set; }
    }

    public class Attachment
    {
        public long Id { get; set; }

        [JsonPropertyName("url_original")]
        public string UrlOriginal { get; set; }

        [JsonPropertyName("url_thumbnail")]
        public string UrlThumbnail { get; set; }
    }

    public class MailHistoryData
    {
        public MailHistory Data { get; set; }
    }

    public class MailHistory
    {
        public List<MailMessage> History { get; set; }

        public int Limit { get; set; }

        public int Page { get; set; }

        public string Status { get; set; }
    }
}
