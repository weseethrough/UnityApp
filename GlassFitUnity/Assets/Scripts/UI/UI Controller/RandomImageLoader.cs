using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RandomImageLoader : MonoBehaviour {

	Object[] textures;

	public enum ImageType 
	{
		Background,
		Card,
		Header
	};

	public ImageType imageType;

	// Use this for initialization
	void Start () {
        textures = Resources.LoadAll("app_header_images/25-06");
		if(textures != null) {
			Texture2D chosenTexture = null;
			switch(imageType)
			{
			case ImageType.Background:
				chosenTexture = (Texture2D)textures[Random.Range(0, 2)];
				break;
				
			case ImageType.Card:
				chosenTexture = (Texture2D)textures[Random.Range(2, 4)];
				break;
				
			case ImageType.Header:
				chosenTexture = (Texture2D)textures[Random.Range(4, 6)];
				break;
				
			default:
				UnityEngine.Debug.LogError("RandomImageLoader: can't find imageType");
				break;
			}

			if(chosenTexture != null) 
			{
				UITexture textureComponent = GetComponent<UITexture>();
				if(textureComponent != null) {
					textureComponent.mainTexture = chosenTexture;
				}
			}
			else
			{
				UnityEngine.Debug.LogError("RandomImageLoader: chosen texture is null");
			}

		} else 
		{
			UnityEngine.Debug.LogError("RandomImageLoader: textures is null");
		}
	}

}
