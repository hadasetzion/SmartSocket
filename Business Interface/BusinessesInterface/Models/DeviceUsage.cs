using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace BusinessesInterface.Models
{

    public class Address
    {
        [JsonProperty(PropertyName = "id")] 
        public string? Id { get; set; }

        [JsonProperty(PropertyName = "devices")]
        public Device[]? Devices { get; set; }

        [JsonProperty(PropertyName = "password")]
        public string? Password { get; set; }

        [JsonProperty(PropertyName = "overAllUsage")]
        public int OverAllUsage { get; set; }
    }

    public class Device
    {
        [JsonProperty(PropertyName = "id")]
        public string? Id { get; set; }

        [JsonProperty(PropertyName = "deviceID")]
        public string? DeviceID { get; set; }

        [JsonProperty(PropertyName = "history")]
        public long[]? History { get; set; }

        [JsonProperty(PropertyName = "historyDateTime")]
        public DateTime[]? HistoryDateTime { get; set; }

        [JsonProperty(PropertyName = "overAllUsage")]
        public int? OverAllUsage { get; set; }

        [JsonProperty(PropertyName = "table")]
        public string? Table { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string? Status { get; set; }

        [JsonProperty(PropertyName = "last_Seen")]
        public long LastSeen { get; set; }

        [JsonProperty(PropertyName = "healthCheck")]
        public string? HealthCheck { get; set; }

    }

    public class TimeSearch
    {
        [JsonProperty(PropertyName = "timeToSearch")]
        [DataType(DataType.Date, ErrorMessage = "Date only")]
        [DisplayFormat(DataFormatString = "{dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime TimeToSearch { get; set; }
    }
}

