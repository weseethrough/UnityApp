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
        public bool test;

        public PlayerConfig ()
        {
            test = false;
        }

        public PlayerConfig(bool test)
        {
            this.test = test;
        }
    }
}