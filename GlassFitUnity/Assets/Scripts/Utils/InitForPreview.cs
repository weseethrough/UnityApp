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
	protected GameObject platformPartner = null;
	
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
		if(!UnityEditor.EditorApplication.isPlaying && !UnityEditor.EditorApplication.isPaused)
		{
			ExitedPlayMode();
		}
	}

	public void PrepareForPlayMode()
	{
		dataStorage.SetActive(true);
		flow.SetActive(true);
		uiScene.SetActive(true);
		platformPartner = new GameObject();
		platformPartner.AddComponent<PlatformPartner>();
		//mainCamera.clearFlags = CameraClearFlags.SolidColor;
	}

	public void ExitedPlayMode()
	{
		dataStorage.SetActive(false);
		flow.SetActive(false);
		uiScene.SetActive(false);
		if(platformPartner != null)
		{
			Destroy(platformPartner);
		}
	}

	// Update is called once per frame
	void Update () {
	
	}
}

#endif