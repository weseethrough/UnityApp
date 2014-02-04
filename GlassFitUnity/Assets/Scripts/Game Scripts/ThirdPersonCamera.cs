using UnityEngine;
using System.Collections;
using System;

public class ThirdPersonCamera : MonoBehaviour {
	
	private float height = 0.0f;
	private float zoom = 0.0f;
	
	private GestureHelper.OnSwipeLeft leftHandler = null;
	private GestureHelper.OnSwipeRight rightHandler = null;
	
	private bool third = false;
	
	public GameObject runner;
	
	void Start() {
		
		if(!Convert.ToBoolean(DataVault.Get("activity_fov"))){
			leftHandler = new GestureHelper.OnSwipeLeft(() => {
				GoThird();
			});
			
			GestureHelper.onSwipeLeft += leftHandler;
			
			rightHandler = new GestureHelper.OnSwipeRight(() => {
				GoFirst();
			});
			
			GestureHelper.onSwipeRight += rightHandler;
		}
		if(runner != null)
		{
			runner.SetActive(false);
		}
	}
	
	// Update is called once per frame
	void Update () {
		
		transform.localPosition = new Vector3(0, height, zoom);
	}
	
	void GoThird() {
		if(!third) {
			height = 0.3f;
			zoom = -5f;
			third = true;
			if(runner != null)
			{
				runner.SetActive(true);
			}
		}
	}
	
	void GoFirst() {
		if(third)
		{
			height = 0.0f;
			zoom = 0.0f;
			third = false;
			
			if(runner != null)
			{
				runner.SetActive(false);
			}
		}
	}
	
	void OnDestroy() {
		GestureHelper.onSwipeRight -= rightHandler;
		GestureHelper.onSwipeLeft -= leftHandler;
	}
}
