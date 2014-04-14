using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Component which provides management functionality for dynamic hex screens
/// </summary>
public class DynamicHexList : MonoBehaviour
{
    const string GRAPHIC_BACKGROUND = "Background";
    const string GRAPHIC_FOREGROUND = "Foreground";
    const string GRAPHIC_PLUS       = "Plus";
    const string GRAPHIC_PADLOCK    = "Padlock";
    
	
	public Font font;
	
    HexPanel parent = null;

    UICamera guiCamera;
    public static Vector2 hexLayoutOffset = new Vector2(0.25f, 0.4330127f);
	
    //float screenEnterTime = 8.0f;
    float buttonEnterDelay = 0.0f;
    float buttonNextEnterDelay = 0.0f;
    int buttonNextEnterIndex = 0;

    int buttonCount = 0;

    List<GameObject> buttons;
    List<UIImageButton> buttonsImageComponents;
    List<Vector2> hexPosition2d;
    List<Dictionary<string, UISprite>> buttonSprites;
    List<UILabel> overlays;

    UIImageButton selection;
    private string btEnterAnimation = "HexEnter";

    bool buttonsReady = false;

    Vector2 draggingStartPos = Vector2.zero;
    bool dragging = false;
    int draggingFingerID = -1;

    GameObject buttonBase;
	
	private GestureHelper.OnTap tapHandler = null;
	
    bool initialized = false;
	
	private GestureHelper.OnBack backHandler = null;
	
	private GestureHelper.OnSwipeLeft leftHandler = null;
	
	private GestureHelper.OnSwipeRight rightHandler = null;

    /// <summary>
    /// List initialization process
    /// </summary>
    /// <returns></returns>
    void Start()
    {
		font = Resources.Load("Font/!BebasNeue") as Font;
		
		if(font != null) {
			font.RequestCharactersInTexture("qwertyuiopasdfghklzxcvbnm,./;'[]098754321QWERTYUIOPASDFGHJKLZXCVBNM<>?|:@~{}+_)(*&^%$£!¬`");
		}
		
        initialized = false;

        buttonBase = transform.GetChild(0).gameObject;

        Camera[] camList = (Camera[])Camera.FindObjectsOfType(typeof(Camera));
        foreach (Camera c in camList)
        {
            UICamera uicam = c.GetComponent<UICamera>();
            if (uicam != null && c.gameObject.layer == LayerMask.NameToLayer(HexPanel.CAMERA_3D_LAYER))
            {
				UnityEngine.Debug.Log("DynamicHexList: Camera's name is: " + c.name);
                guiCamera = uicam;
                break;
            }
        }
		
		backHandler = new GestureHelper.OnBack(() =>
		{
			GoBack();
		});
		GestureHelper.onBack += backHandler;
        
		if (Platform.Instance.OnGlass())
        {
            tapHandler = new GestureHelper.OnTap(() =>
            {
                EnterGame();
            });

            GestureHelper.onTap += tapHandler;


            leftHandler = new GestureHelper.OnSwipeLeft(() =>
            {
                if (!IsPopupDisplayed())
                {
                    GUICamera cameraScript = guiCamera.GetComponent<GUICamera>();
                    if (cameraScript.zoomLevel > -1.5f)
                    {
                        cameraScript.zoomLevel -= 0.5f;
                    }
                }
            });

            GestureHelper.onSwipeLeft += leftHandler;

            rightHandler = new GestureHelper.OnSwipeRight(() =>
            {
                if (!IsPopupDisplayed())
                {
                    GUICamera cameraScript = guiCamera.GetComponent<GUICamera>();
                    if (cameraScript.zoomLevel < -0.5f)
                    {
                        cameraScript.zoomLevel += 0.5f;
                    }
                }
            });

            GestureHelper.onSwipeRight += rightHandler;
        }
        InitializeItems();
    }
	
	public bool IsPopupDisplayed() {
		HexInfoManager info = GameObject.FindObjectOfType(typeof(HexInfoManager)) as HexInfoManager;
		if(info != null) {
			if(info.IsInOpenStage()) {
				info.AnimExit();
				return true;
			}
		}
		return false;
	}	

	void OnDisable() 
	{
		GestureHelper.onSwipeLeft -= leftHandler;
		GestureHelper.onSwipeRight -= rightHandler;
	}
	
	void OnDestroy()
	{
        GestureHelper.onSwipeLeft -= leftHandler;
        GestureHelper.onSwipeRight -= rightHandler;
	}

    /// <summary>    
    /// function which converts from 
    /// </summary>
    /// <param name="height">height is a result of the pitch calculation so it should be between 0 and 360</param>
    /// <returns>physical height used by camera to be lifted by</returns>
    private float HeightToPositionValue(float height)
    {
        while (height < 0)
        {
            height += 360.0f;
        }
        height %= 360;
        if (height > 90 && height <= 270) { height = 180 - height; }
        else if (height > 270) { height = height - 360; }


        return height * 0.1f;
    }

    /// <summary>
    /// getter for panel button data
    /// </summary>
    /// <returns>returns list of hex buttons data containing all settings for button creation </returns>
    public List<HexButtonData> GetButtonData()
    {
        if (parent == null) return null;

        return parent.buttonData;
    }

    /// <summary>
    /// finds base all other buttons are initialized from
    /// </summary>
    /// <returns></returns>
    public Transform GetButtonBase()
    {
        return buttonBase.transform;// transform.GetChild(0);
    }

    /// <summary>
    /// finds button on transform child list
    /// </summary>
    /// <param name="index">zero based index</param>
    /// <returns></returns>
    public Transform GetButton(int index)
    {
        return transform.GetChild(index + 1);
    }

    /// <summary>
    /// sets parent panel for this component
    /// </summary>
    /// <param name="_parent">hex panel pointer</param>
    /// <returns></returns>
    public void SetParent(HexPanel _parent)
    {
        parent = _parent;
    }

    /// <summary>
    /// standard unity update function called once per frame
    /// </summary>
    /// <returns></returns>
    void Update()
    {

        //if (!initialized) return;

//		if(Input.GetKeyDown(KeyCode.Escape)) {
//			GoBack();
//		}
		
        buttonNextEnterDelay -= Time.deltaTime;
		if(buttons == null) {
			UnityEngine.Debug.Log("DynamicHexList: buttons is null");
            return;
		}
        if (buttonNextEnterDelay <= 0 && buttons.Count > buttonNextEnterIndex)
        {
            int ring = (int)(buttonNextEnterIndex / 8);

            for (int i = 0; i <= ring && buttons.Count > buttonNextEnterIndex; i++)
            {
                PlayButtonEnter(buttons[buttonNextEnterIndex], true);
                buttonNextEnterIndex++;
            }

            buttonNextEnterDelay = buttonEnterDelay;
        }

        //		if(Input.touchCount == 1) 
        //		{
        //			if(Input.GetTouch(0).phase == TouchPhase.Began) {
        //				ResetGyro();
        //			}
        //		}
        
        //if button enter delay is below 0 at this stage then screen has finished loading
        if (parent.state == FlowStateBase.State.Idle && guiCamera != null)
        {

            Vector3 forward = guiCamera.transform.forward;

            RaycastHit[] hits = Physics.RaycastAll(guiCamera.transform.position, forward, 5.0f);// ,LayerMask.NameToLayer(HexPanel.CAMERA_3D_LAYER));

            bool selectionStillActive = false;
            UIImageButton newSelection = null;

            if (hits.Length > 0)
            {
                foreach (RaycastHit hit in hits)
                {
                    UIImageButton button = hit.collider.GetComponent<UIImageButton>();
                    if (button == null || GetButtonBase() == button.transform.parent) continue;

                    
                    newSelection = button;
                    if (newSelection == selection)
                    {
                        selectionStillActive = true;
                        break;
                    }
                }
            }
            else
            {
                newSelection = FindNearest();
                if (newSelection == null || newSelection == selection)
                {
                    selectionStillActive = true;                    
                }
            }
            
            if (!selectionStillActive && newSelection != null)
            {
                UIButtonAnimationLocker lockScript = newSelection.GetComponent<UIButtonAnimationLocker>();
                FlowButton newFb = newSelection.GetComponent<FlowButton>();

                HexButtonData newHbd = newFb.userData.ContainsKey("HexButtonData") ? newFb.userData["HexButtonData"] as HexButtonData : null ;

                if (    ( newHbd != null && newHbd.allowEarlyHover) || 
                        lockScript == null || 
                        !lockScript.locked)
                {
                    if (selection != null)
                    {                        
                        //selection.SendMessage("OnHover", false, SendMessageOptions.DontRequireReceiver);
                        TweenPosition tp = guiCamera.GetComponent<TweenPosition>();
                        if (tp != null)
                        {
                            TweenPosition.Begin(tp.gameObject, 0.3f, guiCamera.transform.position);
                        }
                        
                        FlowButton fb = selection.GetComponent<FlowButton>();
                        if (parent != null)
                        {                            
                            if (fb != null)
                            {
                                parent.OnHover(fb, false);
                            }
                        }

                        if (lockScript != null && !lockScript.locked)
                        {
                            if (fb != null)
                            {
                                HexButtonData hbd = fb.userData.ContainsKey("HexButtonData") ? fb.userData["HexButtonData"] as HexButtonData : null;
                                HoverButtonAnim(buttonsImageComponents.IndexOf(selection), hbd, false);
                            }
                        }
                        //make sure that no data artifacts are left over                        
                        /*
                         * UIButtonAnimationLocker lockSelectionScript = selection.GetComponent<UIButtonAnimationLocker>();
                        if (lockSelectionScript == null)
                        {
                            lockSelectionScript.locked = false;
                        }*/
                        
                    }
                    selection = newSelection;
                    //newSelection.SendMessage("OnHover", true, SendMessageOptions.DontRequireReceiver);
                    HexMarkerLogic.SetTarget(newSelection.transform.parent.localPosition);                    

                    if (parent != null)
                    {
                        FlowButton fb = newSelection.GetComponent<FlowButton>();
                        if (fb != null)
                        {
                            parent.OnHover(fb, true);
                        }
                        
                    }

                    if (newHbd.locked)
                    {
                        UIButtonAnimationLocker newLockScript = newSelection.GetComponent<UIButtonAnimationLocker>();
                        if (newLockScript != null && !newLockScript.locked)
                        {
                            HoverButtonAnim(buttonsImageComponents.IndexOf(newSelection), newHbd, true);                            
                        }
                    }

                    HexInfoManager info = GameObject.FindObjectOfType(typeof(HexInfoManager)) as HexInfoManager;
                    if (info != null)
                    {
                        info.AnimExit();
                    }
                    //selection changed we want to stop dragging, user need to start drag from the start
                    dragging = false;
                }
            }
           
            if (Platform.Instance.OnGlass())
            {
                bool buttonClick = Input.GetMouseButton(0);

                if (selection != null && (Input.touchCount == 1 || buttonClick))
                {				
                    Touch touch = new Touch();
                    bool found = false;

                    if (!buttonClick)
                    {					
                        touch = Input.touches[0];
                        if (dragging == false)
                        {						
                            dragging = true;
                            draggingFingerID = touch.fingerId;
                            draggingStartPos = touch.position;
                            found = true;
                        }
                        else
                        {
                            foreach (Touch t in Input.touches)
                            {
                                if (t.fingerId == draggingFingerID)
                                {								
                                    found = true;
                                    touch = t;
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {					
                        if (dragging == false)
                        {						
                            draggingStartPos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                            dragging = true;
                        }

                        found = true;
                    }

                    if (found)
                    {					
                        Vector2 offset;
                        if (!buttonClick)
                        {						
                            offset = touch.position - draggingStartPos;
                        }
                        else
                        {						
                            Vector2 mousePos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                            offset = mousePos - draggingStartPos;
                        }
										
                        float height = Screen.currentResolution.height;
                        float scale = -offset.y / height;

                        scale = Mathf.Clamp(scale, -1, 1);
										
                        TweenPosition tp = guiCamera.GetComponent<TweenPosition>();
                        if (tp == null)
                        {
                            tp = guiCamera.gameObject.AddComponent<TweenPosition>();
                        }					
                        Vector3 cameraCoreAxis = guiCamera.transform.position;
                        cameraCoreAxis.y = selection.transform.position.y;
                        Vector3 direction = selection.transform.position - cameraCoreAxis;
										
                        Vector3 pos = cameraCoreAxis + (direction * scale);

                        TweenPosition.Begin(guiCamera.gameObject, 0.6f, pos);

                        if (parent != null && scale > 0.6f)
                        {						
                            FlowButton fb = selection.gameObject.GetComponent<FlowButton>();
                            if (fb != null)
                            {
                                parent.OnClick(fb);
                            }
                        }
                        else if (parent != null && scale < -0.6f)
                        {
                            parent.OnBack();

                        }
                    }
                }
            }
            else
            {
				
                dragging = false;

                if (selection != null)
                {
                    TweenPosition tp = guiCamera.GetComponent<TweenPosition>();
                    if (tp != null)
                    {
                        Vector3 pos = guiCamera.transform.position;

                        TweenPosition.Begin(tp.gameObject, 0.3f, pos);
                    }
                }
            }            
        }
    }   
	
	/// <summary>
	/// Enters the currently selected game.
	/// </summary>
	public void EnterGame()
	{
		FlowButton fb = selection.gameObject.GetComponent<FlowButton>();
        if (fb != null)
        {
			parent.OnClick(fb);
			//guiCamera.transform.position =  new Vector3(0, 0, -1.5f);
        }
	}

    public UIImageButton FindNearest()
    {
        Vector3 camPos = guiCamera.transform.position;
        Vector2 flatCameraPosition = new Vector2(camPos.x, camPos.y);
        int closest = -1;
        float distance = float.MaxValue; 
        for(int i=0; i<hexPosition2d.Count; i++)
        {
            Vector2 localDist = hexPosition2d[i] - flatCameraPosition;
            float localDistSQMag = localDist.sqrMagnitude;
            if (distance > localDistSQMag)
            {
                distance = localDistSQMag;
                closest = i;
            }
        }

        return closest > -1 ? buttonsImageComponents[closest] : null;
    }

	/// <summary>
	/// Sends message to parent asking it to previous history flow state
	/// </summary>
	/// <returns></returns>
	public void GoBack()
	{
		parent.OnBack();
	}
	
    /// <summary>
    /// Cleans up elements from the screen preparing for recreation
    /// </summary>
    /// <param name="elementsToKeep"> number of the elements which would not be destroyed during cleanup. Its for performance only as all elements should be reconfiguret later anyway</param>
    /// <returns>Null</returns>
    void CleanupChildren(int elementsToKeep)
    {
        if (transform.childCount < 1)
        {
            Debug.LogError("List doesn't have at least one button element to clone later");
            return;
        }

        buttons = new List<GameObject>();
        buttonsImageComponents = new List<UIImageButton>();
        hexPosition2d = new List<Vector2>();
        buttonSprites = new List<Dictionary<string, UISprite>>();
        overlays = new List<UILabel>();
        buttonsReady = false;

        if (elementsToKeep < 1) elementsToKeep = 1;

        while (transform.childCount > elementsToKeep)
        {
            GameObject.Destroy(transform.GetChild(transform.childCount - 1));
        }

        Animation anim = GetButtonBase().gameObject.GetComponentInChildren<Animation>();
        if (anim != null)
        {
            anim.gameObject.transform.localScale = new Vector3(0.001f, 0.001f, 1);
        }
    }

    /// <summary>
    /// Unified section building and updating hex visual data in dynamic list based on button data from panel
    /// </summary>
    /// <returns></returns>
    private void ButtonDataToVisualProcess()
    {
        Transform child = GetButtonBase();
        float Z = child.transform.position.z;                

        for (int i = 0; i < GetButtonData().Count; i++)
        {
            HexButtonData data = GetButtonData()[i];

            if (data.markedForVisualRefresh == false)
            {
                continue;
            }
			
            GameObject tile = null;
            if (i >= buttons.Count)
            {
                tile = (GameObject)GameObject.Instantiate(child.gameObject);
				//tile.GetComponent<MeshRenderer>().isPartOfStaticBatch = true;
                tile.transform.parent = child.parent;
                tile.transform.rotation = child.rotation;
                tile.transform.localScale = child.localScale;
                Vector3 pos = GetLocation(data.column, data.row);

                Vector3 hexPosition = new Vector3(pos.x, pos.y, pos.z);
                tile.transform.position = hexPosition;                
            }
            else
            {
                tile = GetButton(i).gameObject;
            }

            tile.name = data.buttonName;

            FlowButton fb = tile.GetComponentInChildren<FlowButton>();
            if (fb != null)
            {
                fb.owner = parent;
                fb.name = data.buttonName;
                UIImageButton graphics = fb.GetComponentInChildren<UIImageButton>();

				graphics.pressedSprite = data.imageName;
                graphics.hoverSprite = graphics.pressedSprite;
                graphics.normalSprite = graphics.pressedSprite;
                graphics.disabledSprite = graphics.pressedSprite;

                fb.userData["HexButtonData"] = data;
            }
			
			if(font != null) {
				font.RequestCharactersInTexture("qwertyuiopasdfghklzxcvbnm,./;'[]098754321QWERTYUIOPASDFGHJKLZXCVBNM<>?|:@~{}+_)(*&^%$£!¬`");
			}
			
            UILabel[] labels = tile.GetComponentsInChildren<UILabel>() as UILabel[];
            if (labels != null)
            {
                foreach (UILabel label in labels)
                {
                    switch (label.gameObject.name)
                    {
                        case "Counter":
                            label.text = data.count < 0 ? "" : "" + data.count;
                            break;
                        case "TextContent":
                            label.text = data.textNormal;
                            break;
                        case "BoldText":
                            label.text = data.textBold;
						    break;
                        case "SmallText":
                            label.text = data.textSmall;
                            break;
                        case "OverlayText":
                            label.text = data.textOverlay;
                            overlays.Add(label);
                            break;
                    }
                }
            }

            if (i >= buttons.Count)
            {
                AddTileToLists(tile, data);
            }

            if (buttonSprites.Count > i )
            {
                if (buttonSprites[i].ContainsKey(GRAPHIC_PLUS))
                {
                    buttonSprites[i][GRAPHIC_PLUS].gameObject.SetActive(false);
                }

                if (buttonSprites[i].ContainsKey(GRAPHIC_FOREGROUND))
                {
                    buttonSprites[i][GRAPHIC_FOREGROUND].gameObject.SetActive(data.locked);
                }
                if (buttonSprites[i].ContainsKey(GRAPHIC_PADLOCK))
                {
                    uint color = data.backgroundTileColor;
                    buttonSprites[i][GRAPHIC_PADLOCK].gameObject.SetActive(data.locked);
                }
                if (buttonSprites[i].ContainsKey(GRAPHIC_BACKGROUND))
                {                
                    uint color = data.backgroundTileColor;
                    buttonSprites[i][GRAPHIC_BACKGROUND].color = new Color((float)((color >> 24) & 0xFF) / 256.0f,
                                                                     (float)((color >> 16) & 0xFF) / 256.0f,
                                                                     (float)((color >> 8) & 0xFF) / 256.0f);
                }
                
            }
            data.markedForVisualRefresh = false;
        }
		StaticBatchingUtility.Combine(buttons.ToArray(), child.gameObject);
	
    }

    /// <summary>
    /// IF button list changed you might need to update visual data collections to match it. This is function which finds differences and adds missing buttons
    /// </summary>
    /// <returns></returns>
    public void UpdateButtonList()
    {
        if (buttons == null || buttons.Count == 0)
        {
            InitializeItems();
            return;
        }

        ButtonDataToVisualProcess();
    }

    /// <summary>
    /// Builds screen buttons based on first prefab button it contains
    /// </summary>
    /// <returns></returns>
    public void InitializeItems()
    {
        //too early for initialization
        if (buttonBase == null) return;

        if (parent == null)
        {
            Debug.LogError("Data have not been set properly before this call!");
        }

        if (transform.childCount < 1)
        {
            Debug.LogError("List doesn't have at least one button element to clone");
            return;
        }

        initialized = true;        

        int count = GetButtonData().Count;
        CleanupChildren(count);
        buttonEnterDelay = 0.07f;

        Transform child = GetButtonBase();
        child.gameObject.SetActive(true);

        UIImageButton ib = child.gameObject.GetComponentInChildren<UIImageButton>();
        if (parent != null && ib != null)
        {
            FlowButton fb = ib.GetComponent<FlowButton>();
            if (fb == null)
            {
                fb = ib.gameObject.AddComponent<FlowButton>();
            }
            fb.owner = parent;
        }

        ButtonDataToVisualProcess();

      //  float Z = child.transform.position.z;
       /* for (int i = 0; i < count; i++)
        {
            HexButtonData data = GetButtonData()[i];
            GameObject tile = null;
            if (i+1 >= transform.childCount)
            {
                tile = (GameObject)GameObject.Instantiate(child.gameObject);
                tile.transform.parent = child.parent;
                tile.transform.rotation = child.rotation;
                tile.transform.localScale = child.localScale;
            }
            else
            {
                tile = GetButton(i).gameObject;
            }
            Vector3 pos = GetLocation(data.column, data.row);    

            Vector3 hexPosition = new Vector3(pos.x, pos.y, pos.z);
            tile.transform.position = hexPosition;
            tile.name = data.buttonName;

            FlowButton fb = tile.GetComponentInChildren<FlowButton>();
            if (fb != null)
            {
                fb.owner = parent;
                fb.name = data.buttonName;
                UIImageButton graphics = fb.GetComponentInChildren<UIImageButton>();
                               
                
                graphics.pressedSprite = data.imageName;
                graphics.hoverSprite = graphics.pressedSprite;
                graphics.normalSprite = graphics.pressedSprite;
                graphics.disabledSprite = graphics.pressedSprite;

                fb.userData["HexButtonData"] = data;
            }

            UILabel[] labels = tile.GetComponentsInChildren<UILabel>() as UILabel[];
            if (labels != null)
            {
                foreach(UILabel label in labels)
                {
                    switch (label.gameObject.name)
                    {
                        case "Counter":
                            label.text = data.count < 0 ? "" : ""+data.count;
                            break;
                        case "TextContent":
                            label.text = data.onButtonCustomString;
                            break;
                    }                    
                }
            }            
            
            AddTileToLists(tile, data);

            if (buttonSprites.Count > i && buttonSprites[i].ContainsKey("Plus") )
            {
                buttonSprites[i]["Plus"].gameObject.SetActive(data.displayPlusMarker);
            }

            if (buttonSprites.Count > i && buttonSprites[i].ContainsKey("Foreground"))
            {
                buttonSprites[i]["Foreground"].gameObject.SetActive(data.locked);
            }

            data.markedForVisualRefresh = false;
        }*/

      /*  foreach (GameObject go in buttons)
        {
            go.SetActive(false);
        }*/
    }

    /// <summary>
    /// Get location in 2d space for the element based on its column and row position
    /// </summary>
    /// <param name="column">horizontal position (might be negative)</param>
    /// <param name="row">vertical position (might be negative)</param>
    /// <returns>flat 2d position for hex </returns>
    public static Vector2 GetLocation(int column, int row)
    {
        int Xoffset = -(Mathf.Abs(row) % 2);
        return new Vector2(-hexLayoutOffset.x * (Xoffset + -column * 2), hexLayoutOffset.y * -row);
    }

    /// <summary>
    /// Plays animation to enter button
    /// </summary>
    /// <param name="buttonRoot">Game Object which is root element in button structure.</param>
    /// <param name="forward">should animation go forward or backward</param>
    /// <returns></returns>
    public void PlayButtonEnter(GameObject buttonRoot, bool forward)
    {
       // buttonRoot.SetActive(true);
        UIImageButton bi = buttonRoot.GetComponentInChildren<UIImageButton>();

        Animation target = buttonRoot.GetComponentInChildren<Animation>();
        if (target != null)
        {
            AnimationOrTween.Direction dir = forward ? AnimationOrTween.Direction.Forward : AnimationOrTween.Direction.Reverse;
            ActiveAnimation anim = ActiveAnimation.Play(target, btEnterAnimation, dir);

            if (anim != null)
            {                
                anim.Reset();

                UIButtonAnimationLocker buttonLocker = buttonRoot.GetComponentInChildren<UIButtonAnimationLocker>();
                if (buttonLocker != null)
                {                    
                    EventDelegate.Add(anim.onFinished, buttonLocker.OnButtonAnimFinished, true);
                    buttonLocker.OnButtonAnimStarted();
                }
            }
        }
    }

    /// <summary>
    /// standard unity function which is called when object gets destroyed. Used for cleaning up
    /// </summary>
    /// <returns></returns>
    public void OnExit()
    {
        if (guiCamera != null)
        {
            TweenPosition tp = guiCamera.GetComponent<TweenPosition>();
            if (tp != null)
            {
                GameObject.Destroy(tp);
            }
        }
		
		GestureHelper.onTap -= tapHandler;
		GestureHelper.onBack -= backHandler;
    }


    /// <summary>
    /// Function which fills all required lists with tile data
    /// </summary>
    /// <param name="tile"></param>
    /// <returns></returns>
    public void AddTileToLists(GameObject tile, HexButtonData data)
    {
        buttons.Add(tile);
        buttonsImageComponents.Add(tile.GetComponentInChildren<UIImageButton>());
        Vector3 tilePos = tile.transform.position;
        hexPosition2d.Add(new Vector2(tilePos.x, tilePos.y));     
        
        UISprite[] sprites = tile.GetComponentsInChildren<UISprite>() as UISprite[];
        Dictionary<string, UISprite> singleButtonSprites = new Dictionary<string,UISprite>();
        if (sprites != null)
        {
            foreach (UISprite sprite in sprites)
            {
                singleButtonSprites[sprite.gameObject.name] = sprite;
            }
        }

        buttonSprites.Add(singleButtonSprites);
    }

    /// <summary>
    /// Finds hexagonal distance from center
    /// </summary>
    /// <param name="column">column of the target hex</param>
    /// <param name="row">row of the target hex</param>
    /// <returns>hexagonal path (distance in hexes) fro 0,0 to target hex</returns>
    private int Distance(int column, int row )
    {
        int rowDist = Mathf.Abs(row);
        int Xoffset = rowDist % 2;

        if (column > -1 && Xoffset == 1)
        {
            column += 1;
        }
        int columnDist = Mathf.Abs(column);

        //rowdist /2 can be only bigger or rounded up equal to columnDist
        //if this is not true we will get non diagonal moves
        int nonDIagonalMoves = columnDist - (int)(rowDist * 0.5f + 0.6f) ;

        return nonDIagonalMoves > 0 ? nonDIagonalMoves + rowDist : rowDist;
        
    }

    private void HoverButtonAnim(int index, HexButtonData hbd, bool hoverEnter)
    {
        if ( hbd.locked == false) return;



        UIButtonAnimationLocker lockScript = buttons[index].GetComponent<UIButtonAnimationLocker>();
        //Transform padlock = button.transform.Find("Padlock");
        //Transform plus = button.transform.Find("Plus");
        UISprite sprite;
        TweenAlpha ta;
        
        if (hoverEnter == true)
        {
            if (buttonSprites[index].ContainsKey(GRAPHIC_PADLOCK))
            {
                sprite = buttonSprites[index][GRAPHIC_PADLOCK];
                sprite.gameObject.SetActive(true);
                ta = TweenAlpha.Begin(sprite.gameObject, 0.15f, 0.0f);
            }

            if (buttonSprites[index].ContainsKey(GRAPHIC_PLUS))
            {
                sprite = buttonSprites[index][GRAPHIC_PLUS];
                sprite.gameObject.SetActive(true);
                sprite.alpha = 1.0f;
                ta = TweenAlpha.Begin(sprite.gameObject, 0.25f, 0.0f);
                ta.delay = 2.0f;
            }

            if (buttonSprites[index].ContainsKey(GRAPHIC_FOREGROUND))
            {
                sprite = buttonSprites[index][GRAPHIC_FOREGROUND];
                sprite.gameObject.SetActive(true);
                sprite.alpha = 0.6f;
                ta = TweenAlpha.Begin(sprite.gameObject, 0.25f, 0.0f);
                ta.delay = 2.0f;
            }

            if ( overlays.Count > index )
            {                                
                ta = TweenAlpha.Begin(overlays[index].gameObject, 0.25f, 0.0f);
                ta.delay = 2.0f;
            }
        }
        else
        {
            if (buttonSprites[index].ContainsKey(GRAPHIC_PADLOCK))
            {
                sprite = buttonSprites[index][GRAPHIC_PADLOCK];                
                ta = TweenAlpha.Begin(sprite.gameObject, 0.15f, 1.0f);
            }

            if (buttonSprites[index].ContainsKey(GRAPHIC_PLUS))
            {
                sprite = buttonSprites[index][GRAPHIC_PLUS];                
                ta = TweenAlpha.Begin(sprite.gameObject, 0.25f, 0.0f);
                ta.delay = 0.0f;
            }

            if (buttonSprites[index].ContainsKey(GRAPHIC_FOREGROUND))
            {
                sprite = buttonSprites[index][GRAPHIC_FOREGROUND];                                
                ta = TweenAlpha.Begin(sprite.gameObject, 0.25f, 0.6f);
                ta.delay = 0.0f;
            }

            if (overlays.Count > index)
            {
                ta = TweenAlpha.Begin(overlays[index].gameObject, 0.25f, 1.0f);
                ta.delay = 0.0f;
            }
        }        
    }
}
