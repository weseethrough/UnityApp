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
	private GameBase game;
	
	private bool isChanging = false;
	private bool moving = false;
	
	private Vector3 startMove;
	private Vector3 endMove;
	
	void Start() {
		
		if(!Convert.ToBoolean(DataVault.Get("activity_fov"))){
			leftHandler = new GestureHelper.OnSwipeLeft(() => {
				if(game != null) {
					if(game.IsReady() && !isChanging) 
					{
						GoThird();
					}
				}
				else
				{
					UnityEngine.Debug.Log("ThirdPersonCamera: game is null");
				}
			});
			
			GestureHelper.onSwipeLeft += leftHandler;
			
			rightHandler = new GestureHelper.OnSwipeRight(() => {
				if(game != null) {
					if(game.IsReady() && !isChanging) 
					{
						GoFirst();
					}
				}
				else
				{
					UnityEngine.Debug.Log("ThirdPersonCamera: game is null");
				}
				
			});
			
			game = GameObject.FindObjectOfType(typeof(GameBase)) as GameBase;
			
			GestureHelper.onSwipeRight += rightHandler;
		}
		if(runner != null)
		{
			runner.SetActive(false);
		}
	}
	
	void SetZoomFromPitch()
	{
		float pitch = Platform.Instance.GetPlayerOrientation().AsPitch();
		if(pitch > 0.0f)
		{
			UnityEngine.Debug.Log("ThirdPersonCamera: pitch is " + pitch.ToString("f2"));
			height = Mathf.Clamp(0.3f - (3.0f * pitch), -3.0f, 0.3f);
			zoom = Mathf.Clamp(-5.0f + (-5.0f * (pitch * 2)), -10.0f, -5.0f);
		}
		else
		{
			height = 0.3f;
			zoom = -5.0f;
		}
	}
	
	IEnumerator MoveTo(Vector3 start, Vector3 end, float time)
	{
		isChanging = true;
		float t = 0.0f;
		while (t < 1.0f)
		{
			t += Time.deltaTime / time;
			transform.localPosition = Vector3.Lerp(start, end, t);
			yield return 0;
		}
		isChanging = false;
		if(!third)
		{
			if(runner != null)
			{
				runner.SetActive(false);
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		
		if(third) {
			SetZoomFromPitch();
		}
		
		if(!isChanging) {
			transform.localPosition = new Vector3(0, height, zoom);
		}
	}
	
	void GoThird() {
		if(!third) {
			startMove = new Vector3(0,0,0);
			SetZoomFromPitch();
			endMove = new Vector3(0, height, zoom);
			StartCoroutine(MoveTo(startMove, endMove, 0.5f));
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
			startMove = new Vector3(0, height, zoom);
			endMove = new Vector3(0,0,0);
			StartCoroutine(MoveTo(startMove, endMove, 0.5f));
			
			height = 0.0f;
			zoom = 0.0f;
			third = false;
			
		}
	}
	
	void OnDestroy() {
		GestureHelper.onSwipeRight -= rightHandler;
		GestureHelper.onSwipeLeft -= leftHandler;
	}
}
