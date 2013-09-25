using UnityEngine;
using System.Collections;

public class Collisions : MonoBehaviour {
	
	//public GameObject cam;
	private bool check = false;
	private GameObject cam;
	private UICamera camm;
	// Use this for initialization
	void Start () {
		cam = GameObject.Find("Main Camera");
		camm = cam.GetComponent<UICamera>();
	}
	
	void OnGUI() 
	{
		if(check)
		{
			GUI.Label(new Rect(Screen.width, Screen.height-100, 200, 100), "Checking");	
		}
		
		GUI.Label(new Rect(Screen.width, Screen.height-100, 200, 100), camm.zoomedIn.ToString());
	}
	
	void OnCollisionEnter(Collision collision)
	{
		if(camm.zoomedIn)
		{
			check = true;
			if(collision.gameObject.name == "Test")
			{
				camm.stuck = true;
			}
			else
			{
				camm.stuck = false;
			}
		}
		else
			check = false;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
