using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;
using System.Threading;
using System;
using System.Collections.Generic;

[Serializable]
public class MobileSettings : MobilePanel 
{

    public MobileSettings() { }
    public MobileSettings(SerializationInfo info, StreamingContext ctxt)
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
            return "MobileSettings: " + gName.Value;
        }
        return "MobileSettings: UnInitialzied";
    }

    public override void EnterStart()
    {
        base.EnterStart();

        MobileList list = physicalWidgetRoot.GetComponentInChildren<MobileList>();
        if (list != null)
        {
            list.SetTitle("Select Track");
        }

        /*AddButtonData("Button0", "Button0", "B", ListButtonData.ButtonFormat.ButtonNormalPrototype, GetBaseButtonConnection());
        AddButtonData("Button1", "Button1", "B", ListButtonData.ButtonFormat.ButtonNormalPrototype, GetBaseButtonConnection());
        AddButtonData("Button2", "Button2", "B", ListButtonData.ButtonFormat.ButtonNormalPrototype, GetBaseButtonConnection());
        AddButtonData("Button3", "Button3", "B", ListButtonData.ButtonFormat.ButtonNormalPrototype, GetBaseButtonConnection());        
        AddButtonData("Slider0", "Slider0", "S", ListButtonData.ButtonFormat.SliderPrototype);
        AddButtonData("Slider1", "Slider1", "S", ListButtonData.ButtonFormat.SliderPrototype);
        AddButtonData("Slider2", "Slider2", "S", ListButtonData.ButtonFormat.SliderPrototype);
        AddButtonData("Slider3", "Slider3", "S", ListButtonData.ButtonFormat.SliderPrototype);
        */
        for (int k = 0; k < 800; k++ )
        {
            AddButtonData("Slider"+k, "Slider"+k, "S", ListButtonData.ButtonFormat.SliderPrototype);
            AddButtonData("Button"+k, "Button"+k, "B", ListButtonData.ButtonFormat.ButtonNormalPrototype, GetBaseButtonConnection());
        }

            if (list != null)
            {
                list.SetParent(this);
            }
        
    }

}
