using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class RemoteTextureManager : MonoBehaviour {

	private int request = 0;
	private Dictionary<string, Texture> textureDictionary = new Dictionary<string, Texture>();

	void Awake()
	{
		DontDestroyOnLoad(gameObject);
	}

	public void LoadImage(string url, string callbackArgument, Action<Texture, string> callback) {
		if(textureDictionary.ContainsKey(url)) {
			callback(textureDictionary[url], callbackArgument);
		}
		StartCoroutine(LoadImageCoroutine(url, callbackArgument, callback));
	}

	public IEnumerator LoadImageCoroutine(string url, string callbackArgument, Action<Texture, string> callback) {
		while(request >= 10) {
			yield return new WaitForSeconds(5f);
		}
		request++;

		WWW webURL = new WWW(url);

		yield return webURL;

		Texture2D webTexture = new Texture2D(140, 140, TextureFormat.ARGB32, false);

		webURL.LoadImageIntoTexture(webTexture);

		textureDictionary.Add(url, webTexture);

		callback(webTexture, callbackArgument);
		request--;
	}
}
