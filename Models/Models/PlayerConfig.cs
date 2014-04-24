using System;
using Newtonsoft.Json;
using Sqo.Attributes;
using UnityEngine;
using Sqo;
using System.Collections.Generic;

namespace RaceYourself.Models
{
    //------------------------------------------------------------------------------
    // Config for the person playing the game (not a friend).
    //------------------------------------------------------------------------------
    public class PlayerConfig
    {
        [Index]
        [UniqueConstraint]
        public string id;

        public string type; // ignore

        /// Is the player a beta tester? Used to determine whether we should prompt them for feedback.
        public Dictionary<String, bool> configuration;

        public DateTime? created_at;

        public DateTime? updated_at;

        public PlayerConfig ()
        {

        }

        public PlayerConfig(Dictionary<String, bool> configuration)
        {
            this.configuration = configuration;
        }

        public bool Test()
        {
            return configuration["test"];
        }
    }
}