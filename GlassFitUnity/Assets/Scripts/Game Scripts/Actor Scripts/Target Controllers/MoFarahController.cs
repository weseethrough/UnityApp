using UnityEngine;
using System.Collections;

public class MoFarahController : TargetController {

	// Use this for initialization
	void Start () {
		base.Start();
		base.SetAttribs(0, 135, -254.6f, 50);
	}
	
	void OnEnable() {
		base.OnEnable();
		base.SetAttribs(0, 135, -254.6f, 50);
	}
	
	// Update is called once per frame
	void Update () {
		base.Update();
	}
}
