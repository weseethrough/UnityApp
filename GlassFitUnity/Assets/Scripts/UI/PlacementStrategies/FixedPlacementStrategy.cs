using System;
using System.Collections.Generic;
using UnityEngine;

namespace RaceYourself
{
    /// <summary>
    /// For testing purposes: places all world objects at the same fixed point.
    /// </summary>
    public class FixedPlacementStrategy : PlacementStrategy
    {
        public Vector3 position;
        
        public override void UpdateScenePositions()
        {
            List<WorldObject> worldObjects = GetRaceGame().worldObjects;

            List<Vector3> positions = new List<Vector3>();
            foreach (WorldObject worldObject in worldObjects)
            {
                worldObject.gameObject.transform.position = position;
            }
        }
    }
}
