using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;


namespace SocketWebApp.Models
{

    public class User
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

        [JsonProperty(PropertyName = "userName")] //change to be a secret
        public string UserName { get; set; } 

        [JsonProperty(PropertyName = "password")] //change to be a secret
        public string Password { get; set; }
    }
}

