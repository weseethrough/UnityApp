using System;
using UnityEngine;
using RaceYourself.Models;

namespace RaceYourself
{
    public abstract class ActorVisualsFactory : MonoBehaviour
    {
        /// <summary>
        /// Instantiates an appropriate visual representation, based off:
        /// 
        /// -The actor's activity - e.g. a model of a person on a bike if the activity is ActorActivity.Cyclist
        /// -The user corresponding to this actor.
        ///     -Can be used to determine whether it's the active player (user.id == Platform.Instance.api.user.id)
        ///     -Unless a stickman is represented, we should consider the user's gender when choosing a model.
        ///     -User.image could theoretically be mapped on to a 3D model's face...
        ///     -(Warning - possibly offensive!) Age/fitness level could be worked in...
        ///     -If at some point we capture appearance characteristics 'a la Wii/Mii', then height/skin colour/clothing
        ///      adjustments can be taken into account here.
        /// 
        /// This representation could be:
        /// 
        /// -A sprite.
        /// -A 3D model.
        /// -null (to represent the player in a 3D 1st person view)
        /// 
        /// The same input should always give the same output. In a multi-opponent scenario, if different clothing colours
        /// etc are desired, this can be done in a consistent way by e.g. mapping user.id to a colour (via hashing?).
        /// </summary>
        /// <returns>The visual representation.</returns>
        /// <param name="actor">Actor.</param>
        protected abstract GameObject GetVisualRepresentation(ActorActivity activity, User user);

        /// <summary>
        /// Adds an appropriate visual representation to the actor. Added as a child GameObject of actor.
        /// </summary>
        /// <param name="actor">Actor to add visual representation to.</param>
        /// <param name="activity">Activity that the actor is engaging in.</param>
        /// <param name="user">User corresponding to this actor.</param>
        public void AddVisualRepresentation(GameObject actor, ActorActivity activity, User user)
        {
            GameObject visualRep = GetVisualRepresentation(activity, user);
            if (visualRep != null)
                visualRep.transform.parent = actor.transform;
        }
    }
}