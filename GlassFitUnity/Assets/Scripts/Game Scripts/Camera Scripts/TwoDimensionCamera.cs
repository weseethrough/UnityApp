using UnityEngine;
using System.Collections;

public class TwoDimensionCamera : MonoBehaviour {

	private GestureHelper.TwoFingerTap twoTapHandler = null;
	
	// Use this for initialization
	void Start () {
		twoTapHandler = new GestureHelper.TwoFingerTap(() => {
			Platform.Instance.GetPlayerOrientation().Reset();
		});
		GestureHelper.onTwoTap += twoTapHandler;
		
		
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
