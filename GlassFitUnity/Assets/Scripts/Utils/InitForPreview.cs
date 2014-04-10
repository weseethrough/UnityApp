using UnityEngine;
using System.Collections;

#if UNITY_EDITOR

/// <summary>
/// Init for preview.
/// Should automatically detect the platform and, if in the editor, does the relevant setup to allow previewing.
/// </summary>
public class InitForPreview : MonoBehaviour {

	UnityEditor.EditorApplication.CallbackFunction playStateChanged;
	
	public Camera mainCamera;
	public GameObject dataStorage;
	public GameObject flow;
	public GameObject uiScene;
	protected bool currentlyInPlayMode = false;
	
	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start () 
	{
		UnityEditor.EditorApplication.playmodeStateChanged += PlayStateChanged;
	}

	public void PlayStateChanged()
	{
		//if we stopped playing, deactivate the relevant objects
		if(currentlyInPlayMode)
		{
			if(!UnityEditor.EditorApplication.isPlaying)
			{
				currentlyInPlayMode = false;
				if(!UnityEditor.EditorApplication.isPaused)
				{
					ExitedPlayMode();
				}
			}
		}
		else
		{
			if(UnityEditor.EditorApplication.isPlaying)
			{
				currentlyInPlayMode = true;
				//PrepareForPlayMode ();
			}
		}
	}

	public void PrepareForPlayMode()
	{
		dataStorage.SetActive(true);
		flow.SetActive(true);
		uiScene.SetActive(true);

		//create a platform partner
		GameObject platformPartner = new GameObject();
		platformPartner.AddComponent<PlatformPartner>();
		platformPartner.name = "Platform Partner";
		//mainCamera.clearFlags = CameraClearFlags.SolidColor;
	}

	public void ExitedPlayMode()
	{
		dataStorage.SetActive(false);
		flow.SetActive(false);
		uiScene.SetActive(false);

		//find and remove temporary platform partner
		GameObject pp = GameObject.Find("Platform Partner");
		if(pp != null)
		{
			DestroyImmediate(pp);
		}
		else
		{
			UnityEngine.Debug.LogError("InitForPreview: Couldn't find platform partner to destroy on returning to edit mode");
		}
	}

	// Update is called once per frame
	void Update () {
	
	}
}

#endif