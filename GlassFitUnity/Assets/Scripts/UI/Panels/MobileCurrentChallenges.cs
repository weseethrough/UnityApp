using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;
using System.Threading;
using System;
using System.Collections.Generic;
using RaceYourself.Models;

[Serializable]
public class MobileCurrentChallenges : MobilePanel 
{

    private bool threadComplete = false;
    private bool buttonsCreated = false;
    private List<ChallengeNotification> challengeNotifications = new List<ChallengeNotification>();

    private NetworkMessageListener.OnSync shandler = null;

    public MobileCurrentChallenges() { }
    public MobileCurrentChallenges(SerializationInfo info, StreamingContext ctxt)
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
            return "MobileCurrentChallenges: " + gName.Value;
        }
        return "MobileCurrentChallenges: UnInitialzied";
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
            list.SetTitle("Current Challenges");            
        }

        GetChallenges();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override bool EnterUpdate()
    {
        base.EnterUpdate();
     
        if (threadComplete)
        {
            Debug.Log("challengeNotifications contains " + challengeNotifications.Count + " challenge(-s)");

            GConnector baseConnection = GetBaseButtonConnection();

            //AddBackButtonData();
            
            for (int i = 0; i < challengeNotifications.Count; i++)
            {
                AddButtonData("button" + i, challengeNotifications[i].GetName(), "", ListButtonData.ButtonFormat.ButtonNormalPrototype, baseConnection);

                ListButtonData data = new ListButtonData();
                data.textNormal = challengeNotifications[i].GetName();
                data.buttonName = "button" + i;
                buttonData.Add(data);
            }

            MobileList list = physicalWidgetRoot.GetComponentInChildren<MobileList>();
            if (list != null)
            {
                list.SetParent(this);
            }
            return true;
        }

        return false;
    }

    public override void ExitStart()
    {
        base.ExitStart();
        
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    /*public void GetChallenges()
    {
        if (!Platform.Instance.HasPermissions("any", "login"))
        {
            // Restart function once authenticated
            Platform.OnAuthenticated handler = null;
            handler = new Platform.OnAuthenticated((authenticated) =>
            {
                Platform.Instance.onAuthenticated -= handler;
                if (authenticated)
                {
                    GetChallenges();
                }
            });
            Platform.Instance.onAuthenticated += handler;

            UnityEngine.Debug.Log("ChallengePanel: Need to authenticate");
            DataVault.Set("tutorial_hint", "Authenticating device");

            Platform.Instance.Authorize("any", "login");
            return;
        }
        
        shandler = new Platform.OnSync((message) =>
        {
            Platform.Instance.onSync -= shandler;
            UnityEngine.Debug.Log("ChallengePanel: about to lock datavault");
            DataVault.Set("tutorial_hint", "Getting challenges and friends");
            lock (DataVault.data)
            {
                if (DataVault.Get("loaderthread") != null) return;
                UnityEngine.Debug.Log("ChallengePanel: starting thread");
                Thread loaderThread = new Thread(() =>
                {
#if !UNITY_EDITOR
					AndroidJNI.AttachCurrentThread();
#endif
                    try
                    {
                        UnityEngine.Debug.Log("ChallengePanel: getting notifications");
                        RaceYourself.Models.Notification[] notifications = Platform.Instance.Notifications();
                        UnityEngine.Debug.Log("ChallengePanel: notifications obtained");
                        foreach (RaceYourself.Models.Notification notification in notifications)
                        {
                            UnityEngine.Debug.Log("ChallengePanel: notification has been found");
                            if (notification.read)
                            {
                                UnityEngine.Debug.Log("ChallengePanel: notification set to read");
                                continue;
                            }
                            UnityEngine.Debug.Log("ChallengePanel: notification not read");
                            if (string.Equals(notification.message, "challenge"))
                            {
                                int challengerId = notification.id;
                                if (challengerId == null) continue;
                                string challengeId = notification .node["challenge_id"].ToString();
                                if (challengeId == null || challengeId.Length == 0) continue;
                                if (challengeId.Contains("$oid")) challengeId = notification.node["challenge_id"]["$oid"].ToString();
                                challengeId = challengeId.Replace("\"", "");
                                RaceYourself.Models.Challenge potential = Platform.Instance.FetchChallenge(challengeId);
                                if (potential is DistanceChallenge)
                                {
                                    RaceYourself.Models.User user = Platform.Instance.GetUser(challengerId);
                                    //			UnityEngine.Debug.Log("ChallengeNotification: getting first track");
                                    UnityEngine.Debug.Log("ChallengePanel: getting track");
                                    RaceYourself.Models.Track track = potential.UserTrack(user.id);
                                    UnityEngine.Debug.Log("ChallengePanel: fetching track using previous");
                                    if (track == null) continue;
                                    RaceYourself.Models.Track realTrack = Platform.Instance.FetchTrack(track.deviceId, track.trackId);

                                    UnityEngine.Debug.Log("ChallengePanel: creating challenge notification");
                                    ChallengeNotification challengeNot = new ChallengeNotification(notification, potential, user, realTrack);
                                    challengeNotifications.Add(challengeNot);
                                }
                            }
                        }
                    }
                    finally
                    {
                        UnityEngine.Debug.Log("ChallengePanel: removing loaderthread");
                        DataVault.Remove("loaderthread");

                        UnityEngine.Debug.Log("ChallengePanel: thread complete true");
                        threadComplete = true;
#if !UNITY_EDITOR
						UnityEngine.Debug.Log("ChallengePanel: detaching thread");
						AndroidJNI.DetachCurrentThread();
#endif
                        UnityEngine.Debug.Log("ChallengePanel: Setting data to notification list");

                        DataVault.Set("challenge_notifications", challengeNotifications);
                    }
                });
                DataVault.Set("loaderthread", loaderThread);
                loaderThread.Start();
            }
        });
        Platform.Instance.onSync += shandler;
        Platform.Instance.SyncToServer();
    }*/

    public void GetChallenges()
    {
        if (!Platform.Instance.HasPermissions("any", "login"))
        {
            // Restart function once authenticated
            NetworkMessageListener.OnAuthenticated handler = null;
            handler = new NetworkMessageListener.OnAuthenticated((authenticated) =>
            {
                    Platform.Instance.NetworkMessageListener.onAuthenticated -= handler;
                if (authenticated)
                {
                    GetChallenges();
                }
            });
            Platform.Instance.NetworkMessageListener.onAuthenticated += handler;

            UnityEngine.Debug.Log("ChallengePanel: Need to authenticate");
            DataVault.Set("tutorial_hint", "Authenticating device");

            Platform.Instance.Authorize("any", "login");
            return;
        }

        NetworkMessageListener.OnSync shandler = null;
        shandler = new NetworkMessageListener.OnSync((message) =>
        {
                Platform.Instance.NetworkMessageListener.onSync -= shandler;
            UnityEngine.Debug.Log("ChallengePanel: about to lock datavault");
            DataVault.Set("tutorial_hint", "Getting challenges and friends");
            lock (DataVault.data)
            {
                if (DataVault.Get("loaderthread") != null) return;
                UnityEngine.Debug.Log("ChallengePanel: starting thread");
                Thread loaderThread = new Thread(() =>
                {
#if UNITY_ANDROID
					AndroidJNI.AttachCurrentThread();
#endif
                    try
                    {
                        UnityEngine.Debug.Log("ChallengePanel: getting notifications");
                        Notification[] notifications = Platform.Instance.Notifications();
                        UnityEngine.Debug.Log("ChallengePanel: notifications obtained");
                        foreach (Notification notification in notifications)
                        {
                            UnityEngine.Debug.Log("ChallengePanel: notification has been found");
                            if (notification.read)
                            {
                                UnityEngine.Debug.Log("ChallengePanel: notification set to read");
                                continue;
                            }
                            UnityEngine.Debug.Log("ChallengePanel: notification not read");
                            if (string.Equals(notification.message.type, "challenge"))
                            {
                                int challengerId = notification.message.from;
                                int challengeId = notification.message.challenge_id;
                                Challenge potential = Platform.Instance.FetchChallenge(challengeId);
                                if (potential is DistanceChallenge)
                                {
                                    User user = Platform.Instance.GetUser(challengerId);
                                    //			UnityEngine.Debug.Log("ChallengeNotification: getting first track");
                                    UnityEngine.Debug.Log("ChallengePanel: getting track");
                                    var attempt = potential.attempts.Find(a => a.user_id == user.id);
                                    UnityEngine.Debug.Log("ChallengePanel: fetching track using previous");
                                    if (attempt == null) continue;
                                    Track realTrack = Platform.Instance.FetchTrack(attempt.device_id, attempt.track_id);

                                    UnityEngine.Debug.Log("ChallengePanel: creating challenge notification");
                                    ChallengeNotification challengeNot = new ChallengeNotification(notification, potential, user, realTrack);
                                    challengeNotifications.Add(challengeNot);
                                }
                            }
                        }
                    }
                    finally
                    {
                        UnityEngine.Debug.Log("ChallengePanel: removing loaderthread");
                        DataVault.Remove("loaderthread");

                        UnityEngine.Debug.Log("ChallengePanel: thread complete true");
                        threadComplete = true;
#if UNITY_ANDROID
						UnityEngine.Debug.Log("ChallengePanel: detaching thread");
						AndroidJNI.DetachCurrentThread();
#endif
                        UnityEngine.Debug.Log("ChallengePanel: Adding hexes");

                        DataVault.Set("challenge_notifications", challengeNotifications);
                    }
                });
                DataVault.Set("loaderthread", loaderThread);
                loaderThread.Start();
            }
        });
        Platform.Instance.NetworkMessageListener.onSync += shandler;
        Platform.Instance.SyncToServer();
    }

    public override void OnClick(FlowButton button)
    {

        if (button != null && challengeNotifications != null)
        {
            string prefix = "button";
            string index = button.name.Substring(prefix.Length);
            int i = Convert.ToInt32(index);

            RaceYourself.Models.Track track = challengeNotifications[i].GetTrack();
            UnityEngine.Debug.Log("ButtonFunc: setting track");
            DataVault.Set("current_track", track);
            DataVault.Set("race_type", "challenge");
            DataVault.Set("challenger", challengeNotifications[i].GetName());
            DataVault.Set("current_challenge_notification", challengeNotifications[i]);

        }
        else
        {
            return;
        }

        base.OnClick(button);
    }

}
