using UnityEngine;
using System.Collections;

public class MobileAwaitGPS : MonoBehaviour {

	public GameObject SkipButton;

	// Use this for initialization
	void Start () {


		StartCoroutine(showSkipAfterDelay());
	}

	IEnumerator showSkipAfterDelay()
	{
		yield return new WaitForSeconds(5f);
		SkipButton.SetActive(true);
	}

	// Update is called once per frame
	void Update () {
		//if we get GPs, proceed straight to game view
		if(Platform.Instance.LocalPlayerPosition.HasLock())
		{
			Platform.Instance.LocalPlayerPosition.SetIndoor(true);
			FlowState.FollowFlowLinkNamed("GotGPS");
		}
	}
}
