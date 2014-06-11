using System;
using System.Diagnostics;
using UnityEngine;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using RaceYourself.Models;

public class EditorPlayerPosition : PlayerPosition {

    /// <summary>
    /// Latitude of initial fake position. Morden tube, south London.
    /// </summary>
    private static double INITIAL_LATITUDE = 51.400;
    /// <summary>
    /// Longitude of initial fake position. A bit east of Morden tube station in south London.
    /// </summary>
    private static double INITIAL_LONGITUDE = -0.15;

	private Position _position = null;
	public override Position Position { get { return _position; } }


	private Position _predictedPosition = null;
	public override Position PredictedPosition { get { return _predictedPosition; } }

	private Stopwatch timer = new Stopwatch();
	public override long Time { get { return timer.ElapsedMilliseconds; } }

	private double _distance = 0.0;
	public override double Distance { get { return _distance; } }

	private float _pace = 1.0f;
	public override float Pace { get { return _pace; } }

	private const float speedIncrement = 0.5f;

	protected float _bearing = -999.0f;
	public override float Bearing { get { return _bearing; } }


	private System.Random random = new System.Random();
	private new Log log = new Log("EditorPlayerPosition");

    private List<Position> positions = new List<Position>();
    private int frames;

	public EditorPlayerPosition() {


	}

	public override void Update() {

		//fake speed up/ slow down
		if(Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.KeypadPlus))
		{
			_pace += speedIncrement;
		}
		if(Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
		{
			_pace -= speedIncrement;
		}

		if (!timer.IsRunning) return;

		_distance += Pace * UnityEngine.Time.deltaTime;
		if (random.Next() % 5 == 4) {
			_bearing += 10;
        }

        _position = new Position((float)(INITIAL_LATITUDE+Math.Cos(Bearing*Math.PI/180)*Distance/111229d), (float)(INITIAL_LONGITUDE + Math.Sin (Bearing * Math.PI / 180) * Distance / 111229d));
		_predictedPosition = _position;

        long ts = ConvertToTimestamp(DateTime.Now);

        if (frames++ % 30 == 0)
        {
            Position pTemp = new Position(_position.latitude, _position.longitude);
            pTemp.device_ts = ts;
            positions.Add(pTemp);
            //Platform.Instance.GetMonoBehavioursPartner ().SendMessage ("NewTrack", "json");
        }
	}

    private long ConvertToTimestamp(DateTime value)
    {
        TimeSpan span = (value - new DateTime(1970, 1, 1, 0, 0, 0, 0));
        return (long) span.TotalSeconds;
    }

	public override void StartTrack() {
		timer.Start();
		base.StartTrack();
	}

	public override Boolean HasLock() {
		//always report that we have gps lock in editor
		return true;
	}
	
	public override Track StopTrack() {
		timer.Stop();
		base.StopTrack();

        List<Position> pos = new List<Position>();
        Track dummy = new Track();
        dummy.positions = positions;
		return dummy;
	}

	public override void Reset() {
		timer.Stop();
		timer.Reset();
		base.Reset();
		_distance = 0;
	}

	public override void NotifyAutoBearing() {}
}