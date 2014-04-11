using UnityEngine;
using System.Collections;

public class SprinterTenKController : TargetController {

	// Use this for initialization
	public override void Start () {
		base.Start();
		base.SetAttribs(0, 135, -254.6f, 50);
	}
	
	public override void OnEnable() {
		base.OnEnable();
		base.SetAttribs(0, 135, -254.6f, 50);
	}
	
	// Update is called once per frame
	public override void Update () {
		base.Update();
	}
}
