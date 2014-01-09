using UnityEngine;
using System.Collections;


/// <summary>
/// Basic label is a class which finds single label in child structure of attached Game Object and links itself there for display information management and linking with database and flow settings
/// </summary>
[ExecuteInEditMode]
public class UIBasiclabel : UIComponentSettings 
{
	
	public string label;
	private UILabel labelInstance;    
	
	/// <summary>
	/// default unity initialization function, reads basic settings from associated label.
	/// </summary>
	/// <returns></returns>
	void Awake()
	{
		UILabel[] labels = GetComponentsInChildren<UILabel>();
		if (labels.Length < 1 )
		{
			Debug.LogError("Label system on buttons expect minimum one label. Object: "+gameObject.name);
			return;
		}
        if (labels.Length > 1)
        {
            Debug.LogError("Label system on buttons expect only one label. Found: " + labels.Length + "; Object: " + gameObject.name);            
        }

		labelInstance = labels[0];
		UpdateFromLabel();
	}
	
	/// <summary>
	/// allows to set new string to linked label and marks it to refresh
	/// </summary>
	/// <param name="s">string to be set on label</param>
	/// <returns></returns>
	void SetLabel(string s)
	{
		if ( labelInstance != null && 
			(labelInstance.text != s))
		{
			labelInstance.text = s;	
			labelInstance.MarkAsChanged();
		}
	}

	/// <summary>
	/// read label and store value on local buffer, this restores build in value, but usualy later if any other form contains changed value this one would be overriden
	/// </summary>
	/// <returns></returns>
	void UpdateFromLabel()
	{
		if (labelInstance != null)
		{
			label = labelInstance.text;
		}
	}
    
    /// <summary>
    /// requests translated value of the label variabe and sets it label component
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
    /// translate curent label variable and feeds text field with result, might register for later updates on it as well
    /// </summary>
    /// <param name="register">should label register for changes on identified database links</param>
    /// <returns></returns>
    public void SetTranslatedText(bool register)
    {
        SetLabel(DataVault.Translate(label, register? this : null));
    }
}

