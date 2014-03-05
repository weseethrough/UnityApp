using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;
using System.Threading;
using System;
using System.Collections.Generic;

[Serializable]
public class MobileSelectTrack : MobilePanel 
{
    public MobileSelectTrack() { }
    public MobileSelectTrack(SerializationInfo info, StreamingContext ctxt)
        : base(info, ctxt)
    {
    }

    /// <summary>
    /// Gets display name of the node, helps with node identification in editor
    /// </summary>
    /// <returns>name of the node</returns>
    public override string GetDisplayName()
    {
        base.GetDisplayName();

        GParameter gName = Parameters.Find(r => r.Key == "Name");
        if (gName != null)
        {
            return "MobileSelectTrack: " + gName.Value;
        }
        return "MobileSelectTrack: UnInitialzied";
    }

    public override void EnterStart()
    {
        base.EnterStart();

        MobileList list = physicalWidgetRoot.GetComponentInChildren<MobileList>();
        if (list != null)
        {
            list.SetTitle("Select Track");
        }

        int finish = 10000;
        int lowerFinish = 100;

        List<Track> trackList = Platform.Instance.GetTracks(finish, lowerFinish);
        if (trackList != null && trackList.Count > 0)
        {
            GConnector baseConnection = GetBaseButtonConnection();
            //AddBackButtonData();

            for (int i = 0; i < trackList.Count; i++)
            {
                AddButtonData("button" + i, "D:" + trackList[i].distance + ",T:" + trackList[i].time, "", baseConnection);
            }

            
            if (list != null)
            {
                list.SetParent(this);
                list.RebuildList();
            }
        }
    }

}
