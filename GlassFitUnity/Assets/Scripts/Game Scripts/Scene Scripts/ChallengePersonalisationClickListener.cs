using UnityEngine;
using System.Collections;

public class ChallengePersonalisationClickListener : MonoBehaviour {
    private static Log log = new Log("ChallengePersonalisationClickListener");

    void OnClick()
    {
        ChallengeControllerPanel panel = FlowStateMachine.GetCurrentFlowState() as ChallengeControllerPanel;
        if (gameObject.name == "DislikeIcon")
            panel.DislikeChallenge();
        else if (gameObject.name == "LikeIcon")
            panel.LikeChallenge();
        else
            log.error("Cannot determine whether the user clicked on 'Dislike' or 'Like'. Code depends on name " +
                "of GameObjects - should be 'DislikeIcon' and 'LikeIcon'.");
    }
}