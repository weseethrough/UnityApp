using UnityEngine;
using System.Collections;

public class ScaleClippingPanel : MonoBehaviour {

	float defaultHeight = 1136f;
	float defaultYClipping = 0;
	float currentHeight;
	UIPanel panel;

	// Use this for initialization
	void Start () {
		panel = GetComponent<UIPanel>();
		RYOrthoCamera camera = (RYOrthoCamera)Component.FindObjectOfType(typeof(RYOrthoCamera));
		if(panel != null && camera != null) {
			Vector4 clipping = panel.clipRange;
			clipping.x = 0;
			defaultYClipping = clipping.w;
			float scaledHeight = Screen.height * ((float)camera.mCam.orthographicSize / 640);
			UnityEngine.Debug.Log("ScaleClippingPanel: current size is " + camera.mCam.orthographicSize);
			float heightDifference = defaultHeight - scaledHeight;
			float newYClipping = defaultYClipping - heightDifference;
			clipping.w = newYClipping;
			
			panel.clipRange = clipping;
		}
	}

}
