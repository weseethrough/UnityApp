using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

using RaceYourself.Models;
using Newtonsoft.Json;

public class MobileFriendInvite : MonoBehaviour {

	GameObject hex;
	GameObject inviteRemainLabel;

	// Use this for initialization
	void Start () {
		Friend chosenFriend = (Friend)DataVault.Get("chosen_friend");

		DataVault.Set("chosen_friend_name", chosenFriend.name);

		DataVault.Set("invite_status", "Sending");

		Panel currentPanel = (Panel)FlowStateMachine.GetCurrentFlowState();

		hex = GameObjectUtils.SearchTreeByName(currentPanel.physicalWidgetRoot, "InviteHex");
		if(hex != null) {
			hex.SetActive(false);
		}
		inviteRemainLabel = GameObjectUtils.SearchTreeByName(currentPanel.physicalWidgetRoot, "InviteRemainLabel");
		if(inviteRemainLabel != null) {
			inviteRemainLabel.SetActive(false);
		}

		List<Invite> invites = (List<Invite>)DataVault.Get("invite_codes");
		Invite unusedInvite = invites.Find(x => x.used_at == null);

		if(unusedInvite != null) {


			Hashtable eventProperties = new Hashtable();
			eventProperties.Add("event_name", "invite");
			eventProperties.Add("invite_code", unusedInvite.code);
			eventProperties.Add("provider", "facebook");
			Platform.Instance.LogAnalyticEvent(JsonConvert.SerializeObject(eventProperties));


			string[] toFriend = new string[1];
			toFriend[0] = chosenFriend.uid;

			FB.AppRequest("Hello " + chosenFriend.name + ", I have invited you to the Race Yourself mobile app!", toFriend, "", null, null, unusedInvite.code, "", result => {
				if(result.Error != null) {
					UnityEngine.Debug.Log("MobileFriendInvite: FB error - " + result.Error);
					DataVault.Set("invite_status", "Error!");
				} else {
					DataVault.Set("invite_status", "Invite sent");
					int numInvites = invites.FindAll(x => x.used_at == null).Count;
					DataVault.Set ("invites_remain", numInvites);
					if(hex != null) {
						hex.SetActive(true);
					}
					if(inviteRemainLabel != null) {
						inviteRemainLabel.SetActive(true);
					}
					// Invite will only be linked to the identity after a sync
					unusedInvite.used_at = DateTime.Now;
					InviteAction action = new InviteAction("invite", chosenFriend.provider, chosenFriend.uid, unusedInvite.code);
					Platform.Instance.QueueAction(JsonConvert.SerializeObject(action));
					
					Platform.Instance.SyncToServer();
				}
			});
		} else {

		}
	}

}
