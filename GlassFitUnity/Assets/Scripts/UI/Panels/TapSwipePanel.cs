using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

[Serializable]
public class TapSwipePanel : Panel 
{
    static public string LOCK_NAME = "input_lock";
    static public string tempLock = "tempLock";

	protected GestureHelper.OnTap tapHandler = null;
    protected GestureHelper.OnSwipeLeft leftHandler = null;
    protected GestureHelper.OnSwipeRight rightHandler = null;
    protected GestureHelper.UpSwipe upHandler = null;
    protected GestureHelper.DownSwipe downHandler = null;
	
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
        NewOutput("OnSwipeUp", "Flow");
        NewOutput("OnSwipeDown", "Flow");

        UpdateSize();
    }

    public override void RebuildConnections()
    {

        base.RebuildConnections();

        NewOutput("OnTap", "Flow");
        NewOutput("OnSwipeLeft", "Flow");
        NewOutput("OnSwipeRight", "Flow");
        NewOutput("OnSwipeUp", "Flow");
        NewOutput("OnSwipeDown", "Flow");

        UpdateSize();
    }
	
	public override void EnterStart()
    {
        base.EnterStart();
        DefineHandlers();

        if (tapHandler != null)     GestureHelper.onTap         += tapHandler;
        if (leftHandler != null)    GestureHelper.onSwipeLeft   += leftHandler;
        if (rightHandler != null)   GestureHelper.onSwipeRight  += rightHandler;
        if (upHandler != null)      GestureHelper.onSwipeUp     += upHandler;
        if (downHandler != null)    GestureHelper.onSwipeDown   += downHandler;
    }

    public override void ExitStart()
    {
 	     base.ExitStart();

         if (tapHandler != null)    GestureHelper.onTap         -= tapHandler;
         if (leftHandler != null)   GestureHelper.onSwipeLeft   -= leftHandler;
         if (rightHandler != null)  GestureHelper.onSwipeRight  -= rightHandler;
         if (upHandler != null)     GestureHelper.onSwipeUp     -= upHandler;
         if (downHandler != null)   GestureHelper.onSwipeDown   -= downHandler;
    }

    public virtual void DefineHandlers()
    {
        tapHandler = new GestureHelper.OnTap(() => 
        {
            if (DataVault.GetString(LOCK_NAME) != string.Empty) return;

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
            if (DataVault.GetString(LOCK_NAME) != string.Empty) return;

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
            if (DataVault.GetString(LOCK_NAME) != string.Empty) return;

            if (Outputs.Count > 0 && parentMachine != null)
            {
                GConnector gConect = Outputs.Find(r => r.Name == "OnSwipeRight");
                if (gConect != null)
                {
                    ConnectionWithCall(gConect, null);
                }
            }
        });

        upHandler = new GestureHelper.UpSwipe(() =>
        {
            if (DataVault.GetString(LOCK_NAME) != string.Empty) return;

            if (Outputs.Count > 0 && parentMachine != null)
            {
                GConnector gConect = Outputs.Find(r => r.Name == "OnSwipeUp");
                if (gConect != null)
                {
                    ConnectionWithCall(gConect, null);
                }
            }
        });

        downHandler = new GestureHelper.DownSwipe(() =>
        {
            if (DataVault.GetString(LOCK_NAME) != string.Empty) return;

            if (Outputs.Count > 0 && parentMachine != null)
            {
                GConnector gConect = Outputs.Find(r => r.Name == "OnSwipeDown");
                if (gConect != null)
                {
                    ConnectionWithCall(gConect, null);
                }
            }
        });	
    }

    public override void StateUpdate()
    {
        base.StateUpdate();

        //any tap&swipe screen can release previous frame lock. 
        //lock survives one frame because we don't want to any event be handled in same frame release have been registered 
        //(effectively event being handled by more screens. like pause screen and then game at once)
        if (DataVault.GetString(LOCK_NAME) == tempLock)
        {
            DataVault.Set(LOCK_NAME, string.Empty);
        }

    }
}
