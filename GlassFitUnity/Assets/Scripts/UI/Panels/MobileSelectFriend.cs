﻿using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;
using System.Threading;
using System;
using System.Collections.Generic;

[Serializable]
public class MobileSelectFriend : MobilePanel 
{
    public MobileSelectFriend() { }
    public MobileSelectFriend(SerializationInfo info, StreamingContext ctxt)
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
            return "MobileSelectFriend: " + gName.Value;
        }
        return "MobileSelectFriend: UnInitialzied";
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override void EnterStart()
    {
        base.EnterStart();

        MobileList list = physicalWidgetRoot.GetComponentInChildren<MobileList>();
        if (list != null)
        {
            list.SetTitle("Select Friend");
        }

        GConnector baseConnection = GetBaseButtonConnection();
        //AddBackButtonData();
        
        List<Friend> friendsData = Platform.Instance.Friends();
        if (friendsData != null)
        {            

            for (int i = 0; i < friendsData.Count; i++)
            {
                AddButtonData("button" + i, friendsData[i].name, "", baseConnection);
            }
            
            if (list != null)
            {            
                list.SetParent(this);
                list.RebuildList();
            }
        }
    }

}
