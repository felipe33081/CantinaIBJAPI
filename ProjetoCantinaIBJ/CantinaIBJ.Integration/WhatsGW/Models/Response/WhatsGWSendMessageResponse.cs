using Newtonsoft.Json;

namespace CantinaIBJ.Integration.WhatsGW.Models.Response;

#nullable disable
public class WhatsGWSendMessageResponse
{
    [JsonProperty("result")]
    public string Result { get; set; }

    [JsonProperty("message_id")]
    public int MessageId { get; set; }

    [JsonProperty("contact_phone_number")]
    public string ContactPhoneNumber { get; set; }

    [JsonProperty("phone_state")]
    public string PhoneState { get; set; }
}