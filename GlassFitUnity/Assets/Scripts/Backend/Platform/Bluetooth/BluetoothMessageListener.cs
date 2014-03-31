using System;
using UnityEngine;
using RaceYourself.Models;
using SimpleJSON;
using Sqo;
using SiaqodbDemo;

public class BluetoothMessageListener : MonoBehaviour
{
    private Log log = new Log("NetworkMessageListener");
    private Siaqodb db = DatabaseFactory.GetInstance();

    protected void OnBluetoothJson(JSONNode json) {
        UnityEngine.Debug.Log("Platform: OnBluetoothJson"); 
        switch(json["action"]) {
        case "LoadLevelFade":
            if (Platform.Instance.IsRemoteDisplay()) {
                //              DataVaultFromJson(json["data"]);
                //              if (json["levelName"] != null) AutoFade.LoadLevel(json["levelName"], 0f, 1.0f, Color.black);            
                //              if (json["levelIndex"] != null) AutoFade.LoadLevel(json["levelIndex"].AsInt, 0f, 1.0f, Color.black);            
            }
            break;
        case "LoadLevelAsync":
            if (Platform.Instance.IsRemoteDisplay()) {
                DataVaultFromJson(json["data"]);
                FlowStateMachine.Restart("Restart Point");
            }
            break;
        case "InitiateSnack":
            if (Platform.Instance.IsRemoteDisplay()) {
                //find SnackRun Object
                SnackRun snackRunGame = (SnackRun)GameObject.FindObjectOfType(typeof(SnackRun));
                string gameID = json["snack_gameID"];
                if(snackRunGame != null)
                {
                    UnityEngine.Debug.Log("Platform: Received InitiateSnack message. Initiating game: " + gameID);
                    snackRunGame.OfferPlayerSnack(gameID);
                }
                else
                {
                    UnityEngine.Debug.LogWarning("Platform: Received InitiateSnack message for " + gameID + " but not currently on a snack run");
                }
            }
            break;
        case "ReturnToMainMenu":
            if(Platform.Instance.IsDisplayRemote()) {
                FlowStateMachine.Restart("Start Point");    
            }
            break;
        case "OnSnackFinished":
            if(Platform.Instance.IsDisplayRemote()) {
                UnityEngine.Debug.Log("Platform: Received bluetooth snack finished message");
                //find SnackRemoteControlPanel
                SnackRemoteControlPanel remotePanel = (SnackRemoteControlPanel)FlowStateMachine.GetCurrentFlowState();
                if(remotePanel != null)
                {
                    remotePanel.ClearCurrentSnackHex();
                }
                else
                {
                    UnityEngine.Debug.LogWarning("Platform: Couldn't find Snack remote panel");
                }
            }
            break;
        default:
            UnityEngine.Debug.Log("Platform: unknown Bluetooth message: " + json);
            break;
        }
            
        // TODO: Start challenge
        // TODO: Toggle outdoor/indoor
    }

    public void OnBluetoothConnect(string message) {
        MessageWidget.AddMessage("Bluetooth", message, "settings");
    }

    public void OnBluetoothMessage(string message) {
        //      MessageWidget.AddMessage("Bluetooth", message, "settings"); // DEBUG
        UnityEngine.Debug.Log("Platform: OnBluetoothMessage " + message.Length + "B"); 
        JSONNode json = JSON.Parse(message);
        OnBluetoothJson(json);
    }

    protected void DataVaultFromJson(JSONNode json) {
        JSONNode track = json["current_track"];
        if (track != null) {
            Track t = Platform.Instance.FetchTrack(track["device_id"].AsInt, track["track_id"].AsInt);
            if (t != null) DataVault.Set("current_track", t);
            else track = null;
        } 
        if (track == null) DataVault.Remove("current_track");
        if (json["race_type"] != null) DataVault.Set("race_type", json["race_type"].Value);
        else DataVault.Remove("race_type");
        if (json["type"] != null) DataVault.Set("type", json["type"].Value);
        else DataVault.Remove("type");
        if (json["finish"] != null) DataVault.Set("finish", json["finish"].AsInt);
        else DataVault.Remove("finish");
        if (json["lower_finish"] != null) DataVault.Set("lower_finish", json["lower_finish"].AsInt);
        else DataVault.Remove("lower_finish");
        if (json["challenger"] != null) DataVault.Set("challenger", json["challenger"].Value);
        else DataVault.Remove("challenger");
        if (json["current_game_id"] != null)
        {
            UnityEngine.Debug.Log("Bluetooth: Current Game ID received: " + json["current_game_id"]);
            DataVault.Set("current_game_id", json["current_game_id"].Value);
        }
        else DataVault.Remove("current_game_id");
        JSONNode challengeNotification = json["current_challenge_notification"];
        if (challengeNotification != null) {
            // TODO: fetch challenge notification and store in datavault
        } 
        if (challengeNotification == null) DataVault.Remove("current_challenge_notification");
        Platform.Instance.ResetTargets();
    }

}

