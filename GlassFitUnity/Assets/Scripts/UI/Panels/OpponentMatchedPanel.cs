using UnityEngine;
using System;
using System.Runtime.Serialization;
using System.Collections;
using RaceYourself.Models;

[Serializable]
public class OpponentMatchedPanel : MobilePanel {

	bool haveOpponentDetails = false;

    public OpponentMatchedPanel() { }
    public OpponentMatchedPanel(SerializationInfo info, StreamingContext ctxt)
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
            return "OpponentMatchedPanel: " + gName.Value;
        }
        return "OpponentMatchedPanel: Uninitialized";
    }

    public override void EnterStart ()
    {
        base.EnterStart ();
		haveOpponentDetails = FillInOpponentDetails();
    }

	public override void StateUpdate()
	{
		base.StateUpdate();

		if(!haveOpponentDetails)
		{
			haveOpponentDetails = FillInOpponentDetails();
		}
	}

	private bool FillInOpponentDetails()
	{
		GameObject rivalGo = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "RivalPicture");

		User rivalUser = (User) DataVault.Get("competitor");

		if(rivalUser != null)
		{
            DataVault.Set("competitor", rivalUser);
			GameObjectUtils.SetTextOnLabelInChildren(rivalGo, "PlayerName", rivalUser.name);

			UITexture profilePicture = rivalGo.GetComponentInChildren<UITexture>();
			
			// TODO load image on transition from previous page with a 'please wait' dialog
			if(profilePicture != null)
			{
				Platform.Instance.RemoteTextureManager.LoadImage(rivalUser.image, "", (tex, text) => {
					profilePicture.mainTexture = tex;
				});
			}

			return true;
		}
		else
		{
			GameObjectUtils.SetTextOnLabelInChildren(rivalGo, "PlayerName", "?");
			return false;
		}
	}
}
