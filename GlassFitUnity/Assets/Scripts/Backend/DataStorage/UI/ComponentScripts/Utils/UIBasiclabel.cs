using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class UIBasiclabel : UIComponentSettings {
	
	public string label;
	private UILabel labelInstance;    
	
	void Awake()
	{
		UILabel[] labels = GetComponentsInChildren<UILabel>();
		if (labels.Length != 1 )
		{
			Debug.LogError("Label system on buttons expect only one and minimum one label");
			return;
		}
		
		labelInstance = labels[0];
		UpdateFromLabel();
	}
	
	void SetLabel(string s)
	{
		if ( labelInstance != null && 
			(labelInstance.text != s))
		{
			labelInstance.text = s;	
			labelInstance.MarkAsChanged();
		}
	}

	void UpdateFromLabel()
	{
		if (labelInstance != null)
		{
			label = labelInstance.text;
		}
	}
    
    override public void Apply()
    {
        base.Apply();
        SetTranslatedText(false);
    }

    public override void Register()
    {
        base.Register();

        SetTranslatedText(true);
    }

    public void SetTranslatedText(bool register)
    {
        SetLabel(DataVault.Translate(label, register? this : null));
    }
}

