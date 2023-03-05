using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Text.Json;
using System.ComponentModel.DataAnnotations;

namespace UI.Models
{
    public class Messanger
    {
        public int OwnerId { get; set; }

        public string Avatar { get; set; }

        public List<Chat> Chats { get; set; }
    }

    public class Chat
    {
        public int Id { get; set; }

        public string UserName { get; set; }

        public string Avatar { get; set; }

        public Status Status { get; set; }

        public string AboutMe { get; set; }

        public List<Message> Messages { get; set; }
    }

    public class Message
    {
        public string Text { get; set; }

        //[JsonConverter(typeof(CustomDateTimeConverter))]

        public DateTime DateTime { get; set; }

        public bool IsOwner { get; set; }

        public bool IsRead { get; set; }
    }

    public class CustomDateTimeConverter : IsoDateTimeConverter
    {
        public CustomDateTimeConverter()
        {
            DateTimeFormat = "dd-MM-yyyy";
        }
    }
}
