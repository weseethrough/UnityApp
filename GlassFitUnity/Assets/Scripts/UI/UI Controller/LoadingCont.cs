using UnityEngine;
using System.Collections;
using System.Threading;
using System.Collections.Generic;

using RaceYourself.Models;

public class LoadingCont : MonoBehaviour {
	
	private float rotate = 0;
    private NetworkMessageListener.OnSyncProgress progressHandler = new NetworkMessageListener.OnSyncProgress((message) => {
		DataVault.Set("loading", "Synchronizing with server: " + message);
	});
	
	// Use this for initialization
	void Start () {
        Platform.Instance.NetworkMessageListener.onSyncProgress += progressHandler;
		AcceptChallenges();
	}
	
	void AcceptChallenges() {
		Debug.Log("AcceptChallenges: click");
		if (!Platform.Instance.HasPermissions("any", "login")) {			
			// Restart function once authenticated
            NetworkMessageListener.OnAuthenticated handler = null;
            handler = new NetworkMessageListener.OnAuthenticated((authenticated) => {
                Platform.Instance.NetworkMessageListener.onAuthenticated -= handler;
				if (authenticated) {
					AcceptChallenges();
				}
			});
            Platform.Instance.NetworkMessageListener.onAuthenticated += handler;	
			
			Platform.Instance.Authorize("any", "login");
			return;
		}
		
		FlowStateBase fs = FlowStateMachine.GetCurrentFlowState();
		GConnectorBase race = fs.Outputs.Find(r => r.Name == "completeExit");
		GConnectorBase back = fs.Outputs.Find(r => r.Name == "failedExit");
        NetworkMessageListener.OnSync shandler = null;
        shandler = new NetworkMessageListener.OnSync((message) => {
            Platform.Instance.NetworkMessageListener.onSync -= shandler;
			
			lock(DataVault.data) {
				if (DataVault.Get("loaderthread") != null) return;
				Thread loaderThread = new Thread(() =>
                                                       {
#if UNITY_ANDROID
					AndroidJNI.AttachCurrentThread();
#endif
                                                           try {
						// Reset world
						Platform.Instance.ResetTargets();
						DataVault.Remove("challenges");
						DataVault.Remove("finish");
						
						List<Challenge> relevant = new List<Challenge>();		
						int? finish = null;
						
						Notification[] notifications = Platform.Instance.Notifications();
						foreach (Notification notification in notifications) {
							if (notification.read) continue;
							if (string.Equals(notification.message.type, "challenge")) {
								int challengerId = notification.message.from;
								string challengeId = notification.message.challenge_id;
								if (challengeId == null || challengeId.Length == 0) continue;
								
								Debug.Log("AcceptChallenges: " + challengeId + " from " + challengerId);
								
								DataVault.Set("loading", "Please wait while we fetch a challenge");
								Challenge potential = Platform.Instance.FetchChallenge(challengeId);
								if (potential == null) continue;
								if (potential is DistanceChallenge) {
									DistanceChallenge challenge = potential as DistanceChallenge;					
									
									DataVault.Set("loading", "Please wait while we fetch a track");
									var attempt = challenge.attempts.Find(a => a.user_id == challengerId);
									if (attempt != null) {
										var track = Platform.Instance.FetchTrack(attempt.device_id, attempt.track_id); // Make sure we have the track in the local db
										TargetTracker tracker = Platform.Instance.CreateTargetTracker(track.deviceId, track.trackId);
										User challenger = Platform.Instance.GetUser(challengerId);
										tracker.name = challenger.username;
										if (tracker.name == null || tracker.name.Length == 0) tracker.name = challenger.name;
									} // else race leader/friends/creator?
				
									relevant.Add(challenge); 					
									if (!finish.HasValue || finish.Value < challenge.distance) finish = challenge.distance;					
								}
							}
						}		
						if (!finish.HasValue || relevant.Count == 0) {
							MessageWidget.AddMessage("Challenges", "No relevant challenges", "settings");
							FlowStateBase.FollowBackLink();
							return;
						}
						
						MessageWidget.AddMessage("Challenges", "Accepted " + relevant.Count + " challenges", "settings");
						Debug.Log("AcceptChallenges: Accepted " + relevant.Count + " challenges");
						DataVault.Set("challenges", relevant);
						DataVault.Set("finish", finish.Value);
						
						AutoFade.LoadLevel(1, 0.1f, 1.0f, Color.black);
						
						// Follow connection since the function may have been called asynchronously
						fs.parentMachine.FollowConnection(race);
					} finally {
						DataVault.Remove("loaderthread");
#if UNITY_ANDROID
						AndroidJNI.DetachCurrentThread();
#endif
                    }
				});
				DataVault.Set("loaderthread", loaderThread);
				loaderThread.Start();
			}
		});
        Platform.Instance.NetworkMessageListener.onSync += shandler;
		
		DataVault.Set("loading", "Please wait while we sync the database");
		Platform.Instance.SyncToServer();
		
		return;		
	}
	
	// Update is called once per frame
	void Update () {
		rotate += 10 * Time.deltaTime;
		
		transform.rotation = Quaternion.Euler(0, 0, rotate);
	}
	
	void OnDisable () {
        Platform.Instance.NetworkMessageListener.onSyncProgress -= progressHandler;
	}
	
	void OnDestroy () {
        Platform.Instance.NetworkMessageListener.onSyncProgress -= progressHandler;
	}
}
