using UnityEngine;
using System.Collections;

public class softwareBackButton : MonoBehaviour {

	void Awake () {
		if(Platform.Instance.RequiresSoftwareBackButton())
		if(false)
		{
			UnityEngine.Debug.Log("SoftwareBackButton: enabling software back button");
			enabled = true;
		}
		else
		{
			UnityEngine.Debug.Log("SoftwareBackButton: disabling software back button");
			enabled = false;
		}
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void OnClick () {
		//treat as a back gesture
		GestureHelper.onBack();
	}
}
