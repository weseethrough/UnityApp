using UnityEngine;
using System.Collections;

public class ShrinkingReticle : MonoBehaviour {

	private float cutoffValue = 0.0f;
	private bool scaling = true;
	private Vector3 endScale;
	private Vector3 startScale;
	private Vector3 currentScale;
	
	void Start()
	{
		startScale = new Vector3(1200f, 1200f, 1200f);
		endScale = new Vector3(242.6905f, 242.6905f, 242.6905f);
		currentScale = startScale;
		renderer.enabled = false;
	}
	
	void Update () { 
		if(scaling) {
			cutoffValue += Time.deltaTime;
			float realCutoff = (cutoffValue / 0.3f) * 0.4f;
			currentScale = Vector3.Lerp(startScale, endScale, realCutoff);
			UnityEngine.Debug.Log("Shrinking: current size is - " + currentScale.ToString());
		}
		
		transform.localScale = currentScale;
		
		if(cutoffValue > 1.0f)
		{
			StopTurning();
			UnityEngine.Debug.Log("Shrinking: stopped shrinking");
		}
	}
	
	public void StartTurning()
	{
		scaling = true;
//		cutoffValue += Time.deltaTime;
//		float realCutoff = (cutoffValue / 0.3f) * 0.4f;
//		currentScale = Vector3.Lerp(startScale, endScale, cutoffValue);
		transform.localScale = currentScale;
		renderer.enabled = true;
	}
	
	public void StopTurning()
	{
		scaling = false;
		cutoffValue = 0.0f;	
		renderer.enabled = false;
		currentScale = startScale;
	}
}