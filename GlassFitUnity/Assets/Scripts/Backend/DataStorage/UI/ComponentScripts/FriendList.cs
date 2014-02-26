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

    private GameObject ftcBaseButtonInstance;
    private GameObject incChalBaseButtonInstance;
    private List<GameObject> friendButtons;

    private bool threadComplete = false;
    private bool buttonsCreated = false;
    private static List<ChallengeNotification> challengeNotifications;

    private GameObject friendsToChallenge;
    private GameObject friendsToChallengeHeader;
    private GameObject incommingChallenges;
    private GameObject incommingChallengesHeader;
    private GameObject lineDivider;

    private GraphComponent gComponent;

    void Start()
    {
        gComponent = GameObject.FindObjectOfType(typeof(GraphComponent)) as GraphComponent;

        incommingChallengesHeader   = GameObject.Find("IncommingChallengesLine");
        incommingChallenges         = GameObject.Find("IncommingChallenges");
        friendsToChallengeHeader    = GameObject.Find("FriendsToChallengeLine");
        friendsToChallenge          = GameObject.Find("FriendsToChallenge");
        lineDivider                 = GameObject.Find("friendListLineDivider");

        challengeNotifications      = new List<ChallengeNotification>();

        ftcBaseButtonInstance = friendsToChallenge.transform.GetChild(0).gameObject;
        incChalBaseButtonInstance = incommingChallenges.transform.GetChild(0).gameObject;

        ftcBaseButtonInstance.SetActive(false);
        incChalBaseButtonInstance.SetActive(false);

        if (incChalBaseButtonInstance == null)
        {
            Debug.Log("button base not found");
        }

        incChalBaseButtonInstance.SetActive(false);
        GetChallenges();
    }

    void Update()
    {
        if (!threadComplete || buttonsCreated)
        {
            return;
        }

        Panel parentPanel = FlowStateMachine.GetCurrentFlowState() as Panel;

        GConnector sendExit = parentPanel.Outputs.Find(r => r.Name == "sendExit");
        GConnector challengeExit = parentPanel.Outputs.Find(r => r.Name == "challengeExit");

        friendButtons = new List<GameObject>();
        buttonsCreated = true;                

        Vector3 headerPos = incommingChallengesHeader.transform.localPosition;
        float yPosForNextElement = headerPos.y;

        yPosForNextElement -= 30.0f;

        for (int i = 0; i < challengeNotifications.Count; i++)
        {
            GameObject friend;
            if (i==0)
            {
                incChalBaseButtonInstance.SetActive(true);
                friend = incChalBaseButtonInstance;
            }
            else
            {
                friend = GameObject.Instantiate(incChalBaseButtonInstance) as GameObject;
            }

            friend.transform.parent = incChalBaseButtonInstance.transform.parent;            
            friend.name = "CNFriend" + i;                        

            friendButtons.Add(friend);

            UIButton button = friend.GetComponentInChildren<UIButton>();
            if (button != null)
            {
                FlowButton fb = button.gameObject.AddComponent<FlowButton>();
                fb.owner    = parentPanel;
                fb.name     = friend.name;

                GConnector gc = parentPanel.NewOutput(friend.name, "Flow");
                gc.EventFunction = "SetChallenge";

                if (challengeExit.Link.Count > 0)
                {
                    gComponent.Data.Connect(gc, challengeExit.Link[0]);
                }
            }

            GameObjectUtils.SetTextOnLabelInChildren(friend, "friendName", challengeNotifications[i].GetName());
            GameObjectUtils.SetTextOnLabelInChildren(friend, "distance", ""+ challengeNotifications[i].GetDistance());

            yPosForNextElement -= 120;            
        }

        UIGrid grid = incommingChallenges.GetComponent<UIGrid>();
        if (grid != null)
        {
            grid.Reposition();
        }        

        yPosForNextElement -= 30.0f;  

        headerPos = friendsToChallengeHeader.transform.localPosition;
        headerPos.y = yPosForNextElement;
        friendsToChallengeHeader.transform.localPosition = headerPos;

        yPosForNextElement -= 30.0f;

        headerPos = friendsToChallenge.transform.localPosition;
        headerPos.y = yPosForNextElement;
        friendsToChallenge.transform.localPosition = headerPos;

        List<Friend> friendsData = Platform.Instance.Friends();

        for (int i = 0; i < friendsData.Count; i++)
        {
            GameObject friend;
            if (i == 0)
            {
                ftcBaseButtonInstance.SetActive(true);
                friend = ftcBaseButtonInstance;
            }
            else
            {
                friend = GameObject.Instantiate(ftcBaseButtonInstance) as GameObject;
            }

            friend.transform.parent = ftcBaseButtonInstance.transform.parent;            
            friend.name = "FTCFriend" + i;            

            friendButtons.Add(friend);

            GameObjectUtils.SetTextOnLabelInChildren(friend, "friendName", friendsData[i].name);

            UIButton button = friend.GetComponentInChildren<UIButton>();
            if (button != null)
            {
                FlowButton fb = button.gameObject.AddComponent<FlowButton>();
                fb.owner    = parentPanel;
                fb.name     = friend.name;

                GConnector gc = parentPanel.NewOutput(friend.name, "Flow");
    //            gc.EventFunction = "SetFriend";

                if (sendExit.Link.Count > 0)
                {
                    gComponent.Data.Connect(gc, sendExit.Link[0]);
                }
            }

            yPosForNextElement -= 120.0f;
        }

        grid = friendsToChallenge.GetComponent<UIGrid>();
        if (grid != null)
        {
            grid.Reposition();
        }
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
        Platform.Instance.SyncToServer();
    }
}
