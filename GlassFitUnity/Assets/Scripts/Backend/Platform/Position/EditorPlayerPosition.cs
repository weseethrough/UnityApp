using System;
using System.Diagnostics;
using UnityEngine;
using System.Runtime.CompilerServices;

using RaceYourself.Models;

public class EditorPlayerPosition : PlayerPosition {

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

	private const float speedIncremet = 0.5f;

	protected float _bearing = -999.0f;
	public override float Bearing { get { return _bearing; } }


	private System.Random random = new System.Random();
	private new Log log = new Log("EditorPlayerPosition");

	public EditorPlayerPosition() {


	}

	public override void Update() {

		//fake speed up/ slow down
		if(Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.KeypadPlus))
		{
			_pace += speedIncremet;
		}
		if(Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
		{
			_pace -= speedIncremet;
		}

		if (!timer.IsRunning) return;

		_distance += Pace * UnityEngine.Time.deltaTime;
		if (random.Next() % 5 == 4) {
			_bearing += 10;
        }
		_position = new Position((float)(51.400+Math.Cos(Bearing*Math.PI/180)*Distance/111229d), (float)(-0.15+Math.Sin(Bearing*Math.PI/180)*Distance/111229d));
		_predictedPosition = _position;

        Platform.Instance.GetMonoBehavioursPartner ().SendMessage ("NewTrack", "json");
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
		return null;
	}

	public override void Reset() {
		timer.Stop();
		timer.Reset();
		base.Reset();
		_distance = 0;
	}

	public override void NotifyAutoBearing() {}
}