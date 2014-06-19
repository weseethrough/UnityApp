using UnityEngine;
using System.Collections;

public class MobileAwaitGPS : MonoBehaviour {

	public GameObject SkipButton;
    private Log log = new Log("MobileAwaitGPS");

	// Use this for initialization
	void Start () {
		FlowState.FollowFlowLinkNamed("Skip");

		//StartCoroutine(showSkipAfterDelay());
	}

	IEnumerator showSkipAfterDelay()
	{
		yield return new WaitForSeconds(5f);
		SkipButton.SetActive(true);
        log.info ("Showing skip button");
	}

	// Update is called once per frame
	void Update () {
		//if we get GPs, proceed straight to game view
		if(Platform.Instance.LocalPlayerPosition.HasLock())
		{
            log.info ("Got GPS lock, moving to game with indoor=false");
            Platform.Instance.LocalPlayerPosition.SetIndoor(false);
			FlowState.FollowFlowLinkNamed("GotGPS");
		}
	}
}
