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
        [JsonPropertyName("items")]
        public List<Ice> Ices { get; set; }
    }

    public class Ice
    {
        private static Random rnd = new Random();

        public string Content{get; set; }

        public DateTime DateCreated { get; set; }

        public int Id { get; set; }

        public int IdTrustedUser { get; set; }

        public IceType Type { get; set; }

        public int NumberOfSent { get; set; } = rnd.Next(30, 50);

        public int Conversion { get; set; } = rnd.Next(0, 30);

        public decimal Efficiency
        {
            get
            {
                if(Conversion == 0)
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
        private static Random rnd = new Random();

        public IceContent Content { get; set; }

        public DateTime DateCreated { get; set; }

        public int Id { get; set; }

        public int IdTrustedUser { get; set; }

        public IceType Type { get; set; }

        public int NumberOfSent { get; set; } = rnd.Next(30, 50);

        public int Conversion { get; set; } = rnd.Next(0, 30);

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
}
