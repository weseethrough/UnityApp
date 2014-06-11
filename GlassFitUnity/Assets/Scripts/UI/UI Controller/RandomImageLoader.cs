using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RandomImageLoader : MonoBehaviour {

	Object[] textures;

	// Use this for initialization
	void Start () {
        textures = Resources.LoadAll("app_header_images");

		if(textures[8] != null) 
		{
            // commented to stop image cyling, uncomment to re-enable
            //Texture2D chosenTexture = (Texture2D)textures[Random.Range(0, textures.Length)];
            Texture2D chosenTexture = (Texture2D)textures[8];
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
