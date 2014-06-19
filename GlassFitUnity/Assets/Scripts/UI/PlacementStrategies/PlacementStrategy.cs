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
            {
                GameObject scriptsHolder = GameObject.Find("ScriptsHolder");
                raceGame = scriptsHolder.GetComponent<RaceGame>();
            }
            if (raceGame == null)
                throw new InvalidProgramException("ScriptsHolder not found!");
            return raceGame;
        }

        public abstract void UpdateScenePositions();

        public void Update()
        {
            UpdateScenePositions();
        }
    }
}
