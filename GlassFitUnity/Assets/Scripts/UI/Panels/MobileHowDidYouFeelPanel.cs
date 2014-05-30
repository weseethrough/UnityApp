using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;
using System.Threading;
using System;
using System.IO;
using System.Collections.Generic;

using RaceYourself.Models;
using Newtonsoft.Json;

[Serializable]
public class MobileHowDidYouFeelPanel : MobilePanel {
    
    Friend chosenFriend;
    
    public MobileHowDidYouFeelPanel() { }
    public MobileHowDidYouFeelPanel(SerializationInfo info, StreamingContext ctxt)
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
            return "MobileHowDidYouFeelPanel: " + gName.Value;
        }
        return "MobileHowDidYouFeelPanel: Uninitialized";
    }
    
    public override void EnterStart ()
    {
        base.EnterStart ();
        
        // TODO notify server that track has been run against if race was completed.
    }
}
