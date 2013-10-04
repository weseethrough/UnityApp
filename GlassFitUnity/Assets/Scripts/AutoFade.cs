using UnityEngine;
using System.Collections;

public class AutoFade : MonoBehaviour {
	
	private static AutoFade mInstance = null;
	private Material mMaterial = null;
	private string mLevelName = "";
	private int mLevelIndex = 0;
	private bool mFading = false;
	
	private static AutoFade Instance
	{
		get	
		{
			if(mInstance == null)
			{
				mInstance = (new GameObject("AutoFade")).AddComponent<AutoFade>();	
			}
			return mInstance;
		}
	}
	
	public static bool Fading
	{
		get { return Instance.mFading; }
	}
	
	// Set the instance and material for fading
	private void Awake()
	{
		DontDestroyOnLoad(this);
		mInstance = this;
		mMaterial = new Material("Shader \"Plane/No zTest\" { SubShader { Pass { Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Off Fog { Mode Off } BindChannels { Bind \"Color\",color } } } }");
	}
	
	//Draw the plane in front of the camera
	private void DrawQuad(Color aColor,float aAlpha)
    {
        aColor.a = aAlpha;
        mMaterial.SetPass(0);
        GL.Color(aColor);
        GL.PushMatrix();
        GL.LoadOrtho();
        GL.Begin(GL.QUADS);
        GL.Vertex3(0, 0, -1);
        GL.Vertex3(0, 1, -1);
        GL.Vertex3(1, 1, -1);
        GL.Vertex3(1, 0, -1);
        GL.End();
        GL.PopMatrix();
    }
	
	private IEnumerator Fade(float aFadeOutTime, float aFadeInTime, Color aColor)
    {
        float t = 0.0f;
        while (t<1.0f)
        {
            yield return new WaitForEndOfFrame();
            t = Mathf.Clamp01(t + Time.deltaTime / aFadeOutTime);
            DrawQuad(aColor,t);
        }
        if (mLevelName != "")
            Application.LoadLevel(mLevelName);
        else
            Application.LoadLevel(mLevelIndex);
        while (t>0.0f)
        {
            yield return new WaitForEndOfFrame();
            t = Mathf.Clamp01(t - Time.deltaTime / aFadeInTime);
            DrawQuad(aColor,t);
        }
        mFading = false;
    }
	
	private void StartFade(float aFadeOutTime, float aFadeInTime, Color aColor)
    {
        mFading = true;
        StartCoroutine(Fade(aFadeOutTime, aFadeInTime, aColor));
    }
	
	public static void LoadLevel(string aLevelName,float aFadeOutTime, float aFadeInTime, Color aColor)
    {
        if (Fading) return;
        Instance.mLevelName = aLevelName;
        Instance.StartFade(aFadeOutTime, aFadeInTime, aColor);
    }
	
	public static void LoadLevel(int aLevelIndex,float aFadeOutTime, float aFadeInTime, Color aColor)
    {
        if (Fading) return;
        Instance.mLevelName = "";
        Instance.mLevelIndex = aLevelIndex;
        Instance.StartFade(aFadeOutTime, aFadeInTime, aColor);
    }
}
