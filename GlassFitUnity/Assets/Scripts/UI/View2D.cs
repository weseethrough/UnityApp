using System;
using UnityEngine;
using RaceYourself.Models;

namespace RaceYourself
{
    public class View2D : View
    {
        public View2D ()
        {

        }

        protected override GameObject GetVisualRepresentation(ActorActivity activity, User user)
        {
            bool player = Platform.Instance.api.user.id == user.id;

            GameObject visRep = (GameObject) Instantiate(Resources.Load(player ? "Sprite_Player" : "Sprite_Opponent"));

            if (activity != ActorActivity.Runner)
                throw new InvalidOperationException("No sprite prefab set up for cyclists!");

            return visRep;
        }
    }
}