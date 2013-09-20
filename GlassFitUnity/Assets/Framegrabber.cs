using UnityEngine;
using System.Collections;
using System;

public class Framegrabber : MonoBehaviour {
	private int fps = 0;
	private const int scale = 1;
	
	IEnumerator Start() {
 		return RecordFramebuffer();
	}
	
	IEnumerator RecordFramebuffer() {
		Debug.Log("Recording..");
		do {
			yield return new WaitForEndOfFrame();
			DateTime start = DateTime.Now;
			int width = Screen.width/scale;
	        int height = Screen.height/scale;
	        Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
	        tex.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
	        tex.Apply();
	        Color[] bytes = tex.GetPixels();
	        Destroy(tex);	
			TimeSpan diff = DateTime.Now - start;			
			if (diff.Milliseconds > 0) fps = 1000/diff.Milliseconds;
		} while(true);
	}

	void OnGUI() {
		GUI.Label(new Rect(Screen.width/2-50, 100, 100, 50), ""+fps);
	}
	
}