using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;
using System.Threading;
using System;
using System.Collections.Generic;

[Serializable]
public class ThreeDimensionalPanel : Panel {

	public ThreeDimensionalPanel() { }
    public ThreeDimensionalPanel(SerializationInfo info, StreamingContext ctxt)
        : base(info, ctxt)
    {
    }
	
	public override string GetWidgetRootName ()
	{
		return "Widgets Container3D";
	}
}
