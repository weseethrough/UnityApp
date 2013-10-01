using UnityEngine;
using System.Collections;

public class camTex : MonoBehaviour {

	//Texture2D testPic;
	private WebCamTexture webCamTexture;
	// Use this for initialization
	void Start () {
		string DevName = WebCamTexture.devices[0].name;
	
		webCamTexture = new WebCamTexture(DevName, 320, 200, 30);
		webCamTexture.Play();
		renderer.material.mainTexture = webCamTexture;

	
		//testPic = Resources.Load("UI Interface");
		
		//renderer.material.mainTexture = testPic;

	}
	
	void OnDisable() {
		webCamTexture.Stop();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
