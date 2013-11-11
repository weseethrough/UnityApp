using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class UIColour : UIComponentSettings {

	public Color sprite;
	private UISprite spriteInstance;    
	
	void Awake()
	{
		UISprite[] sprites = GetComponentsInChildren<UISprite>();
		
		if (sprites.Length != 1 )
		{
			Debug.LogError("Label system on buttons expect only one and minimum one label");
			return;
		}
		spriteInstance = sprites[0];
		UpdateFromSprite();
	}
	
	void SetColour(Color c)
	{
		Vector4 col1 = spriteInstance.color;
		Vector4 col2 = c;
		if (spriteInstance != null && (col1 != col2))
		{
			spriteInstance.color = c;	
			spriteInstance.MarkAsChanged();
		}
	}

	void UpdateFromSprite()
	{
		if(spriteInstance != null)
		{
			sprite = spriteInstance.color;
		}
	}
    
    override public void Apply()
    {
        base.Apply();
        //SetLabel(label);
		SetColour(sprite);
        //SetTranslatedText(true);
    }

//    public void SetTranslatedText(bool register)
//    {
//        SetLabel(DataVault.Translate(label, register? this : null));
//    }

    void OnDestroy()
    {
        DataVault.UnRegisterListner(this);
    }
}
