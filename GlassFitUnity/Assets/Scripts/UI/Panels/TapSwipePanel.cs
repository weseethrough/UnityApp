using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

[Serializable]
public class TapSwipePanel : Panel {
	public string guardKey = null;
	
	private GestureHelper.OnTap tapHandler = null;
	private GestureHelper.OnSwipeLeft leftHandler = null;
    private GestureHelper.OnSwipeRight rightHandler = null;
	
	public TapSwipePanel() {}
    public TapSwipePanel(SerializationInfo info, StreamingContext ctxt)
        : base(info, ctxt)
    {
	}

    protected override void Initialize()
    {
        base.Initialize();

        NewOutput("OnTap", "Flow");
        NewOutput("OnSwipeLeft", "Flow");
        NewOutput("OnSwipeRight", "Flow");
    }
	
	public override void EnterStart()
    {
        base.EnterStart();
        DefineHandlers();

        if (tapHandler != null)     GestureHelper.onTap         += tapHandler;
        if (leftHandler != null)    GestureHelper.onSwipeLeft   += leftHandler;
        if (rightHandler != null)   GestureHelper.onSwipeRight  += rightHandler;
    }

    public override void ExitStart()
    {
 	     base.ExitStart();

         if (tapHandler != null)    GestureHelper.onTap         -= tapHandler;
         if (leftHandler != null)   GestureHelper.onSwipeLeft   -= leftHandler;
         if (rightHandler != null)  GestureHelper.onSwipeRight  -= rightHandler;
    }

    public virtual void DefineHandlers()
    {
        tapHandler = new GestureHelper.OnTap(() => 
        {
            if (Outputs.Count > 0 && parentMachine != null)
            {
                GConnector gConect = Outputs.Find(r => r.Name == "OnTap");
                if (gConect != null)
                {
                    ConnectionWithCall(gConect, null);
                }
            }
		});

        leftHandler = new GestureHelper.OnSwipeLeft(() =>
        {
            if (Outputs.Count > 0 && parentMachine != null)
            {
                GConnector gConect = Outputs.Find(r => r.Name == "OnSwipeLeft");
                if (gConect != null)
                {
                    ConnectionWithCall(gConect, null);
                }
            }
        });

        rightHandler = new GestureHelper.OnSwipeRight(() =>
        {
            if (Outputs.Count > 0 && parentMachine != null)
            {
                GConnector gConect = Outputs.Find(r => r.Name == "OnSwipeRight");
                if (gConect != null)
                {
                    ConnectionWithCall(gConect, null);
                }
            }
        });	
    }

}
