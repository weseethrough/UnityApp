using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// Multi label is a class which finds labels in child structure of attached Game Object and links itself there for linking with database
/// </summary>
[ExecuteInEditMode]
public class UIMultiLabel : UIComponentSettings 
{
	
	private string[] labelsStrings;
	private UILabel[] labelInstances;    
	
	/// <summary>
	/// default unity initialization function, reads basic settings from associated label.
	/// </summary>
	/// <returns></returns>
	void Awake()
	{
		labelInstances = GetComponentsInChildren<UILabel>();
        if (labelInstances != null && labelInstances.Length > 0)
        {
            labelsStrings = new string[labelInstances.Length];
            UpdateFromLabels();
            Register();
            Apply();
        }
	}
	
	/// <summary>
	/// allows to set new string to linked label and marks it to refresh
	/// </summary>
	/// <param name="s">string to be set on label</param>
	/// <returns></returns>
	void SetLabel(string s, int index)
	{
		if ( labelInstances[index] != null &&
            (labelInstances[index].text != s))
		{
            labelInstances[index].text = s;
            labelInstances[index].MarkAsChanged();
		}
	}

	/// <summary>
	/// read label and store value on local buffer, this restores build in value, but usually later if any other form contains changed value this one would be overriden
	/// </summary>
	/// <returns></returns>
	void UpdateFromLabels()
	{
        for (int i = 0; i < labelInstances.Length; i++)
        {
            if (labelInstances != null)
            {
                labelsStrings[i] = labelInstances[i].text;
            }
        }
	}
    
    /// <summary>
    /// requests translated value of the label variable and sets it label component
    /// </summary>
    /// <returns></returns>
    override public void Apply()
    {
        base.Apply();
        SetTranslatedText(false);
    }

    /// <summary>
    /// registers current label contained string for possible database translations and updates
    /// </summary>
    /// <returns></returns>
    public override void Register()
    {
        base.Register();
        SetTranslatedText(true);
    }

    /// <summary>
    /// translate current label variable and feeds text field with result, might register for later updates on it as well
    /// </summary>
    /// <param name="register">should label register for changes on identified database links</param>
    /// <returns></returns>
    public void SetTranslatedText(bool register)
    {
        for (int i = 0; i < labelsStrings.Length; i++)
        {
            SetLabel(DataVault.Translate(labelsStrings[i], register ? this : null), i);
        }
    }
}

