using System;
using UnityEngine;
using Sqo;
using SiaqodbUtils;
using RaceYourself.Models;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Reflection;

public class PositionMessageListener : MonoBehaviour
{

    private Log log = new Log("PositionMessageListener");
    private Siaqodb db = DatabaseFactory.GetInstance();

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
        String desc = "Track before save:";
        foreach(FieldInfo f in t.GetType().GetFields()) {
            desc += ("\n" + f.Name + ": " + f.GetValue(t));
        }
        log.info (desc);

        // save the position
        t.save (db);

        // for testing, pull the track out and print its details
        t = db.Cast<Track>().Where<Track>(to => to.id == t.id).FirstOrDefault();
        desc = "Track after save:";
        foreach(FieldInfo f in t.GetType().GetFields()) {
            desc += ("\n" + f.Name + ": " + f.GetValue(t));
        }
        log.info (desc);
    }

    /// <summary>
    /// Triggered by messages from Java, position data passed in the JSON.
    /// Simply stores the position to the local database.
    /// </summary>
    /// <param name="json">Json.</param>
    public void NewPosition(String json) {
        log.info("Received position from java, saving to SiaqoDb..");
        Position p = JsonConvert.DeserializeObject<Position>(json);
        String desc = "Position before save:";
        foreach(FieldInfo f in p.GetType().GetFields()) {
            desc += ("\n" + f.Name + ": " + f.GetValue(p));
        }
        log.info (desc);

        // save the position
        p.save (db);

        // for testing, pull the track out and print its details
        p = db.Cast<Position> ().Where<Position> (po => po.id == p.id).FirstOrDefault ();
        desc = "Position after save:";
        foreach(FieldInfo f in p.GetType().GetFields()) {
            desc += ("\n" + f.Name + ": " + f.GetValue(p));
        }
        log.info (desc);
    }

    // Called by unity messages on each state change
    void PlayerStateChange(string message) {
        //UnityEngine.Debug.Log("Player state message received from Java: " + message);
        Platform.Instance.LocalPlayerPosition.SetPlayerState(message);
    }


}

