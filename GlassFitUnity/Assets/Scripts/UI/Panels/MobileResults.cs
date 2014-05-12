using UnityEngine;
using System.Collections;

public class MobileResults : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void ContinueToMenu () {
		FlowState.FollowFlowLinkNamed("MainMenu");
	}
}
