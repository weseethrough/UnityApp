using System;

namespace RaceYourself.Models
{
    public class MatchedTrack
    {
        public int deviceId;
        public int trackId;

        public MatchedTrack()
        {

        }
        
        public MatchedTrack (int deviceId, int trackId) 
        {
            this.deviceId = deviceId;
            this.trackId = trackId;
        }
    }
}

