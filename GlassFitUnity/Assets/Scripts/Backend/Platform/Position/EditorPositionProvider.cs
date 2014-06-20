using UnityEngine;
using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using PositionTracker;
using RaceYourself.Models;

public class EditorPositionProvider : MonoBehaviour, IPositionProvider
{

    private const float SPEED_INCREMENT = 0.5f;

    private List<IPositionListener> positionListeners = new List<IPositionListener> ();

    /// <summary>
    /// Latitude of initial fake position. Morden tube, south London.
    /// </summary>
    private double _latitude = 51.400;

    /// <summary>
    /// Longitude of initial fake position. A bit east of Morden tube station in south London.
    /// </summary>
    private double _longitude = -0.15;

    private float _speed = 1.0f;  // metres per second
    private double _distance = 0.0;  // metres
    private float _bearing = 0.0f;  // degrees

    private System.Random random = new System.Random();
    private new Log log = new Log("EditorPositionProvider");
    private int frames = 0;


    //// Update is called once per frame
    public void Update ()
    {
        DataVault.Set ("location_service_status_message", "Location service enabled");

        //fake speed up/ slow down
        if(Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            log.info ("Increasing speed");
            _speed += SPEED_INCREMENT;
        }
        if(Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            log.info ("Decreasing speed");
            _speed -= SPEED_INCREMENT;
        }

        // update distance and direction
        double distanceDelta = _speed * UnityEngine.Time.deltaTime;
        _distance += distanceDelta;
        if (random.Next() % 5 == 4) {
            _bearing += 10;
        }

        // update lat and long
        _latitude += Math.Cos (_bearing * Math.PI / 180) * distanceDelta / 111229d;
        _longitude += Math.Sin (_bearing * Math.PI / 180) * distanceDelta / 111229d;

        // calculate and broadcast a position every 30 frames
        if (frames++ % 100 == 0)
        {
            Position p = new Position((float)_latitude, (float)_longitude);
            p.bearing = _bearing;
            p.speed = _speed;
            p.epe = 0.0f;
            p.device_ts = ConvertToTimestamp(DateTime.Now);

            foreach (IPositionListener listener in positionListeners) {
                log.info ("Sending new position to listener, speed = " + _speed + " distance = " + _distance);
                listener.OnPositionUpdate (p);
            }
        }
    }

    // Helper to create timestamps from 1 Jan 1970 in milliseconds
    private long ConvertToTimestamp(DateTime value)
    {
        TimeSpan span = (value - new DateTime(1970, 1, 1, 0, 0, 0, 0));
        return (long) span.TotalSeconds;
    }

    public bool RegisterPositionListener (IPositionListener posListener)
    {
        positionListeners.Insert (0, posListener);
        return true;
    }

    public void UnregisterPositionListener (IPositionListener posListener)
    {
        positionListeners.Remove (posListener);
    }
}
