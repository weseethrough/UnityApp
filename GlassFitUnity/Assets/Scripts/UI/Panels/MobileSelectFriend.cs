using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;
using System.Threading;
using System;
using System.Collections.Generic;

using Newtonsoft.Json;
using RaceYourself.Models;

[Serializable]
public class MobileSelectFriend : MobilePanel 
{
	List<RaceYourself.Models.Friend> betaFriends;
    List<RaceYourself.Models.Friend> friendsData;

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

        friendsData = Platform.Instance.Friends();

		betaFriends = new List<RaceYourself.Models.Friend>();

		for(int i=0; i<friendsData.Count; i++) {
			if(friendsData[i].userId != null) {
				betaFriends.Add(friendsData[i]);
				friendsData.Remove(friendsData[i]);
				i--;
			}
		}

		if(betaFriends != null && betaFriends.Count > 0) {
			for(int i=0; i<betaFriends.Count; i++) {
				string betaButtonName = "challenge" + i;
				AddButtonData(betaButtonName, betaFriends[i].name, "", ListButtonData.ButtonFormat.ChallengeButton, GetConnection("ChallengeButton"));
				Platform.Instance.RemoteTextureManager.LoadImage("http://graph.facebook.com/" + betaFriends[i].uid + "/picture", betaButtonName, (tex, buttonId) => {
					
					UnityEngine.Debug.Log("MobileSelectPanel: image is loaded, finding button " + buttonId);
					
					GameObject button = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, buttonId);
					if(button != null) {
						UnityEngine.Debug.Log("MobileSelectPanel: button found, setting picture");
						button.GetComponentInChildren<UITexture>().mainTexture = tex;
					}
				});
			}
		}

		DataVault.Remove("invite_codes");
		Platform.Instance.partner.StartCoroutine(Platform.Instance.api.get("invites", body => {
			
			List<Invite> invites = JsonConvert.DeserializeObject<RaceYourself.API.ListResponse<RaceYourself.Models.Invite>>(body).response;		
			DataVault.Set("invite_codes", invites);
		})) ;

        if (friendsData != null)
        {        
			friendsData.Sort((t1, t2) => t1.name.CompareTo(t2.name));

			UnityEngine.Debug.Log("MobileSelectPanel: there are " + friendsData.Count);
            for (int i = 0; i < friendsData.Count; i++)
            {
				string buttonName = "invite" + i;
				AddButtonData(buttonName, friendsData[i].name, "", ListButtonData.ButtonFormat.InviteButton, GetConnection("InviteButton"));
				Platform.Instance.RemoteTextureManager.LoadImage("http://graph.facebook.com/" + friendsData[i].uid + "/picture", buttonName, (tex, buttonId) => {

					UnityEngine.Debug.Log("MobileSelectPanel: image is loaded, finding button " + buttonId);

					GameObject button = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, buttonId);
					if(button != null) {
						UnityEngine.Debug.Log("MobileSelectPanel: button found, setting picture");
						button.GetComponentInChildren<UITexture>().mainTexture = tex;
					}
				});
            }

			AddButtonData ("ImportButton", "", "", ListButtonData.ButtonFormat.ImportButton, GetConnection("ImportButton"));
            
            if (list != null)
            {            
                list.SetParent(this);
                list.RebuildList();
            }
        }
    }

	public GConnector GetConnection(string connectionName) {
		GConnector gc = Outputs.Find(r => r.Name == connectionName);
		if (gc == null)
		{
			UnityEngine.Debug.LogError("MobileSelectPanel: error finding connection - " + connectionName);
		}

		DataVault.Set("facebook_message", "Connect to Facebook");

		return gc;
	}
	
	public override void OnClick(FlowButton button)
    {
        if (button != null && friendsData != null)
        {
			if(button.name != "ImportButton") {
				if(button.name.Contains("invite")) {
					List<Invite> invites = (List<Invite>)DataVault.Get("invite_codes");
					Invite unusedInvite = invites.Find(x => x.used_at == null);
					if(unusedInvite == null) {
						return;
					}
					string prefix = "invite";
					string index = button.name.Substring(prefix.Length);
					int i = Convert.ToInt32(index);
					DataVault.Set("chosen_friend", friendsData[i]);
					Debug.Log("chosen_friend set to " + friendsData[i].name);
				} else if(button.name.Contains("challenge")) {
					string prefix = "challenge";
					string index = button.name.Substring(prefix.Length);
					int i = Convert.ToInt32(index);
					DataVault.Set("chosen_friend", betaFriends[i]);
					Debug.Log("beta chosen_friend set to " + betaFriends[i].name);
				}

			}
		}
		else
        {
            return;
        }
        
        base.OnClick(button);
    }

}
