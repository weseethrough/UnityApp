using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;
using System.Collections.Generic;

public class ChallengePanel : HexPanel {
	
	private GConnector challengeExit;
	private GConnector previousExit;
	private GConnector sendExit;
		
	private GraphComponent gComponent;
	
	List<Notification> challengeNotifications;
	
	// Use this for initialization
	public override void EnterStart ()
	{
		challengeExit = Outputs.Find(r => r.Name == "challengeExit");
		previousExit = Outputs.Find(r => r.Name == "previousExit");
		sendExit = Outputs.Find(r => r.Name == "sendExit");
		
		gComponent = GameObject.FindObjectOfType(typeof(GraphComponent)) as GraphComponent;
		
		GetChallenges();
		
		
		
		base.EnterStart ();
	}
	
	public void GetChallenges() {
/*		  if (!Platform.Instance.HasPermissions("any", "login")) {			
			// Restart function once authenticated
			Platform.OnAuthenticated handler = null;
			handler = new Platform.OnAuthenticated((authenticated) => {
				Platform.Instance.onAuthenticated -= handler;
				if (authenticated) {
					GetChallenges();
				}
			});
			Platform.Instance.onAuthenticated += handler;	
			
			Platform.Instance.Authorize("any", "login");
			return;
		}
		
		Platform.OnSync shandler = null;
		shandler = new Platform.OnSync(() => {
			Platform.Instance.onSync -= shandler;				
				
			lock(DataVault.data) {
				if (DataVault.Get("loaderthread") != null) return;
				Thread loaderThread = new Thread(() => {
#if !UNITY_EDITOR
					AndroidJNI.AttachCurrentThread();
#endif				
					try {
			
						Notification[] notifications = Platform.Instance.Notifications();
						foreach (Notification notification in notifications) {
							if (notification.read) continue;
							
							if (string.Equals(notification.node["type"], "challenge")) {
								int challengerId = notification.node["from"].AsInt;
								if (challengerId == null) continue;
								string challengeId = notification.node["challenge_id"].ToString();
								if (challengeId == null || challengeId.Length == 0) continue;
								if (challengeId.Contains("$oid")) challengeId = notification.node["challenge_id"]["$oid"].ToString();
								challengeId = challengeId.Replace("\"", "");
								Challenge potential = Platform.Instance.FetchChallenge(challengeId);
								if(potential is DistanceChallenge) {
									challengeNotifications.Add(notification);
								}
							}
						}
					}
					
//					int challengerId = notification.node["from"].AsInt;
//					if (challengerId == null) continue;
//					string challengeId = notification.node["challenge_id"].ToString();
//					if (challengeId == null || challengeId.Length == 0) continue;
//					if (challengeId.Contains("$oid")) challengeId = notification.node["challenge_id"]["$oid"].ToString();
//					challengeId = challengeId.Replace("\"", "");
//								
//					Debug.Log("ChallengePanel: Challenge " + challengeId + " from " + challengerId);
//					
//					DataVault.Set("loading", "Please wait while we fetch a challenge");
//					Challenge potential = Platform.Instance.FetchChallenge(challengeId);
//					if (potential == null) continue;
//					if (potential is DistanceChallenge) {
//						DistanceChallenge challenge = potential as DistanceChallenge;					
//								
//						DataVault.Set("loading", "Please wait while we fetch a track");
//						Track track = challenge.UserTrack(challengerId);
//						if (track != null) {
//							Platform.Instance.FetchTrack(track.deviceId, track.trackId); // Make sure we have the track in the local db
//							TargetTracker tracker = Platform.Instance.CreateTargetTracker(track.deviceId, track.trackId);
//							User challenger = Platform.Instance.GetUser(challengerId);
//							tracker.name = challenger.username;
//							if (tracker.name == null || tracker.name.Length == 0) tracker.name = challenger.name;
//						} // else race leader/friends/creator?
//						newChallenges.Add(challenge);		
				//	}
				//}		
					finally {
						DataVault.Remove("loaderthread");
#if !UNITY_EDITOR
						AndroidJNI.DetachCurrentThread();
#endif					
					}
				});
				DataVault.Set("loaderthread", loaderThread);
				loaderThread.Start();
			}
		});
		Platform.Instance.onSync += shandler;
*/
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
