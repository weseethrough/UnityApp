using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace RaceYourself
{
    /// <summary>
    /// For testing purposes: places all world objects at the same fixed point.
    /// </summary>
    public class Track3DPlacementStrategy : PlacementStrategy
    {
        public Vector3 position;

        private WorldObject centreOfUniverse;

        private void init()
        {
            List<WorldObject> worldObjects = GetRaceGame().worldObjects;
            WorldObject centreOfUniverse = worldObjects.FirstOrDefault(w => w.keepAtOrigin);
        }
        
        public override void UpdateScenePositions()
        {
            List<WorldObject> worldObjects = GetRaceGame().worldObjects;

            if (centreOfUniverse == null)
            {
                init();
            }

            foreach (WorldObject worldObject in worldObjects)
            {
                if (worldObject != centreOfUniverse)
                {
                    float sceneZ = worldObject.realWorldPos.z - centreOfUniverse.realWorldPos.z;
                    transform.position = new Vector3(worldObject.realWorldPos.x, worldObject.realWorldPos.y, sceneZ);
                }
            }
        }
    }
}
