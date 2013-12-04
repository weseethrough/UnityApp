using UnityEngine;
using System.Collections;

public class LoadIcon : MonoBehaviour {
	
	private UIAtlas atlas;
	private UISprite sprite;
	
	// Use this for initialization
	void Start () {
		GameObject flow = GameObject.Find("Flow");
		UnityEngine.Debug.Log("Icon: flow found");
		atlas = flow.GetComponent<GraphComponent>().m_defaultHexagonalAtlas;
		UnityEngine.Debug.Log("Icon: atlas found");
		sprite = GetComponentInChildren<UISprite>();
		UnityEngine.Debug.Log("Icon: sprite found");
		if(atlas != null)
		{
			if(sprite != null) {
			UnityEngine.Debug.Log("Icon: everything found");
			sprite.atlas = atlas;
			UnityEngine.Debug.Log("Icon: image is called " + (string)DataVault.Get ("image_name"));
			sprite.spriteName = (string)DataVault.Get ("image_name");
			UnityEngine.Debug.Log("Icon: sprite set");
			}
			else 
			{
				UnityEngine.Debug.Log("Icon: problem with sprite");
			}
		}
		else
		{
			UnityEngine.Debug.Log("Icon: problem with atlas");
		}
	}
	
	// Update is called once per frame
	void Update () {
//		if(atlas != null && sprite != null)
//		{
//			UISpriteData sp = atlas.GetSprite((string)DataVault.Get("image_name"));
//			if(sp != null)
//			{
//				UISprite s;
//			}
//		}
	}
}
