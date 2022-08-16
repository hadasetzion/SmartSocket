using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace SocketWebApp.Models
{
    public class Guest
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "firstName")]
        public string FirstName { get; set; }

        [JsonProperty(PropertyName = "lastName")]
        public string LastName { get; set; }

        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "mobile")]
        public string Mobile { get; set; }

        [JsonProperty(PropertyName = "cardNumber")]
        public string CardNumber { get; set; }

        [JsonProperty(PropertyName = "expiry")]
        public string Expiry { get; set; }

        [JsonProperty(PropertyName = "CVV")]
        public string CVV { get; set; }

    }
}

