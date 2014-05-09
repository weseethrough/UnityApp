using UnityEngine;


[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class RYOrthoCamera : MonoBehaviour
{
	Camera mCam;
	Transform mTrans;

	//only apply this scale change if we're at or around iPhone 4/5 resolution. Otherwise, ignore and we will fall back on scaling to fit height
	bool usePixelPerfect = true;

	void Start ()
	{
		mCam = camera;
		mTrans = transform;
		mCam.orthographic = true;
		usePixelPerfect = (Screen.width > 480) && (Screen.width < 720);
	}
	
	void Update ()
	{

		float y0 = mCam.rect.yMin * Screen.height;
		float y1 = mCam.rect.yMax * Screen.height;
		
		float size = (y1 - y0) * 0.5f * mTrans.lossyScale.y;

		usePixelPerfect = (Screen.width > 480) && (Screen.width < 720);
		if(!usePixelPerfect)
		{
			//scale relative to width of iphone 4/5
			float r = Screen.width / 640f;
			size /= r;
		}

		if (!Mathf.Approximately(mCam.orthographicSize, size)) mCam.orthographicSize = size;
	}
}