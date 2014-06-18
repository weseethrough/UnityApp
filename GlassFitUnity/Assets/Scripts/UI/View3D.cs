using System;
using UnityEngine;
using RaceYourself.Models;

namespace RaceYourself
{
    public class View3D : View
    {
        protected GameObject theVirtualTrack;

        public View3D ()
        {

        }

        // TODO assumes 1st person view. 3rd person 3D will require different handling for player.
        protected override GameObject GetVisualRepresentation(ActorActivity activity, User user)
        {
            bool player = Platform.Instance.api.user.id == user.id;

            if (player)
                return null; // assume 1st person for now, so no visual representation.

            string prefab = null;
            switch (activity)
            {
            case ActorActivity.Runner:
                prefab = "DavidRealWalk";
                break;
            case ActorActivity.Cyclist:
                prefab = "DavidCycling";
                break;
            }

            GameObject visRep = (GameObject) Instantiate(Resources.Load(prefab));

            return visRep;
        }

        public void Start()
        {
            SetVirtualTrackVisible(true);
        }
        
        public void SetVirtualTrackVisible(bool visible)
        {
            if(theVirtualTrack == null)
            {
                theVirtualTrack = GameObject.Find("VirtualTrack");
            }
            if(theVirtualTrack != null)
            {
                theVirtualTrack.SetActive(visible);
            }
            else
            {
                UnityEngine.Debug.Log("GameBase: Couldn't find virtual track to set visiblity");
            }
        }
    }
}