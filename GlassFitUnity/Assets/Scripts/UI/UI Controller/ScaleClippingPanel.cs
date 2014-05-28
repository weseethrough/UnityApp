using UnityEngine;
using System.Collections;

public class ScaleClippingPanel : MonoBehaviour {

	float defaultHeight = 1136f;
	float defaultYClipping = 0;
	float currentHeight;
	UIPanel panel;
	UISprite whiteBkg;

	// Use this for initialization
	void Start () {
		panel = GetComponent<UIPanel>();
		RYOrthoCamera camera = (RYOrthoCamera)Component.FindObjectOfType(typeof(RYOrthoCamera));
		Panel fs = FlowStateMachine.GetCurrentFlowState() as Panel;
		GameObject bkg = GameObjectUtils.SearchTreeByName(fs.physicalWidgetRoot, "WhiteBkg");
		if(bkg != null) {
			whiteBkg = bkg.GetComponent<UISprite>();
		}
		if(panel != null && camera != null) {
			Vector4 clipping = panel.clipRange;
			defaultYClipping = clipping.w;

			float iphoneAspect = 1136f / 640;
			float phoneAspect = (float)Screen.height / Screen.width;
			float aspectDifference = iphoneAspect / phoneAspect;

			float headerHeight = (402f + (70)) * aspectDifference;

			float scrollableHeight = defaultHeight - headerHeight;

			float newYClipping = scrollableHeight / aspectDifference;

			clipping.w = newYClipping;

			if(whiteBkg != null) {
				whiteBkg.height = (int)newYClipping;
			}

			panel.clipRange = clipping;
		}
	}

}
