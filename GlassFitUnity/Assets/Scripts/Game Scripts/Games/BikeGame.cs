using UnityEngine;
using System.Collections;

public class BikeGame : GameBase {

	private MinimalSensorCamera camera;

	private CyclistController cyclist;

	// Use this for initialization
	public override void Start () {
		base.Start();
		GameObject cameraObj = GameObject.Find("CameraHolder");

		if(cameraObj != null) {
			camera = cameraObj.GetComponent<MinimalSensorCamera>();
		} else {
			UnityEngine.Debug.LogError("BikeGame: Camera game object is null!");
		}

		if(camera != null) {
			camera.SetCycling(true);
		} else {
			UnityEngine.Debug.LogError("BikeGame: MinimalSensorCamera is null!");
		}

		GameObject cyclistObj = GameObject.Find("CyclistOpponent");

		if(cyclistObj != null) {
			cyclist = cyclistObj.GetComponent<CyclistController>();
		} else {
			UnityEngine.Debug.LogError("BikeGame: Cyclist game object is null");
		}

		if(cyclist != null) {
			cyclist.enabled = false;
		}

		Platform.Instance.ResetTargets();
	}

	protected override void OnExitState(string state)
	{
		switch(state)
		{
		case GAMESTATE_AWAITING_USER_READY:

			cyclist.enabled = true;

			cyclist.SetHeadstart(20.0f);

			SetVirtualTrackVisible(true);
			break;
		}
		base.OnExitState(state);
	}

	protected override double GetDistBehindForHud ()
	{
		if(cyclist != null) {
			return cyclist.GetDistanceBehindTarget();
		}
		else
		{
			return 0;
		}
	}

	// Update is called once per frame
	public override void Update () {
		base.Update();
	}
}
