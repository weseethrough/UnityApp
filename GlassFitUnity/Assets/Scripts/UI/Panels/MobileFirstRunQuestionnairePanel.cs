using UnityEngine;
using System.Collections.Generic;
using RaceYourself;
using RaceYourself.Models;
using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

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
    
    /// <summary>
    /// default constructor
    /// </summary>
    /// <returns></returns>
    public MobileFirstRunQuestionnaire() : base() {}
    
    /// <summary>
    /// deserialziation constructor
    /// </summary>
    /// <param name="info">seirilization info conataining class data</param>
    /// <param name="ctxt">serialization context </param>
    /// <returns></returns>
    public MobileFirstRunQuestionnaire(SerializationInfo info, StreamingContext ctxt)
        : base(info, ctxt)
    {    
        
    }

	// Use this for initialization
    public override void Entered()
    {
        base.Entered();
        
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
		string cached = Platform.Instance.api.getCached("matches", false);
		if (cached != null) {
			Dictionary<string,Dictionary<string,List<Track>>> matches = JsonConvert.DeserializeObject<
				RaceYourself.API.SingleResponse<Dictionary<string,Dictionary<string,List<Track>>>>>(cached).response;
			DataVault.Set("matches", matches);
		} else {
			Platform.Instance.partner.StartCoroutine(Platform.Instance.api.get("matches", body => {
				if (body == null) {
					UnityEngine.Debug.LogError("Could not get matches from server. Using bundled matches");
					body = Platform.Instance.ReadAssetsString("default_matches.json");
				}

	            Dictionary<string,Dictionary<string,List<Track>>> matches = JsonConvert.DeserializeObject<
	                RaceYourself.API.SingleResponse<Dictionary<string,Dictionary<string,List<Track>>>>>(body).response;
	            DataVault.Set("matches", matches);
	        }));
		}
    }


}
