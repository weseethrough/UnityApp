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
                //Panel panel = FlowStateMachine.GetCurrentFlowState() as Panel;

                //raceGame = gameObject.transform.parent.gameObject.GetComponentInChildren<RaceGame>();

                //GameObject scriptsHolder = GameObjectUtils.GetComponentByName<RaceGame>(panel.physicalWidgetRoot, "ScriptsHolder");
                GameObject scriptsHolder = GameObject.Find("ScriptsHolder");
                //raceGame = gameObject.GetComponent<RaceGame>();
            }
            return raceGame;
        }

        public abstract void UpdateScenePositions();

        public void Update()
        {
            UpdateScenePositions();
        }
    }
}
