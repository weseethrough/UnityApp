using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

/// <summary>
/// Displays friend list with available challenges
/// </summary>
public class FriendList : UIComponentSettings
{
    const string BUTTON_BASE_NAME = "Friend1";    

    private GameObject buttonBaseInstance;
    private List<GameObject> friendButtons;

    private bool threadComplete = false;
    private bool buttonsCreated = false;
    private static List<ChallengeNotification> challengeNotifications;


    void Start()
    {
        challengeNotifications = new List<ChallengeNotification>();

        buttonBaseInstance = GameObject.Find(BUTTON_BASE_NAME);
        if (buttonBaseInstance == null)
        {
            Debug.Log("button base not found");
        }

       // buttonBaseInstance.SetActive(false);

#if UNITY_EDITOR
        challengeNotifications = new List<ChallengeNotification>();

        for (int i=0; i<12; i++)
        {
            User user = new User(i, "User"+i, "User"+i);
            ChallengeNotification cn = new ChallengeNotification(null, null, user, null);
            challengeNotifications.Add(cn);
        }
        threadComplete = true;
#else
        GetChallenges();
#endif
    }

    void Update()
    {
        if (!threadComplete || buttonsCreated)
        {
            return;
        }

        friendButtons = new List<GameObject>();
        //List<Friend> friendsData = Platform.Instance.Friends();
        //buttonBaseInstance.SetActive(true);

        for (int i = 0; i < challengeNotifications.Count; i++)
        {
            GameObject friend;
            if (i==0)
            {
                friend = buttonBaseInstance;
            }
            else
            {
                friend = GameObject.Instantiate(buttonBaseInstance) as GameObject;
            }

            friend.transform.parent = buttonBaseInstance.transform.parent;
            Vector3 pos = friend.transform.localPosition;
            friend.name = "Friend" + i;
           // pos.y -= 120 * i;
            friend.transform.localPosition = pos;

            friendButtons.Add(friend);

            GameObjectUtils.SetTextOnLabelInChildren(friend, "friendName", challengeNotifications[i].GetName());
            GameObjectUtils.SetTextOnLabelInChildren(friend, "distance", ""+ challengeNotifications[i].GetDistance());            
        }

        UIGrid grid = buttonBaseInstance.transform.parent.gameObject.GetComponent<UIGrid>();
        if (grid != null)
        {
            grid.Reposition();
        }


        //buttonBaseInstance.SetActive(false);
        buttonsCreated = true;
    }

    public void GetChallenges()
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

        Platform.OnSync shandler = null;
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
                            if (string.Equals(notification.node["type"], "challenge"))
                            {
                                int challengerId = notification.node["from"].AsInt;
                                if (challengerId == null) continue;
                                string challengeId = notification.node["challenge_id"].ToString();
                                if (challengeId == null || challengeId.Length == 0) continue;
                                if (challengeId.Contains("$oid")) challengeId = notification.node["challenge_id"]["$oid"].ToString();
                                challengeId = challengeId.Replace("\"", "");
                                Challenge potential = Platform.Instance.FetchChallenge(challengeId);
                                if (potential is DistanceChallenge)
                                {
                                    User user = Platform.Instance.GetUser(challengerId);
                                    //			UnityEngine.Debug.Log("ChallengeNotification: getting first track");
                                    UnityEngine.Debug.Log("ChallengePanel: getting track");
                                    Track track = potential.UserTrack(user.id);
                                    UnityEngine.Debug.Log("ChallengePanel: fetching track using previous");
                                    if (track == null) continue;
                                    Track realTrack = Platform.Instance.FetchTrack(track.deviceId, track.trackId);

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
                        UnityEngine.Debug.Log("ChallengePanel: Adding hexes");

                        DataVault.Set("challenge_notifications", challengeNotifications);
                    }
                });
                DataVault.Set("loaderthread", loaderThread);
                loaderThread.Start();
            }
        });
        Platform.Instance.onSync += shandler;

    }
}
