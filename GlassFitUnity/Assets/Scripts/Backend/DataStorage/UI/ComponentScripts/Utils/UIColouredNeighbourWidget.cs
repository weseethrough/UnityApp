using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// component introducing string system for color representation. Used for database based color update
/// </summary>
[ExecuteInEditMode]
public class UIColouredNeighbourWidget : UIComponentSettings
{
	private UIWidget widgetInstance;

    public string colorInt = "AABBAABB";
    public string databaseIDName = "";

    public float r
    {
        get
        {
            string sColor = colorInt.Substring(0, 2);
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

    /// <summary>
    /// Do nothing on awake
    /// </summary>
    /// <returns></returns>
    void Awake(){}

	/// <summary>
	/// initializes component finding widget to control and setting colors...
	/// </summary>
	/// <returns></returns>
	void Start()
	{
        widgetInstance = GetComponent<UIWidget>();

        if (widgetInstance == null)
        {
            Debug.LogError("Color system on widget expect widget to be dropped to this component inspector slot");
            return;
        }
        UpdateFromWidget();
        Register();
    }     
   
	/// <summary>
	/// sets color to linked sprite
	/// </summary>
	/// <param name="c">color which should be set to the component</param>
	/// <returns></returns>
	protected void SetColour(Color c)
	{        
        if (widgetInstance == null)
        {
            Debug.LogError("Instance not awakened");
            return;
        }
        Vector4 col1 = widgetInstance.color;
		Vector4 col2 = c;
        if (widgetInstance != null && (col1 != col2))
		{
            widgetInstance.color = c;
            widgetInstance.MarkAsChanged();
		}
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
	/// reads build in color values from sprite to UIColor component
	/// </summary>
	/// <returns></returns>
    void UpdateFromWidget()
	{
        if (widgetInstance != null)
		{
            Color color = widgetInstance.color;
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
        //we don't want color functionality which makes us skip base.apply
        //base.Apply();
        UpdateFromDatabase();
        SetColour(GetColor());
    }

    /// <summary>
    /// registers component for updates on color variable
    /// </summary>
    /// <returns></returns>
    public override void Register()
    {
        //we don't want color functionality which makes us skip base.Register
        //base.Register();
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
