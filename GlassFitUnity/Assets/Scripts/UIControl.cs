using UnityEngine;
using System.Collections;

public class UIControl : MonoBehaviour {
	
	public float speed = 2.0f;
	private float curDist = 0;
	private int level = 2;
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
			
				float newFOV = this.camera.fieldOfView;
				
				if(newFOV >= 20 && level == 1) {
					newFOV = Mathf.Clamp(this.camera.fieldOfView-((Mathf.Abs(curDist)*curDist) * 100)/((float)Screen.height*Screen.height), 20, 45);
				}
				else if(newFOV <= 30 && level == 2) {
					newFOV = Mathf.Clamp(this.camera.fieldOfView-((Mathf.Abs(curDist)*curDist) * 50)/((float)Screen.height*Screen.height), 10, 30);
				}
				
				this.camera.fieldOfView = newFOV;

			}
			
			if(Input.GetTouch(0).phase == TouchPhase.Ended)
			{
				if(this.camera.fieldOfView == 20 && level == 1)
				{
					level = 2;
				}
				else if(this.camera.fieldOfView > 20 && level == 2)
				{
					level = 1;
				}
			}
			
		}
		else if(this.camera.fieldOfView < 20)
		{
			this.camera.fieldOfView = Mathf.Lerp(this.camera.fieldOfView, 20, Time.deltaTime*4);
		}
		else if(this.camera.fieldOfView > 20 && this.camera.fieldOfView < 45)
		{
			this.camera.fieldOfView = Mathf.Lerp(this.camera.fieldOfView, 45, Time.deltaTime*4);
		}
		
		if(Input.touchCount == 2)
		{
			this.camera.fieldOfView = 10;
		}
		
		
		
	}
}
