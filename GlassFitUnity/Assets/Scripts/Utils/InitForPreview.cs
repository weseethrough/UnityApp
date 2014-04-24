using UnityEngine;
using System.Collections;

/// <summary>
/// Init for preview.
/// Should automatically detect the platform and, if in the editor, does the relevant setup to allow previewing.
/// </summary>
public class InitForPreview : MonoBehaviour {
	
	public Camera mainCamera;
	public GameObject dataStorage;
	public GameObject flow;
	public GameObject uiScene;
	
	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start () 
	{
		if(Application.isEditor)
		{
			
			Debug.Log("setting up for in editor");
			
			//activate the Flow, UIScene and DataStorage objects
			dataStorage.SetActive(true);
			flow.SetActive(true);
			uiScene.SetActive(true);
			
			//Set the camera to clear everything
			mainCamera.clearFlags = CameraClearFlags.SolidColor;

			//do we also need to do something about the flow?
		}
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
