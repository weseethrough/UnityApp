using UnityEngine;
using System.Collections;

public class ShootingProgress : MonoBehaviour {

	private float cutoffValue = 1.0f;
	private bool turning = true;
	
	void Update () { 
		if(turning && cutoffValue > 0.0f) {
			cutoffValue -= Time.deltaTime;
		}
		
		if(cutoffValue < 0.0f)
		{
			StopTurning();
		}
		renderer.material.SetFloat("_Cutoff", cutoffValue); 
	}
	
	public void StartTurning()
	{
		turning = true;
		cutoffValue -= Time.deltaTime;
		renderer.enabled = true;
	}
	
	public void StopTurning()
	{
		turning = false;
		cutoffValue = 1.0f;	
		renderer.enabled = false;
	}
}