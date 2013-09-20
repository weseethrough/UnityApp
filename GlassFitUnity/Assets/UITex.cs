using UnityEngine;
using System.Collections;

public class UITex : MonoBehaviour {

	Texture2D testPic;
	
	// Use this for initialization
	void Start () {
	
		testPic = Resources.Load("UI Interface") as Texture2D;
		
		renderer.material.mainTexture = testPic;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
