using System;
using System.Collections.Generic;
using UnityEngine;

namespace RaceYourself
{
    // TODO decouple from GameObject holding RaceGame so we can put this script on the Actors GameObject
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
