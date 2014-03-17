using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

[Serializable]
public class GuardPanel : Panel {
	public string guardKey = null;
	
	private GestureHelper.OnTap tapHandler = null;
	private GestureHelper.OnBack quitHandler = null;
	
	public GuardPanel() {}
	public GuardPanel(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt) {
	}
	
	public override void EnterStart()
    {		
		if (guardKey == null) {
        	GParameter gName = Parameters.Find(r => r.Key == "Name");
			if (gName != null) guardKey = "guarded_"+gName.Value.ToLower().Replace(" ", "_");
			else guardKey = "guarded_default";
		}
		object go = DataVault.Get(guardKey);
		if (go == null) go = false;
		bool guard = (bool)go;
		
        if (Outputs.Count > 0 && parentMachine != null)
        {
            if (guard == true) {
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
			GestureHelper.onBack -= quitHandler;
			DataVault.Set(guardKey, true);
			DataVault.SetPersistency(guardKey, true);
			DataVault.SaveToBlob();
			UnityEngine.Debug.Log("GuardPanel: " + guardKey + " state save");
			
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
		
		quitHandler = new GestureHelper.OnBack(() => {
				Application.Quit();
		});
		GestureHelper.onBack += quitHandler;
		
		base.EnterStart();
	}
	
	public override void StateUpdate ()
	{
		base.StateUpdate ();
		
		if(Input.GetKeyDown(KeyCode.Escape)) {
			Application.Quit();
		}
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
