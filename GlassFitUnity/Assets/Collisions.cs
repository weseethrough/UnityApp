using UnityEngine;
using System.Collections;

public class Collisions : MonoBehaviour {
	
	public GameObject cam;
	
	// Use this for initialization
	void Start () {
		
	}
	
	void OnCollisionEnter(Collision collision)
	{
		if(collision.gameObject.tag == "Test")
		{
			
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
