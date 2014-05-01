using UnityEngine;
using System.Collections;

public class SoftwareBackButton : MonoBehaviour {

	void Awake () {
		if(Platform.Instance.ProvidesBackButton())
		{
            UnityEngine.Debug.Log("SoftwareBackButton: disabling software back button");
            gameObject.SetActive(false);
		}
		else
		{
            UnityEngine.Debug.Log("SoftwareBackButton: enabling software back button");
            gameObject.SetActive(true);
		}
	}

	public void OnClick ()
    {
		//treat as a back gesture
		GestureHelper.onBack();
	}
}
