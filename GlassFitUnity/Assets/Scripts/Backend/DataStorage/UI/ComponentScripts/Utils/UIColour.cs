using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// component introducing string system for color representation. Used for database based color update
/// </summary>
[ExecuteInEditMode]
public class UIColour : UIComponentSettings {

    public string colorInt = "AABBAABB";
    public string databaseIDName = "";        
    
    public float r
    {
        get 
        { 
            string sColor = colorInt.Substring(0,2);
            int color = Convert.ToInt32(sColor, 16);
            return (float)(color) / (float)(0xFF); 
        }
        set 
        {
            value = Mathf.Clamp(value, 0.0f, 1.0f);
            int color = Mathf.RoundToInt(value * 0xFF);
            string val = color.ToString("X2");
            colorInt = "" + val + colorInt.Substring(2, 6);
        }
    }

    public float g
    {
        get
        {
            string sColor = colorInt.Substring(2, 2);
            int color = Convert.ToInt32(sColor, 16);
            return (float)(color) / (float)(0xFF);
        }
        set
        {
            value = Mathf.Clamp(value, 0.0f, 1.0f);
            int color = Mathf.RoundToInt(value * 0xFF);
            string val = color.ToString("X2");
            colorInt = colorInt.Substring(0, 2) + val + colorInt.Substring(4, 4);
        }
    }

    public float b
    {
        get
        {
            string sColor = colorInt.Substring(4, 2);
            int color = Convert.ToInt32(sColor, 16);
            return (float)(color) / (float)(0xFF);
        }
        set
        {
            value = Mathf.Clamp(value, 0.0f, 1.0f);
            int color = Mathf.RoundToInt(value * 0xFF);
            string val = color.ToString("X2");
            colorInt = colorInt.Substring(0, 4) + val + colorInt.Substring(6, 2);
        }
    }

    public float a
    {
        get
        {
            string sColor = colorInt.Substring(6, 2);
            int color = Convert.ToInt32(sColor, 16);
            return (float)(color) / (float)(0xFF);
        }
        set
        {
            value = Mathf.Clamp(value, 0.0f, 1.0f);
            int color = Mathf.RoundToInt(value * 0xFF);
            string val = color.ToString("X2");
            colorInt = colorInt.Substring(0, 6) + val + "";
        }
    }

    //public Color sprite;
	private UISprite spriteInstance;    
	
	/// <summary>
	/// default unity initialization function. Pulls data from color from sprite among children of the gameobject linked to the component
	/// </summary>
	/// <returns></returns>
	void Awake()
	{
		UISprite[] sprites = GetComponentsInChildren<UISprite>();
		
		if (sprites.Length != 1 )
		{
			Debug.LogError("Color system on buttons expect only one and minimum one sprite");
			return;
		}
		spriteInstance = sprites[0];
		UpdateFromSprite();
	}

    /// <summary>
    /// builds color from r, g, b, a components
    /// </summary>
    /// <returns>current color structure</returns>
    public Color GetColor()
    {
        return new Color(r, g, b, a);
    }

    /// <summary>
    /// builds vector from r, g, b, a components
    /// </summary>
    /// <returns>current color structure as a Vector4 </returns>
    public Vector4 GetVector()
    {
        return new Vector4(r, g, b, a);
    }

	/// <summary>
	/// sets color to linked sprite
	/// </summary>
	/// <param name="c">color which should be set to the component</param>
	/// <returns></returns>
	void SetColour(Color c)
	{
        if (spriteInstance == null)
        {
            Debug.LogError("Instance not awakened");
            return;
        }
		Vector4 col1 = spriteInstance.color;
		Vector4 col2 = c;
		if (spriteInstance != null && (col1 != col2))
		{
			spriteInstance.color = c;	
			spriteInstance.MarkAsChanged();
		}
	}

	/// <summary>
	/// reads build in color values from sprite to UIColor component
	/// </summary>
	/// <returns></returns>
	void UpdateFromSprite()
	{
        if (spriteInstance != null)
		{
			Color color = spriteInstance.color;
            r = color.r;
            g = color.g;
            b = color.b;
            a = color.a;
		}
	}
    
    /// <summary>
    /// tries to load color form database and then set it to sprite component
    /// </summary>
    /// <returns></returns>
    override public void Apply()
    {
        base.Apply();
        UpdateFromDatabase();
        SetColour(GetColor());
    }

    /// <summary>
    /// registers component for updates on color variable
    /// </summary>
    /// <returns></returns>
    public override void Register()
    {
        base.Register();
        if (databaseIDName != null && databaseIDName != "")
        {
            DataVault.RegisterListner(this, databaseIDName);
        }

        Apply();
    }

    /// <summary>
    /// loads color stored in database and stores it for further usage on local variable
    /// </summary>
    /// <returns></returns>
    public void UpdateFromDatabase()
    {
        if (databaseIDName != null)
        {
            System.Object o = DataVault.Get(databaseIDName);
            if (o != null)
            {
                string color = Convert.ToString(o);
                if (color != null && color.Length == 8)
                {
                    colorInt = color;
                }
            }
        }
    }    
}
