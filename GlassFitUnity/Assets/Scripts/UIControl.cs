using UnityEngine;
using System.Collections;

public class UIControl : MonoBehaviour {
	
	public float speed = 2.0f;
	public float minPinchSpeed = 5.0f;
	public float varianceInDistances = 5.0f;
	private float curDist = 0;
	//private float prevDist = new Vector2(0, 0);
	private float touchDelta = 0.0f;
	private float speedTouch0 = 0.0f;
	private float speedTouch1 = 0.0f;
	private Vector2 start = new Vector2(0, 0);
	
	// Use this for initialization
	void Start () {
	
	}
	
	void Update () {
		if(Input.touchCount == 1) 
		{		
			
			if(Input.GetTouch(0).phase == TouchPhase.Began) {
				start = Input.touches[0].position;
			}
			
			if(Input.GetTouch(0).phase == TouchPhase.Moved) {
				// Current distance between fingers
				curDist = Input.GetTouch(0).position.y - start.y;
			
				// Previous distance between fingers, by taking away the change from current position
				//prevDist = ((Input.GetTouch(0).position - Input.GetTouch(0).deltaPosition) - (Input.GetTouch(1).position - Input.GetTouch(1).deltaPosition));
			
				// Find the size of the pinch
				//touchDelta = curDist.magnitude - prevDist.magnitude;
			
				// Get the speed of the change
				//speedTouch0 = Input.GetTouch(0).deltaPosition.magnitude / Input.GetTouch(0).deltaTime;
				//speedTouch1 = Input.GetTouch(1).deltaPosition.magnitude / Input.GetTouch(1).deltaTime;
			
				if(this.camera.fieldOfView > 20)
				{
					speed = 2.0f;
				}
				else if(this.camera.fieldOfView <=20)
				{
					speed = 0.3f;
				}
			
				// Check to see if pinch is bigger, and zoom in if so
//				if((touchDelta + varianceInDistances <= 1) && (speedTouch0 > minPinchSpeed) && (speedTouch1 > minPinchSpeed))
//				{
//					this.camera.fieldOfView = Mathf.Clamp(this.camera.fieldOfView + speed, 10, 60);
//				}
//			
//				// Check to see if pinch is smaller, and zoom out if so
//				if((touchDelta + varianceInDistances > 1) && (speedTouch0 > minPinchSpeed) && (speedTouch1 > minPinchSpeed)) 
//				{
//					this.camera.fieldOfView = Mathf.Clamp(this.camera.fieldOfView - speed, 10, 60);
//				}
				float newFOV = this.camera.fieldOfView;
				
				if(newFOV < 20) {
					speed = 0.5f;
				}
				else {
					speed = 3;
				}
				
				newFOV = Mathf.Clamp(this.camera.fieldOfView-curDist/Screen.height*speed, 10, 45);
				this.camera.fieldOfView = newFOV;

			}
			
		}
		else if(this.camera.fieldOfView < 20)
		{
			this.camera.fieldOfView = 20;
		}
		
		if(Input.touchCount == 2)
		{
			this.camera.fieldOfView = 10;
		}
		
		
		
	}
}
