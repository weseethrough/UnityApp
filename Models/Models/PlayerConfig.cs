using System;
using Newtonsoft.Json;

namespace RaceYourself.Models
{
    //------------------------------------------------------------------------------
    // Config for the person playing the game (not a friend).
    //------------------------------------------------------------------------------
    [JsonConverter(typeof(CustomConverter))]
    public class PlayerConfig
    {
        /// Is the player a beta tester? Used to determine whether we should prompt them for feedback.
        public string configuration;

        [JsonIgnore]
        public ConfigurationPayload payload;

        public PlayerConfig ()
        {

        }

        public PlayerConfig(string configuration)
        {
            this.configuration = configuration;
        }

        public bool Test()
        {
            return payload.test;
        }
    }

    [JsonConverter(typeof(CustomConverter))]
    public class ConfigurationPayload
    {
        public bool test;
    }
}