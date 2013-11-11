using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System;

public class ZombieController : TargetController {

	void Start () {
		base.Start();
		setAttribs(20, 135, -239.5f, -10);
	}
	
	void OnEnable() {
		base.OnEnable();
		setAttribs(20, 135, -239.5f, -10);
	}
	
	void Update () {
		base.Update();
	}
}
