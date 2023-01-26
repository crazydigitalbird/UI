using System.Text.Json.Serialization;

namespace UI.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Role
    {
        Admin,
        AdminAgency,
        Operator,
        User
    }
}
