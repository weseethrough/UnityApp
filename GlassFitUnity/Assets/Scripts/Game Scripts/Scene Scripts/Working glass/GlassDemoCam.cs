using UnityEngine;
using System.Collections;

/// <summary>
/// Glass demo cam. Rotates the camera for the Glass demo
/// </summary>
public class GlassDemoCam : MonoBehaviour {

	public Quaternion offsetFromStart;
	
	private bool started = false;
	private float scaleX;
	private float scaleY;
	public GameObject grid;
	private bool gridOn = false;
	private float gridTimer = 0.0f;
	private bool timerActive = false;
	public GameObject sc;

	// Set the grid, scale values and the initial offset
	void Start () {
		scaleX = (float)Screen.width / 800.0f;
		scaleY = (float)Screen.height / 500.0f;
	}
	
	void OnGUI()
	{		
		// Set the new GUI matrix based on scale and the depth
		GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scaleX,scaleY, 1));		
		GUI.depth = 7;
		
		if(!started)
		{
			GUI.Label(new Rect(325, 100, 200, 50), "Align the grid with the floor");
			GUI.Label(new Rect(375, 150, 100, 50), "Tap to Set");
		}
		
		// Check if the button is being held
		if((GUI.RepeatButton(new Rect(200, 0, 400, 250), "", GUIStyle.none) || sc.GetComponent<GestureHelper>().isTapped))
		{ 
			// Activates the grid and reset the gyros if the timer is off, turns it off if the timer is on
			if(started) {
				if(timerActive) {
					gridOn = false;
				} else {
					offsetFromStart = Platform.Instance.GetOrientation();
					Platform.Instance.ResetGyro();
					gridOn = true;
				}
				gridTimer = 5.0f;
			} else {
				started = true;
			}
		}
		else if(Event.current.type == EventType.Repaint)
		{
			// If the grid is on when the button is released, activate timer, else reset the timer and switch it off
			if(gridOn)
			{
				timerActive = true;
			} else
			{
				gridTimer = 0.0f;
				timerActive = false;
			}
		}	
		GUI.matrix = Matrix4x4.identity;
	}
	
	void Update () {
		// Set the new rotation of the camera
		Quaternion newOffset = Quaternion.Inverse(offsetFromStart) * Platform.Instance.GetOrientation();
		
		// If the timer and grid are on, countdown the timer and switch it off if the timer runs out
		if(timerActive && gridOn)
		{
			gridTimer -= Time.deltaTime;
			if(gridTimer < 0.0f)
			{
				gridOn = false;
				timerActive = false;
			}
		}
		
		if(started) {
			grid.SetActive(gridOn);
		}
		
		transform.rotation =  newOffset;		
	}
}
