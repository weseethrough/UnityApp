using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Helps display current player progress
/// </summary>
public class ProgressScreen : UIComponentSettings
{

    const string PROGRESS = "rank_progress";
    const string PROGRESS_TEXT = "rank_progress_text";

    private GameObject progressLine;
    private UILabel progressLineLabel;

    void Start()
    {
        progressLine = GameObject.Find("progresBarLine");
        GameObject go = GameObject.Find("progressDistanceLabel");
        if (go != null)
        {
            progressLineLabel = go.GetComponent<UILabel>();
        }

        Register();
        Apply();
    }

    public override void Register()
    {
        base.Register();

        DataVault.RegisterListner(this, PROGRESS);
        DataVault.RegisterListner(this, PROGRESS_TEXT);
    }

    public override void Apply()
    {
        base.Apply();

        if (progressLine != null)
        {
            float progress = Convert.ToSingle(DataVault.Get(PROGRESS));
            progress = Mathf.Clamp01(progress);

            Vector3 pos = progressLine.transform.localPosition;
            //0 is x=-450, 1 is x=0
            pos.x = (1.0f - progress) * (- 450.0f);
            progressLine.transform.localPosition = pos;            
        }

        if (progressLineLabel != null)
        {
            string progressText = Convert.ToString(DataVault.Get(PROGRESS_TEXT));
            progressLineLabel.text = progressText;
            progressLineLabel.MarkAsChanged();
        }

    }
}
