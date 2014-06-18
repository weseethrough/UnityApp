using System;
using System.Collections.Generic;
using UnityEngine;

namespace RaceYourself
{
    public class FixedWidthClamped2DPlacementStrategy : PlacementStrategy
    {
        public GameObject trackLeftExtremity;
        public GameObject trackRightExtremity;
        public float distanceShown;

        public override void UpdateScenePositions()
        {
            float minTrackX = trackLeftExtremity.transform.position.x;
            float maxTrackX = trackRightExtremity.transform.position.x;
            float y = trackLeftExtremity.transform.position.y;

            List<WorldObject> worldObjects = GetRaceGame().worldObjects;

            List<Vector3> positions = new List<Vector3>();
            
            float actualMinDist = float.MaxValue, actualMaxDist = float.MinValue;
            foreach (WorldObject worldObject in worldObjects)
            {
                float dist = worldObject.getRealWorldDist();
                
                if (dist < actualMinDist)
                    actualMinDist = dist;
                if (dist > actualMaxDist)
                    actualMaxDist = dist;
            }
            float actualDistDiff = actualMaxDist - actualMinDist;
            
            //float displayedMinDist = actualMinDist, displayedMaxDist = actualMaxDist;

            float midpoint = (actualMinDist + actualDistDiff/2);
            float displayedMinDist = midpoint - distanceShown/2; // TODO should be at least 0
            float displayedMaxDist = midpoint + distanceShown/2; // TODO if min=0, max=distanceShown
            if (displayedMinDist < 0)
            {
                displayedMinDist = 0;
                displayedMaxDist = distanceShown;
            }

            foreach (WorldObject worldObject in worldObjects)
            {
                float dist = worldObject.getRealWorldDist();
                
                if (dist < displayedMinDist)
                    dist = displayedMinDist;
                else if (dist > displayedMaxDist)
                    dist = displayedMaxDist;
                
                float trackWidth = maxTrackX - minTrackX;
                float x = minTrackX + ((dist - displayedMinDist) / distanceShown) * trackWidth;
                
                Vector3 pos = new Vector3(x, y);

                worldObject.gameObject.transform.position = pos;
            }
        }
    }
}
