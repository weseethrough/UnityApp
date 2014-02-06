using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

[Serializable]
public class GuardPanel : Panel {
	
	private GestureHelper.OnTap tapHandler = null;
	private GestureHelper.DownSwipe quitHandler = null;
	
	public GuardPanel() {}
	public GuardPanel(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt) {
	}
	
	public override void EnterStart()
    {		
		object go = DataVault.Get("guarded_"+GetDisplayName());
		if (go == null) go = false;
		bool guard = (bool)go;
		
        if (Outputs.Count > 0 && parentMachine != null)
        {
            if (guard != null && guard == true) {
				parentMachine.FollowConnection(Outputs[0]);
				base.EnterStart();
				return;
			}
        }
        else
        {
            Debug.LogError("Dead end GuardPanel");
        }

		tapHandler = new GestureHelper.OnTap(() => {
			GestureHelper.onTap -= tapHandler;		
			GestureHelper.onSwipeDown -= quitHandler;
			DataVault.Set("guarded_"+GetDisplayName(), true);
			DataVault.SetPersistency("guarded_"+GetDisplayName(), true);
			DataVault.SaveToBlob();
			
	        if (Outputs.Count > 0 && parentMachine != null)
	        {
				parentMachine.FollowConnection(Outputs[0]);
				return;
	        }
	        else
	        {
	            Debug.LogError("Dead end GuardPanel tap");
	        }
		});		
		GestureHelper.onTap += tapHandler;		
		
		quitHandler = new GestureHelper.DownSwipe(() => {
			Application.Quit();
		});
		GestureHelper.onSwipeDown += quitHandler;
		
		base.EnterStart();
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
            return "GuardPanel: " + gName.Value;
        }
        return "GuardPanel: UnInitialized";
    }
}
