using UnityEngine;
using System.Collections;

//used to forward some settings deeper into structure keeping it clean as a prefab

[ExecuteInEditMode]
public class UIComponentSettings : MonoBehaviour {
	
	private string label;
	private UILabel labelInstance;
    
	public string textLabel
	{
		get
		{
			return label;
		}
		
		set
		{
			label = value;
			SetLabel(label);
		}
	}
	
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
			(labelInstance.text != textLabel))
		{
			labelInstance.text = textLabel;	
			labelInstance.MarkAsChanged();
		}
	}

	void UpdateFromLabel()
	{
		if (labelInstance != null)
		{
			textLabel = labelInstance.text;
		}
	}
    
    /// <summary>
    /// If Object support on click and redirects it to ComponentSetting class you can continue with handling this event from this place by either overriding or changing this class directly
    /// </summary>
    /// <returns></returns>
    public virtual void OnClick()
    {
        if (UIButton.current != null)
        {
            Debug.Log("Event 'Click' received from " + UIButton.current.name);
        }        
    }
}

