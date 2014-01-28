using UnityEngine;
using System.Collections;

/// <summary>
/// Fades the level to a specified colour and loads the new level before fading in again.
/// </summary>
public class AutoFade : MonoBehaviour {
	
	// Singleton instance of AutoFade
	private static AutoFade mInstance = null;
	
	// Material for the plane
	private Material mMaterial = null;
	
	// Name of the level
	private string mLevelName = "";
	
	// Integer for the level
	private int mLevelIndex = 0;
	
	// Checks if the level is fading
	private bool mFading = false;
	
	// Returns an instance of autofade
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
	
	// Checks if the scene is fading
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
	
	/// <summary>
	/// Fades in and out based on the time and colour.
	/// </summary>
	/// <param name='aFadeOutTime'>
	/// Time it takes to fade out.
	/// </param>
	/// <param name='aFadeInTime'>
	/// Time it takes to fade in.
	/// </param>
	/// <param name='aColor'>
	/// Colour to fade to.
	/// </param>
	private IEnumerator Fade(float aFadeOutTime, float aFadeInTime, Color aColor)
    {
		// Float for time
        float t = 0.0f;
		
		// While the time is within range, draw a quad with the colour specified
		// and an alpha value based on the current time
        while (t<1.0f)
        {
            yield return new WaitForEndOfFrame();
            t = Mathf.Clamp01(t + Time.deltaTime / aFadeOutTime);
            DrawQuad(aColor,t);
        }
		
		// The level is then loaded based on the name or number given
        if (mLevelName != "")
            Application.LoadLevel(mLevelName);
        else
            Application.LoadLevel(mLevelIndex);
       
		// Now the level is loaded we fade back in based on the time specified.
		while (t>0.0f)
        {
            yield return new WaitForEndOfFrame();
            t = Mathf.Clamp01(t - Time.deltaTime / aFadeInTime);
            DrawQuad(aColor,t);
        }
		
		// Set fading to false
        mFading = false;
    }
	
	/// <summary>
	/// Starts the fade to the next scene.
	/// </summary>
	/// <param name='aFadeOutTime'>
	/// The fade out time.
	/// </param>
	/// <param name='aFadeInTime'>
	/// The fade in time.
	/// </param>
	/// <param name='aColor'>
	/// The colour of the quad.
	/// </param>
	private void StartFade(float aFadeOutTime, float aFadeInTime, Color aColor)
    {
        mFading = true;
        StartCoroutine(Fade(aFadeOutTime, aFadeInTime, aColor));
    }
	
	/// <summary>
	/// Loads the level using a string for the name.
	/// </summary>
	/// <param name='aLevelName'>
	/// The level name.
	/// </param>
	/// <param name='aFadeOutTime'>
	/// The time in seconds to fade out.
	/// </param>
	/// <param name='aFadeInTime'>
	/// The time in seconds to fade in.
	/// </param>
	/// <param name='aColor'>
	/// The colour of the quad to use during the fade.
	/// </param>
	public static void LoadLevel(string aLevelName,float aFadeOutTime, float aFadeInTime, Color aColor)
    {
        if (Fading) return;

		if (!Platform.Instance.OnGlass()) {
            JSONObject json = new JSONObject();
			json.AddField("action", "LoadLevelFade");
			json.AddField("levelName", aLevelName);
			
			JSONObject data = new JSONObject();
			Track track = (Track)DataVault.Get("current_track");
			if (track != null) data.AddField("current_track", track.AsJson);
			else data.AddField("current_track", (JSONObject)null);
			data.AddField("race_type", DataVault.Get("race_type") as string);
			data.AddField("type", DataVault.Get("type") as string);
			if (DataVault.Get("finish") != null) data.AddField("finish", (int)DataVault.Get("finish"));
			if (DataVault.Get("lower_finish") != null) data.AddField("lower_finish", (int)DataVault.Get("lower_finish"));
			if (DataVault.Get("challenger") != null) data.AddField("challenger", DataVault.Get("challenger") as string);
			if (DataVault.Get("current_challenge_notification") != null) data.AddField("current_challenge_notification", (DataVault.Get("current_challenge_notification") as ChallengeNotification).AsJson);
			
			json.AddField("data", data);
			Platform.Instance.BluetoothBroadcast(json);
			// TODO: Show "in game" message and return
		}		
		
		
        Instance.mLevelName = aLevelName;
        Instance.StartFade(aFadeOutTime, aFadeInTime, aColor);
    }
	
	/// <summary>
	/// Loads the level using an integer for the level.
	/// </summary>
	/// <param name='aLevelIndex'>
	/// The level number.
	/// </param>
	/// <param name='aFadeOutTime'>
	/// The time in seconds to fade out.
	/// </param>
	/// <param name='aFadeInTime'>
	/// The time in seconds to fade in.
	/// </param>
	/// <param name='aColor'>
	/// The colour of the quad to use during the fade.
	/// </param>
	public static void LoadLevel(int aLevelIndex,float aFadeOutTime, float aFadeInTime, Color aColor)
    {
        if (Fading) return;
		
		if (!Platform.Instance.OnGlass()) {
            JSONObject json = new JSONObject();
			json.AddField("action", "LoadLevelFade");
			json.AddField("levelIndex", aLevelIndex);
			
			JSONObject data = new JSONObject();
			Track track = (Track)DataVault.Get("current_track");
			if (track != null) data.AddField("current_track", track.AsJson);
			else data.AddField("current_track", (JSONObject)null);
			data.AddField("race_type", DataVault.Get("race_type") as string);
			data.AddField("type", DataVault.Get("type") as string);
			if (DataVault.Get("finish") != null) data.AddField("finish", (int)DataVault.Get("finish"));
			if (DataVault.Get("lower_finish") != null) data.AddField("lower_finish", (int)DataVault.Get("lower_finish"));
			if (DataVault.Get("challenger") != null) data.AddField("challenger", DataVault.Get("challenger") as string);
			if (DataVault.Get("current_challenge_notification") != null) data.AddField("current_challenge_notification", (DataVault.Get("current_challenge_notification") as ChallengeNotification).AsJson);
			
			json.AddField("data", data);
			Platform.Instance.BluetoothBroadcast(json);
			// TODO: Show "in game" message and return
		}		
		
        Instance.mLevelName = "";
        Instance.mLevelIndex = aLevelIndex;
        Instance.StartFade(aFadeOutTime, aFadeInTime, aColor);
    }
}
