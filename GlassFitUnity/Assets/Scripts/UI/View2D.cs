using System;
using UnityEngine;
using RaceYourself.Models;

namespace RaceYourself
{
    public class View2D : View
    {
        public GameObject runningPlayerSprite;
        public GameObject runningOpponentSprite;
        public GameObject cyclingPlayerSprite;
        public GameObject cyclingOpponentSprite;

        protected override GameObject GetVisualRepresentation(ActorActivity activity, User user)
        {
            bool player = user == null ? false : Platform.Instance.api.user.id == user.id;

            GameObject prefab = null;
            if (activity == ActorActivity.Runner)
            {
                if (player)
                {
                    prefab = runningPlayerSprite;
                }
                else
                {
                    prefab = runningOpponentSprite;
                }
            }
            else
            {
                if (player)
                {
                    prefab = cyclingPlayerSprite;
                }
                else
                {
                    prefab = cyclingOpponentSprite;
                }
            }

            GameObject visRep = (GameObject) Instantiate(prefab);

            //GameObject visRep = (GameObject) Instantiate(Resources.Load(player ? "Sprite_Player" : "Sprite_Opponent"));

//            if (activity != ActorActivity.Runner)
//                throw new InvalidOperationException("No sprite prefab set up for cyclists!");

            return visRep;
        }
    }
}