using UnityEngine;
using System.Collections;

public class softwareBackButton : MonoBehaviour {

	void Awake () {
		if(Platform.Instance.RequiresSoftwareBackButton())
		{
			UnityEngine.Debug.Log("SoftwareBackButton: enabling software back button");
			gameObject.SetActive(true);
		}
		else
		{
			UnityEngine.Debug.Log("SoftwareBackButton: disabling software back button");
			gameObject.SetActive(false);
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
