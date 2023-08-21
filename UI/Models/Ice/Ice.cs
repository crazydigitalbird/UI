using Microsoft.CodeAnalysis.CSharp;
using System.Text.Json.Serialization;

namespace UI.Models
{
    public class AprovIce
    {
        public DataIce Data { get; set; }
    }

    public class DataIce
    {
        public string Cursor { get; set; }

        [JsonPropertyName("items")]
        public List<Ice> Ices { get; set; }
    }

    public class Ice
    {
        public string Content { get; set; }

        public DateTime DateCreated { get; set; }

        public int Id { get; set; }

        public int IdTrustedUser { get; set; }

        public IceType Type { get; set; }

        [JsonPropertyName("count_sent")]
        public int NumberOfSent { get; set; }

        [JsonPropertyName("count_answered")]
        public int Conversion { get; set; }

        public decimal Efficiency
        {
            get
            {
                if (Conversion == 0)
                {
                    return 0;
                }
                return (decimal)Conversion / NumberOfSent;
            }
        }
    }

    public class AprovIceProgress
    {
        public DataIceProcess Data { get; set; }
    }

    public class DataIceProcess
    {
        [JsonPropertyName("items")]
        public List<IceProcess> Ices { get; set; }
    }

    public class IceProcess
    {

        public IceContent Content { get; set; }

        public DateTime DateCreated { get; set; }

        public int Id { get; set; }

        public int IdTrustedUser { get; set; }

        public IceType Type { get; set; }

        [JsonPropertyName("count_sent")]
        public int NumberOfSent { get; set; }

        [JsonPropertyName("count_answered")]
        public int Conversion { get; set; }

        public decimal Efficiency
        {
            get
            {
                if (Conversion == 0)
                {
                    return 0;
                }
                return (decimal)Conversion / NumberOfSent;
            }
        }

        public static explicit operator Ice(IceProcess iceProcess)
        {
            Ice ice = new Ice
            {
                Id = iceProcess.Id,
                IdTrustedUser = iceProcess.IdTrustedUser,
                Type = iceProcess.Type,
                NumberOfSent = iceProcess.NumberOfSent,
                Conversion = iceProcess.Conversion,
                DateCreated = iceProcess.DateCreated,
                Content = iceProcess.Content.Body
            };
            return ice;
        }
    }

    public class IceContent
    {
        public string Body { get; set; }
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum IceType
    {
        Mail,
        Message
    }

    public class SendIce
    {
        public long Id { get; set; }
    }

    public class MakeIceTrash
    {
        public bool Response { get; set; }
    }

    public class IceInfoBody
    {
        public IceInfoData Data { get; set; }
    }

    public class IceInfoData
    {
        [JsonPropertyName("items")]
        public List<IceInfo> IcesInfo { get; set; }
    }

    public class IceInfo
    {
        public int IdTrustedUser { get; set; }

        [JsonPropertyName("mail")]
        public int NumberOfMails { get; set; }

        [JsonPropertyName("mail_progress")]
        public int MailInPprogress { get; set; }

        [JsonPropertyName("message")]
        public int NumberOfMessages { get; set; }

        [JsonPropertyName("message_progress")]
        public int MessageInProgress { get; set; }
    }
}
