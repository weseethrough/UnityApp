using UnityEngine;
using System.Collections;

public class BoulderSnack : SnackBase {
	
	public override void Begin ()
	{
		base.Begin ();
		GetComponent<BoulderController>().enabled = true;
	}
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
