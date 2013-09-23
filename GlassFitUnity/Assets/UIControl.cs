using UnityEngine;
using System.Collections;

public class UIControl : MonoBehaviour {
	
	public float speed = 0.5f;
	public float minPinchSpeed = 5.0f;
	public float varianceInDistances = 5.0f;
	private Vector2 curDist = new Vector2(0, 0);
	private Vector2 prevDist = new Vector2(0, 0);
	private float touchDelta = 0.0f;
	private float speedTouch0 = 0.0f;
	private float speedTouch1 = 0.0f;
	
	// Use this for initialization
	void Start () {
	
	}
	
	void Update () {
		if(Input.touchCount == 2 && Input.GetTouch(0).phase == TouchPhase.Moved && Input.GetTouch(1).phase == TouchPhase.Moved) 
		{
			// Current distance between fingers
			curDist = Input.GetTouch(0).position - Input.GetTouch(1).position;
			
			// Previous distance between fingers, by taking away the change from current position
			prevDist = ((Input.GetTouch(0).position - Input.GetTouch(0).deltaPosition) - (Input.GetTouch(1).position - Input.GetTouch(1).deltaPosition));
			
			// Find the size of the pinch
			touchDelta = curDist.magnitude - prevDist.magnitude;
			
			// Get the speed of the change
			speedTouch0 = Input.GetTouch(0).deltaPosition.magnitude / Input.GetTouch(0).deltaTime;
			speedTouch1 = Input.GetTouch(1).deltaPosition.magnitude / Input.GetTouch(1).deltaTime;
			
			// Check to see if pinch is bigger, and zoom in if so
			if((touchDelta + varianceInDistances <= 1) && (speedTouch0 > minPinchSpeed) && (speedTouch1 > minPinchSpeed))
			{
				this.camera.fieldOfView = Mathf.Clamp(this.camera.fieldOfView + speed, 15, 45);
			}
			
			// Check to see if pinch is smaller, and zoom out if so
			if((touchDelta + varianceInDistances > 1) && (speedTouch0 > minPinchSpeed) && (speedTouch1 > minPinchSpeed)) 
			{
				this.camera.fieldOfView = Mathf.Clamp(this.camera.fieldOfView - speed, 15, 45);
			}
		}
	}
}
