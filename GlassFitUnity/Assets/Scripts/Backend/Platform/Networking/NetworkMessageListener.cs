using System;
using UnityEngine;
using RaceYourself.Models;
using SimpleJSON;
using Sqo;
using SiaqodbUtils;

public class NetworkMessageListener : MonoBehaviour
{
    private Log log = new Log("NetworkMessageListener");
    private Siaqodb db = DatabaseFactory.GetInstance();

    // Events
    public delegate void OnAuthenticated(bool success);
    public OnAuthenticated onAuthenticated = null;
    public delegate void OnSync(string message);
    public OnSync onSync = null;
    public delegate void OnSyncProgress(string message);
    public OnSyncProgress onSyncProgress = null;

    // TODO move OnRegistered out of here - this class is otherwise appropriate as an API for 3rd parties
    public delegate void OnRegistered(string message);
    public OnRegistered onDeviceRegistered = null;
    public delegate void OnGroupCreated(int groupId);
    public OnGroupCreated onGroupCreated = null;

    // Transient data
    protected string intent = "";
    public bool authenticated = false;    // Are we authenticated? Note: we mark it false at init and true when any auth call passes


    void Awake()
    {
        log.info("Awake()");
        authenticated = false;
        DontDestroyOnLoad(gameObject);
    }

    /// Message receivers
    public void OnAuthentication(string message) {
        if (string.Equals(message, "Success")) {
            if (authenticated == false) {
                User me = Platform.Instance.User();
                if (me != null) MessageWidget.AddMessage("Logged in", "Welcome " + me.name, "settings");
            }
            authenticated = true;
            if (onAuthenticated != null) onAuthenticated(true);
        }
        if (string.Equals(message, "Failure")) {
            if (onAuthenticated != null) onAuthenticated(false);
        }
        if (string.Equals(message, "OutOfBand")) {
            if (onAuthenticated != null) onAuthenticated(false);
            // TODO: Use confirmation dialog instead of message
            MessageWidget.AddMessage("Notice", "Please use the web interface to link your account to this provider", "settings");
        }
        log.info("Authentication response: " + message.ToLower()); 
    }

    public void OnSynchronization(string message) {
        UnityEngine.Debug.Log("Platform: synchronize finished with " + message);
        Platform.Instance.lastSync = DateTime.Now;
        if (onSync != null) onSync(message);
    }

    public void OnSynchronizationProgress(string message) {
        if (onSyncProgress != null) onSyncProgress(message);
    }

    public void OnRegistration(string message) {
        if (onDeviceRegistered != null) onDeviceRegistered(message);
    }

    public void OnActionIntent(string message) {
        UnityEngine.Debug.Log("Platform: action " + message); 
        MessageWidget.AddMessage("Internal", "App opened with intent " + message, "settings");
        intent = message;
    }

    public void OnUserMessage(string message) {
        JSONNode json = JSON.Parse(message);
        MessageWidget.AddMessage("Network", "<" + json["from"] + "> " + json["data"], "settings"); // DEBUG
    }

    public void OnGroupMessage(string message) {
        JSONNode json = JSON.Parse(message);
        MessageWidget.AddMessage("Network", "#" + json["group"] + " <" + json["from"] + "> " + json["data"], "settings"); // DEBUG
    }

    public void OnGroupCreation(string message) {
        if (onGroupCreated != null) onGroupCreated(int.Parse(message));
        // TODO: Potential hanging deferral. What do we do if socket is disconnected before a group is created?
    }

    // Called by native platform with a push notification id
    public void OnPushId(string id) {
        if (db != null && Platform.Instance.api != null) {
            Device self = Platform.Instance.Device();
            self.push_id = id;
            db.StoreObject(self);
        }
    } 

    public string GetIntent()
    {
        return intent;
    }


}

