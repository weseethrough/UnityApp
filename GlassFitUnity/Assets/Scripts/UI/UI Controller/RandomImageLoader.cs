using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RandomImageLoader : MonoBehaviour {

	Object[] textures;

	// Use this for initialization
	void Start () {
		textures = Resources.LoadAll("app_header_images");

		if(textures != null) 
		{
			Texture2D chosenTexture = (Texture2D)textures[Random.Range(0, textures.Length)];
			if(chosenTexture != null) 
			{
				UITexture textureComponent = GetComponent<UITexture>();
				if(textureComponent != null) {
					textureComponent.mainTexture = chosenTexture;
				}
			}
		}
	}

}
