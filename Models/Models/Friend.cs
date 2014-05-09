using System;
using Sqo.Attributes;
using Newtonsoft.Json;

namespace RaceYourself.Models
{
    public class Friend
    {
        [Index]
        [UniqueConstraint]
        public string guid;
        [JsonProperty("has_glass")]
        public bool hasGlass;
        public string name;
        [JsonProperty("photo")]
        [MaxLength(255)]
        public string image;
        public string uid;
        public string provider;
        [JsonProperty("user_id")]
        public int? userId;
        
        // TOOD: Polymophism
        [JsonProperty("screen_name")]
        public string username;

        public string forename {
            get {
                if(!string.IsNullOrEmpty(name)) return name.Split(' ')[0];
                return "Unknown";
            }
        }
        
        public string surname {
            get {
                if(!string.IsNullOrEmpty(name)) {
                    var split = name.Split(' ');
                    return split[split.Length - 1];
                }
                return "Unknown";
            }
        }

        public string GenerateCompositeId ()
        {
            this.guid = uid + "_" + provider;
            return this.guid;
        }
    }
}

