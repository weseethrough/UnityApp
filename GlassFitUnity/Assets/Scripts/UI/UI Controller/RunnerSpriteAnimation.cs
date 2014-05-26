using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
[RequireComponent(typeof(UISprite))]
[AddComponentMenu("NGUI/UI/Runner Sprite Animation")]
public class RunnerSpriteAnimation : MonoBehaviour {

	UISprite mSprite;
	float mDelta = 0f;
	int mIndex = 0;
	bool mActive = true;
	List<string> mSpriteNames = new List<string>();

	/// <summary>
	/// Is this sprite stationary and should show the idle sprite
	/// </summary>

	public bool stationary = true;

	/// <summary>
	/// Number of frames in the animation.
	/// </summary>
	
	public int frames { get { return mSpriteNames.Count; } }
	
	/// <summary>
	/// Animation framerate.
	/// </summary>
	
	//public int framesPerSecond { get { return mFPS; } set { mFPS = value; } }
	public int framesPerSecond = 0;


	/// <summary>
	/// Set the name prefix used to filter sprites from the atlas.
	/// </summary>
	
	public string namePrefix;
	
	/// <summary>
	/// Set the animation to be looping or not
	/// </summary>
	
	public bool loop;
	
	/// <summary>
	/// Returns is the animation is still playing or not
	/// </summary>
	
	public bool isPlaying { get { return mActive; } }
	
	/// <summary>
	/// Rebuild the sprite list first thing.
	/// </summary>
	
	public string idleSpriteName = "runner_idle";


	void Start () { RebuildSpriteList(); }
	
	/// <summary>
	/// Advance the sprite animation process.
	/// </summary>
	
	void Update ()
	{
		if (mActive && mSpriteNames.Count > 1 && Application.isPlaying && framesPerSecond > 0f)
		{
			mDelta += Time.deltaTime;
			float rate = 1f / framesPerSecond;
			
			if (rate < mDelta)
			{
				
				mDelta = (rate > 0f) ? mDelta - rate : 0f;
				if (++mIndex >= mSpriteNames.Count)
				{
					mIndex = 0;
					mActive = loop;
				}
				
				if (mActive)
				{
					//use running anim if we're going fast enough, else use idle
					if(!stationary)
					{
						mSprite.spriteName = mSpriteNames[mIndex];
					}
					else
					{
						mSprite.spriteName = idleSpriteName;
					}
					mSprite.MakePixelPerfect();
				}
			}
		}
	}
	
	/// <summary>
	/// Rebuild the sprite list after changing the sprite name.
	/// </summary>
	
	void RebuildSpriteList ()
	{
		if (mSprite == null) mSprite = GetComponent<UISprite>();
		mSpriteNames.Clear();
		
		if (mSprite != null && mSprite.atlas != null)
		{
			List<UISpriteData> sprites = mSprite.atlas.spriteList;
			
			for (int i = 0, imax = sprites.Count; i < imax; ++i)
			{
				UISpriteData sprite = sprites[i];
				
				if (string.IsNullOrEmpty(namePrefix) || sprite.name.StartsWith(namePrefix))
				{
					mSpriteNames.Add(sprite.name);
				}
			}
			mSpriteNames.Sort();
		}
	}
	
	/// <summary>
	/// Reset the animation to frame 0 and activate it.
	/// </summary>
	
	public void Reset()
	{
		mActive = true;
		mIndex = 0;
		
		if (mSprite != null && mSpriteNames.Count > 0)
		{
			mSprite.spriteName = mSpriteNames[mIndex];
			mSprite.MakePixelPerfect();
		}
	}
}
