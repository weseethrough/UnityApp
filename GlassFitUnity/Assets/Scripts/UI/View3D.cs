using System;
using UnityEngine;

namespace RaceYourself
{
    public class View3D : View
    {
        protected GameObject theVirtualTrack;

        public View3D ()
        {

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