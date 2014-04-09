using System;
using UnityEngine;
using Sqo;
using SiaqodbUtils;
using RaceYourself.Models;
using Newtonsoft.Json;

public class PositionMessageListener : MonoBehaviour
{

    private Log log = new Log("PositionMessageListener");
    private Siaqodb db = DatabaseFactory.GetWritableInstance();

    void Awake()
    {
        log.info("Awake()");
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Triggered by messages from Java, track data passed in the JSON.
    /// Simply stores the track to the local database.
    /// </summary>
    /// <param name="json">Json.</param>
    public void NewTrack(String json) {
        log.info("Received track from java, saving to SiaqoDb..");
        Track t = JsonConvert.DeserializeObject<Track>(json);
        if (db.Cast<Track>().Where<Track>(tr => tr.id == t.id).FirstOrDefault() != null) {
            db.UpdateObjectBy("id", t);
        } else {
            db.StoreObject(t);
        }
    }

    /// <summary>
    /// Triggered by messages from Java, position data passed in the JSON.
    /// Simply stores the position to the local database.
    /// </summary>
    /// <param name="json">Json.</param>
    public void NewPosition(String json) {
        log.info("Received position from java, saving to SiaqoDb..");
        Position p = JsonConvert.DeserializeObject<Position>(json);
        if (db.Cast<Position>().Where<Position>(po => po.id == p.id).FirstOrDefault() != null) {
            db.UpdateObjectBy("id", p);
        } else {
            db.StoreObject(p);
        }
    }

    // Called by unity messages on each state change
    void PlayerStateChange(string message) {
        //UnityEngine.Debug.Log("Player state message received from Java: " + message);
        Platform.Instance.LocalPlayerPosition.SetPlayerState(message);
    }


}

