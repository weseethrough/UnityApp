using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

[Serializable]
public class PausePanel : TapSwipePanel {
	
    static protected string screenInputLock = "Pause";

	public PausePanel() {}
    public PausePanel(SerializationInfo info, StreamingContext ctxt)
        : base(info, ctxt)
    {
	}

    public override void EnterStart()
    {
        base.EnterStart();

        physicalWidgetRoot.SetActive(false);
    }

    public override void DefineHandlers()
    {
        tapHandler = new GestureHelper.OnTap(() => 
        {
            if (DataVault.GetString(LOCK_NAME) != string.Empty &&
                DataVault.GetString(LOCK_NAME) != screenInputLock)
            {
                return;
            }

            if (physicalWidgetRoot.activeSelf == true )
            {
                physicalWidgetRoot.SetActive(false);
                DataVault.Set(LOCK_NAME, tempLock);
            }
		});

        downHandler = new GestureHelper.DownSwipe(() =>
        {
            if (DataVault.GetString(LOCK_NAME) != string.Empty &&
                DataVault.GetString(LOCK_NAME) != screenInputLock)
            {
                return;
            }

            if (physicalWidgetRoot.activeSelf != true)
            {
                physicalWidgetRoot.SetActive(true);
                DataVault.Set(LOCK_NAME, screenInputLock); 
            }
            else
            {
                if (Outputs.Count > 0 && parentMachine != null)
                {
                    GConnector gConect = Outputs.Find(r => r.Name == "OnSwipeDown");
                    if (gConect != null)
                    {
                        ConnectionWithCall(gConect, null);
                    }
                }
            }
        });	
    }

}
