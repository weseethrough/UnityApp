using UnityEngine;
using System.Collections;

public class ChangeColour : MonoBehaviour {

	Color startColor;
	Color finalColor;

	bool isChanging = false;

	public void SetColours(Color end) {
		startColor = GetComponent<UIWidget>().color;
		finalColor = end;
	}

	public bool IsChanging() {
		return isChanging;
	}

	IEnumerator Change() {
		isChanging = true;
		UIWidget widget = GetComponent<UIWidget>();
		float time = 0.0f;
		while(time < 1.0f) {
//			UnityEngine.Debug.LogError((Color.Lerp(startColor, finalColor, time).ToString()));
			widget.color = Color.Lerp(startColor, finalColor, time);
			time += Time.deltaTime * 3;
			yield return null;
		}
		isChanging = false;
		widget.color = Color.white;
//		ChallengeControllerPanel.SetNewChallenge();
	}

	public void StartChange() {
		StartCoroutine(Change());
	}

	public void EndSection() {
		StartCoroutine(EndChallenges());
	}

	IEnumerator EndChallenges() {
		yield return new WaitForSeconds(0.5f);
		FlowState.FollowFlowLinkNamed("Exit");
	}
}
