using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

/// <summary>
/// advanced panel which allows to display complex hex menu screens
/// </summary>
[Serializable]
public class HexPanel : Panel 
{
    public const string CAMERA_3D_LAYER = "GUI3D";
    const string defaultExit = "Default Exit";

    public List<HexButtonData> buttonData;
    public List<HexButtonData> buttonDataSource;
    private bool camStartMouseAction;
    private bool camStartTouchAction;
    private bool dirtyButtonData;

    /// <summary>
    /// default constructor
    /// </summary>
    /// <returns></returns>
    public HexPanel()
        : base()
    {
        buttonData = new List<HexButtonData>();
        buttonDataSource = new List<HexButtonData>();
    }

    /// <summary>
    /// deserialization constructor
    /// </summary>
    /// <param name="info">seirilization info conataining class data</param>
    /// <param name="ctxt">serialization context </param>
    /// <returns></returns>
    public HexPanel(SerializationInfo info, StreamingContext ctxt)
        : base(info, ctxt)
    {
        foreach (SerializationEntry entry in info)
        {
            switch (entry.Name)
            {
                case "buttonData":
                    this.buttonDataSource = entry.Value as List<HexButtonData>;
                    this.buttonData = CopyButtonList(this.buttonDataSource);
                    break;                
            }
        }
    }

    /// <summary>
    /// Resets button data to serialized state. Use this only before EnterStart, this will not change currently visible buttons
    /// </summary>
    /// <returns></returns>
    public void ResetButtonData()
    {
        this.buttonData = CopyButtonList(this.buttonDataSource);
    }

    /// <summary>
    /// serialization function called by serializer
    /// </summary>
    /// <param name="info">serialziation info where all data would be pushed to</param>
    /// <param name="ctxt">serialzation context</param>
    /// <returns></returns>
    public override void GetObjectData(SerializationInfo info, StreamingContext ctxt)
    {
        base.GetObjectData(info, ctxt);

        info.AddValue("buttonData", this.buttonData);
    }

    /// <summary>
    /// Gets display name of the node, helps with node identification in editor
    /// </summary>
    /// <returns>name of the node</returns>
    public override string GetDisplayName()
    {
        //base.GetDisplayName();

        GParameter gName = Parameters.Find(r => r.Key == "Name");
        if (gName != null)
        {
            return "HEX Panel: " + gName.Value;
        }
        return "HEX Panel: UnInitialzied";
    }

    /// <summary>
    /// initializes node and creates name for it. Makes as well default input/output connection sockets
    /// </summary>
    /// <returns></returns>
    protected override void Initialize()
    {
        base.Initialize();

        NewOutput(defaultExit, "Flow");
        NewParameter("HexListManager", GraphValueType.HexButtonManager, "true"); //fake variable just to trigger option visibility on graph editor

    }   

    /// <summary>
    /// refreshes connections lists
    /// </summary>
    /// <returns></returns>
    public override void RebuildConnections()
    {
        base.RebuildConnections();

        NewOutput(defaultExit, "Flow");
    }

    /// <summary>
    /// function called when screen started enter process
    /// </summary>
    /// <returns></returns>
    public override void EnterStart()
    {
		
	    //UnityEngine.Debug.Log("TutorialPanel: setting two finger tap");
		
        base.EnterStart();

        if (physicalWidgetRoot != null)
        {
            DynamicHexList list = (DynamicHexList)physicalWidgetRoot.GetComponentInChildren(typeof(DynamicHexList));            
            list.SetParent(this);            
        }

        SortHexesFromCenter();
#if !UNITY_EDITOR         
        Camera[] camList = (Camera[])Camera.FindObjectsOfType(typeof(Camera));
        UICamera uicam = null;
        foreach (Camera c in camList)
        {
            uicam = c.GetComponent<UICamera>();
            if (uicam != null && c.gameObject.layer == LayerMask.NameToLayer(CAMERA_3D_LAYER))
            {                
                break;
            }
        }

        if (uicam != null)
        {         
            
            camStartMouseAction = uicam.useMouse;
            camStartTouchAction = uicam.useTouch;                        

            /* do not block button clicks, current design is expecting clicks on hexes
            uicam.useMouse = false;
            uicam.useTouch = false;      */          
        }                   
#endif

    }

    /// <summary>
    /// function called when panels started exiting process
    /// </summary>
    /// <returns></returns>
    public override void ExitStart()
    {
        base.ExitStart();
        
        foreach (HexButtonData data in buttonData)
        {
            data.markedForVisualRefresh = true;
        } 
    }

    /// <summary>
    /// exit finalization and clearing process
    /// </summary>
    /// <returns></returns>
    public override void Exited()
    {
        base.Exited();

        UICamera cam = GameObject.FindObjectOfType(typeof(UICamera)) as UICamera;
        if (cam != null)
        {
            cam.transform.rotation = Quaternion.identity;
        }

        DynamicHexList dh = physicalWidgetRoot.GetComponentInChildren<DynamicHexList>();
        if (dh != null)
        {
            dh.OnExit();
        }

#if !UNITY_EDITOR            
        Camera[] camList = (Camera[])Camera.FindObjectsOfType(typeof(Camera));
        UICamera uicam = null;
        foreach (Camera c in camList)
        {
            uicam = c.GetComponent<UICamera>();
            if (uicam != null && c.gameObject.layer == LayerMask.NameToLayer(CAMERA_3D_LAYER))
            {                
                break;
            }
        }

        if (uicam != null)
        {           
            uicam.useMouse = camStartMouseAction;
            uicam.useTouch = camStartTouchAction;                                    
        }             
#endif
    }

    /// <summary>
    /// whenever button get clicked it would be handled here
    /// </summary>
    /// <param name="button">button which send this event</param>
    /// <returns></returns>
    public override void OnClick(FlowButton button)
    {
        //base.OnClick(button);

        //check type of click
        if (Input.touchCount > 1)
        {
            //two fingers click are not click event for us
            return;
        }

        

        if (Outputs.Count > 0 && parentMachine != null)
        {
            GConnector gConect = Outputs.Find(r => r.Name == button.name);
            if (gConect != null)
            {
				SoundManager.PlaySound(SoundManager.Sounds.Tap);
                ConnectionWithCall(gConect, button);
            }
            else
            {
                gConect = Outputs.Find(r => r.Name == defaultExit);
                if (gConect != null)
                {
                    ConnectionWithCall(gConect, button);
                }
            }
        }
        else
        {
            Debug.LogError("Dead end");
        }
    }


    /// <summary>
    /// feedback function informing when button get hover over and hover out states. it is useful if some external systems need to be triggered on those events
    /// function would be called in most cases twice, once for button leaving and then second time for button entering in this order.
    /// </summary>
    /// <param name="button">button which trigered evet. </param>
    /// <param name="justStarted">information if its entering or leaving event</param>
    /// <returns></returns>
    public virtual void OnHover(FlowButton button, bool justStarted)
    {
        if (justStarted)
        {
            HexInfoManager info = GameObject.FindObjectOfType(typeof(HexInfoManager)) as HexInfoManager;
            if (info != null)
            {
               // if (!info.IsInOpenStage())
               // {
                    HexButtonData data = button.userData["HexButtonData"] as HexButtonData;
                    DataVault.Set(HexInfoManager.DV_HEX_DATA, data);
                    info.PrepareForNewData();
                    return;
              //  }
            }
        }
    }

    /// <summary>
    /// checks if class have button data and at least one button. As well do the parent checks if screen type is set
    /// </summary>
    /// <returns></returns>
    public override bool IsValid()
    {
        //this panel could be invalid if no buttons are defined but because this screen is defined dynamically we might have screen with no buttons before runtime
        return base.IsValid();
    }

    /// <summary>
    /// Function which allows to check if data contains button at certain coordinate position to protect from double position usage
    /// </summary>
    /// <param name="column">column coordinate in hex menu</param>
    /// <param name="row">row coordinate in hex menu. Note that in hexes row is a horisontal "zigzac" based on number elements from the center "zigzac" axis.</param>
    /// <returns>true if slot is empty</returns>
    public bool IsPositionEmpty(int column, int row)
    {        
        return GetButtonAt(column, row) == null;
    }

    /// <summary>
    /// Finds button data bound to provided coordinates
    /// </summary>
    /// <param name="column">column coordinate in hex menu</param>
    /// <param name="row">row coordinate in hex menu. Note that in hexes row is a horisontal "zigzac" based on number elements from the center "zigzac" axis.</param>
    /// <returns>null if slot is empty, object if button data exist at provided coordinates</returns>
    public HexButtonData GetButtonAt(int column, int row)
    {
        HexButtonData hbd = buttonData.Find(r => ((r.column == column) && (r.row == row)));
        return hbd;
    }

    /// <summary>
    /// Overridden version of widget root name. Allows to differ widget root designed for 3d assets
    /// </summary>
    /// <returns>const name widget root name</returns>
    public override string GetWidgetRootName()
    {
        return "Widgets Container3D";
    }

    /// <summary>
    /// Allows to sort hexes in order to show them in more desirable sequence starting from center -> out
    /// </summary>
    /// <returns></returns>
    public void SortHexesFromCenter()
    {
        buttonData.Sort
            (
                delegate(HexButtonData p1, HexButtonData p2)
                {
                    Vector2 pos1 = DynamicHexList.GetLocation(p1.column, p1.row);
                    Vector2 pos2 = DynamicHexList.GetLocation(p2.column, p2.row);
                    float sqrMag1 = pos1.sqrMagnitude;
                    float sqrMag2 = pos2.sqrMagnitude;

                    if (sqrMag1 < sqrMag2)
                    {
                        return -1;
                    }
                    if (sqrMag1 > sqrMag2)
                    {
                        return 1;
                    }

                    return 0;
                }

            );
    }


    /// <summary>
    /// Util function for copying list of button data. Used to ensure we have source for screen creation
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public List<HexButtonData> CopyButtonList(List<HexButtonData> source)
    {
        List<HexButtonData> newList = new List<HexButtonData>();
        for (int i=0; i<source.Count; i++)
        {
            newList.Add(source[i].Copy());
        }

        return newList;
    }
	
	///Utility function to populate hexes based on a single index for hexes arranged in a spiral pattern.
	protected Vector2i getHexCoordsForSpiralIndex(int i)
	{
		switch(i)
		{
		case 0:
			return new Vector2i(0,0);
			break;
		case 1:
			return new Vector2i(0,-1);
			break;
		case 2:
			return new Vector2i(1,0);
			break;
		case 3:
			return new Vector2i(0,1);
			break;
		case 4:
			return new Vector2i(-1,1);
			break;
		case 5:
			return new Vector2i(-1,0);
			break;
		case 6:
			return new Vector2i(-1,0);
			break;
		case 7:
			return new Vector2i(0,-2);
			break;
		case 8:
			return new Vector2i(1,-2);
			break;
		case 9:
			return new Vector2i(1,-1);
			break;
		case 10:
			return new Vector2i(2,0);
			break;
		case 11:
			return new Vector2i(1,1);
			break;
		case 12:
			return new Vector2i(1,2);
			break;
		case 13:
			return new Vector2i(0,2);
			break;
		case 14:
			return new Vector2i(-1,2);
			break;
		case 15:
			return new Vector2i(-2,1);
			break;
		case 16:
			return new Vector2i(-2,0);
			break;
		case 17:
			return new Vector2i(-2,-1);
			break;			
		case 18:
			return new Vector2i(-1,-2);
			break;			
		default:
			UnityEngine.Debug.LogError("HexPanel: Spiral index not yet defined for index " + i);
			return new Vector2i(-1,-3);
			break;
		}
	}
}
