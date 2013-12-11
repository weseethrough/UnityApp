using UnityEngine;
using System.Collections;

public class camTex : MonoBehaviour {

	// Use this for initialization
	void Start () {
		string DevName = WebCamTexture.devices[0].name;			
		WebCamTexture	webCamTexture = new WebCamTexture(DevName, 320, 200, 30);
		webCamTexture.Play();
		renderer.material.mainTexture = webCamTexture;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
