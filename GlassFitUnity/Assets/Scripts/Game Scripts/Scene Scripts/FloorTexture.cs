using UnityEngine;
using System.Collections;

public class FloorTexture : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		Vector2 uvOffset = renderer.material.mainTextureOffset;
		
		uvOffset.y -= ((Platform.Instance.Pace() * 0.25f) * Time.deltaTime );
		
		renderer.material.mainTextureOffset = uvOffset;
	}
}
