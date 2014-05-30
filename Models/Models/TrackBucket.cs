using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Sqo.Attributes;

namespace RaceYourself.Models
{
    public class TrackBucket
    {
        public string fitnessLevel;
        public int duration;
        public List<Track> all;
        [Ignore]
        public List<Track> tracks; // Unmatched tracks

        public TrackBucket() { }
        public TrackBucket (string fitnessLevel, int duration, List<Track> tracks)
        {
            this.fitnessLevel = fitnessLevel;
            this.duration = duration;
            this.all = tracks;
        }
    }
}

