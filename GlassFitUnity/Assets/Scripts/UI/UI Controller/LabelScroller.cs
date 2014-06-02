using UnityEngine;
using System;

public class LabelScroller : MonoBehaviour 
{
	public float cullHeight = 450;
	public float margin = 25;
	
	private float labelHeight = 0;
	private float labelOffset = 0;
	
	private Vector2? draggingStartPos = null;
	
	private float velocity = 0;
	
	public void Start ()
	{
		Bounds bounds = NGUIMath.CalculateAbsoluteWidgetBounds(transform);
		labelOffset = transform.position.y;
		labelHeight = bounds.max.y - bounds.min.y;
	}

	public void Update ()
	{		
		velocity *= (1.0f-Math.Min(1.0f, Time.deltaTime*2));
        
        //mouse input
        if (Input.GetMouseButtonDown(0))
        {
            draggingStartPos = Input.mousePosition;
            velocity = 0;
        }
        else if (Input.GetMouseButton(0))
        {
            Vector2 newPos = Input.mousePosition;
            Vector2 offset = newPos - draggingStartPos.Value;
            draggingStartPos = newPos;
            
            velocity = offset.y*25;
        }
		//touch input. TODO test on iOS, Android mobile and Glass
		else if (Platform.Instance.GetTouchCount() >= 1) {
			Vector2? touch = Platform.Instance.GetTouchInput();
			if (touch.HasValue) {
				if (draggingStartPos.HasValue) {
					
					Vector2 offset = touch.Value - draggingStartPos.Value;
					UnityEngine.Debug.Log("offset-y: " + offset.y);
					if (Platform.Instance.OnGlass()) {
						// Forward/back
						velocity = -offset.x*2500;
					} else {
						// Up/down
						velocity = offset.y*Screen.height*10;
					}
						
				}
				draggingStartPos = touch.Value;
			}
		} else draggingStartPos = null;

		Vector3 position = transform.position;
		position.y += velocity*Time.deltaTime;
		if (position.y > labelOffset+labelHeight-cullHeight+margin) {
			position.y = labelOffset+labelHeight-cullHeight+margin;
			velocity = -margin;
		}
		if (position.y < labelOffset-margin) {
			position.y = labelOffset-margin;
			velocity = margin;
		}
		transform.position = position;
	}
	
}