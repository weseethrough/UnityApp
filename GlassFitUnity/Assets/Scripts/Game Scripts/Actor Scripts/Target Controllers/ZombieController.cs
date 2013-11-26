using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System;

public class ZombieController : TargetController {

	void Start () {
		base.Start();
		SetAttribs(50, 135, -150f, 13.88668f);
	}
	
	void OnEnable() {
		base.OnEnable();
		SetAttribs(50, 135, -150f, 13.88668f);
	}
	
	void Update () {
		base.Update();
	}
}
