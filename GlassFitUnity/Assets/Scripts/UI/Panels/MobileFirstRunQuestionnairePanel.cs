using UnityEngine;
using System.Collections.Generic;
using RaceYourself;
using RaceYourself.Models;
using Newtonsoft.Json;
using System;

[Serializable]
public class MobileFirstRunQuestionnaire : MobilePanel {

    private API api;
    private Platform platform;

    private IDictionary<string, string> buttonNameToFitnessLevel = new Dictionary<string, string>() {
        {"OutOfShapeButton", "out of shape"},
        {"AverageButton", "average"},
        {"AthleticButton", "athletic"},
        {"EliteButton", "elite"}
    };

	// Use this for initialization
	void Start () {
        GetMatches();
        platform = Platform.Instance;
        api = platform.api;
	}

    public override string GetDisplayName()
    {
        base.GetDisplayName();
        
        GParameter gName = Parameters.Find(r => r.Key == "Name");
        if (gName != null)
        {
            return "MobileFirstRunQuestionnairePanel: " + gName.Value;
        }
        return "MobileFirstRunQuestionnairePanel: UnInitialized";
    }

    public override void OnClick(FlowButton button)
    {
        if (button != null)
        {
            string fitnessLevel = buttonNameToFitnessLevel[button.name];
            DataVault.Set("fitness_level", fitnessLevel);

            GConnector gc = Outputs.Find(r => r.Name == "Exit");
            if (gc != null)
            {
                parentMachine.FollowConnection(gc);
            }
        }
    }
    
    void GetMatches()
    {
        Platform.Instance.partner.StartCoroutine(Platform.Instance.api.get("matches", body => {
            IDictionary<string,IDictionary<string,IList<Track>>> matches = JsonConvert.DeserializeObject<
                RaceYourself.API.SingleResponse<IDictionary<string,IDictionary<string,IList<Track>>>>>(body).response;
            DataVault.Set("matches", matches);
        }));
    }
}
