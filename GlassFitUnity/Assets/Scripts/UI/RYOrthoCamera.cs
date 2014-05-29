using UnityEngine;


[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class RYOrthoCamera : MonoBehaviour
{
	public Camera mCam;
	Transform mTrans;

	int minPixelPerfectResolution = 480;
	int maxPixelPerfectResolution = 720;

	//only apply this scale change if we're at or around iPhone 4/5 resolution. Otherwise, ignore and we will fall back on scaling to fit height
	bool usePixelPerfect = true;

	void Start ()
	{
		mCam = camera;
		mTrans = transform;
		mCam.orthographic = true;
		usePixelPerfect = (Screen.width > minPixelPerfectResolution) && (Screen.width < maxPixelPerfectResolution);
	}
	
	void Update ()
	{

		float y0 = mCam.rect.yMin * Screen.height;
		float y1 = mCam.rect.yMax * Screen.height;
		
		float size = (y1 - y0) * 0.5f * mTrans.lossyScale.y;

		if(Screen.orientation == ScreenOrientation.Portrait || Screen.orientation == ScreenOrientation.PortraitUpsideDown)
		{
			usePixelPerfect = (Screen.width > minPixelPerfectResolution) && (Screen.width < maxPixelPerfectResolution);
			if(!usePixelPerfect)
			{
				//scale relative to width of iphone 4/5
				float r = Screen.width / 640f;
				size /= r;
			}
		}
		else
		{	
			//Landscape
			usePixelPerfect = (Screen.height > minPixelPerfectResolution) && (Screen.height < maxPixelPerfectResolution);
			if(!usePixelPerfect)
			{
				float r = Screen.height / 640f;
				size /= r;
			}
		}

		if (!Mathf.Approximately(mCam.orthographicSize, size)) mCam.orthographicSize = size;
	}
}