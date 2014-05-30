using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using RaceYourself;
using RaceYourself.Models;
using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

[Serializable]
public class MobileFirstRunQuestionnaire : MobilePanel {

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

            Profile profile = Platform.Instance.api.user.profile;
            switch (fitnessLevel)
            {
                case "out of shape":
                    profile.runningFitness = "Out Of Shape";
                    break;
                case "average":
                    profile.runningFitness = "Average";
                    break;
                case "athletic":
                    profile.runningFitness = "Athletic";
                    break;
                case "elite":
                    profile.runningFitness = "Elite";
                    break;
            }
            var profileHash = new Hashtable();
            profileHash["running_fitness"] = profile.runningFitness;

            Platform.Instance.partner.StartCoroutine(Platform.Instance.api.UpdateUser(null, null, null, null, null, profileHash));

            DataVault.Set("fitness_level", fitnessLevel); // TODO find refs, replace with profile

            GConnector gc = Outputs.Find(r => r.Name == "Exit");
            if (gc != null)
            {
                parentMachine.FollowConnection(gc);
            }
        }
    }
}