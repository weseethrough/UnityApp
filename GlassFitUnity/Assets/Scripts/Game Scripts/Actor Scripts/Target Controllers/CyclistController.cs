using UnityEngine;
using System.Collections;

public class CyclistController : TargetController {

	// Use this for initialization
	void Start () {
		base.Start();
		SetAttribs(0, 135f, -220f, 100f);
	}
	
	void OnEnable() {
		base.OnEnable();
		SetAttribs(0, 135f, -220f, 100f);
	}
	
	// Update is called once per frame
	void Update () {
		base.Update();
		
		
	}
}
