using UnityEngine;
using System.Collections;

public class BoulderController : TargetController {
	
	private float yRot;
	
	// Use this for initialization
	void Start () {
		SetAttribs(50f, 135f, 420f, 0f);
		yRot = 0;
	}
	
	void OnEnable() 
	{
		base.OnEnable();
		SetAttribs(50f, 135f, 420f, 0f);		
	}
	
	// Update is called once per frame
	void Update () {
		base.Update();
		
		yRot += (10 * (((float)DataVault.Get("slider_val")* 9.15f) + 1.25f)) * Time.deltaTime;
		
		if(yRot > 360)
		{
			yRot -= 360;
		}
		
		Quaternion rot = Quaternion.Euler(new Vector3(yRot,0,0));
		
		transform.rotation = rot;
	}
}
