using System;
using System.Collections.Generic;
using UnityEngine;

namespace RaceYourself
{
    public abstract class PlacementStrategy : MonoBehaviour
    {
        private RaceGame raceGame;
        
        protected RaceGame GetRaceGame()
        {
            if (raceGame == null)
                raceGame = gameObject.GetComponent<RaceGame>();
            return raceGame;
        }

        public abstract void UpdateScenePositions();

        public void Update()
        {
            UpdateScenePositions();
        }
    }
}
