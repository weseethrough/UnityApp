using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Component which provides management functionality for dynamic hex screens
/// </summary>
public class DynamicHexList : MonoBehaviour
{
    const float CAMERA_SENSITIVITY_X = 4.5f;
    const float CAMERA_SENSITIVITY_Y = 5.5f;
	
	public Font font;
	
    HexPanel parent = null;

    UICamera guiCamera;
    public static Vector2 hexLayoutOffset = new Vector2(0.25f, 0.4330127f);
    Vector3 distanceVector;
    Vector3 cameraPosition;
    float radius;

    //default height and rotation is used as a default offset when reseting sensors
    Quaternion cameraDefaultRotation;
    Vector2 cameraMoveOffset;

    //float screenEnterTime = 8.0f;
    float buttonEnterDelay = 0.0f;
    float buttonNextEnterDelay = 0.0f;
    int buttonNextEnterIndex = 0;
	
	private float zoomLevel = -1.0f;

    int buttonCount = 0;

    List<GameObject> buttons;
    List<UIImageButton> buttonsImageComponents;
    List<Vector2> hexPosition2d;
    List<Dictionary<string, UISprite>> buttonSprites;

    UIImageButton selection;
    private string btEnterAnimation = "HexEnter";

    bool buttonsReady = false;

    Vector2 draggingStartPos = Vector2.zero;
    bool dragging = false;
    int draggingFingerID = -1;

    GameObject buttonBase;
	
	private GestureHelper.OnTap tapHandler = null;
	
	private GestureHelper.OnSwipeLeft leftHandler = null;
	
	private GestureHelper.OnSwipeRight rightHandler = null;
	
    bool initialized = false;
	
	private GestureHelper.DownSwipe downHandler = null;


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

        if (guiCamera != null)
        {
            cameraPosition = guiCamera.transform.position;
            distanceVector = transform.position - cameraPosition;
            distanceVector.y = 0;
            cameraDefaultRotation = guiCamera.transform.rotation;

            radius = distanceVector.magnitude;

#if !UNITY_EDITOR
            Platform.Instance.ResetGyro();
            cameraDefaultRotation = ConvertOrientation(Platform.Instance.GetPlayerOrientation(), out cameraMoveOffset);

            //Quaternion newOffset = Quaternion.Inverse(cameraDefaultRotation) * cameraDefaultRotation;
            //guiCamera.transform.rotation = newOffset;
#endif         
        }
		
		tapHandler = new GestureHelper.OnTap(() => {
			EnterGame();
		});
		
		GestureHelper.onTap += tapHandler;
		
		downHandler = new GestureHelper.DownSwipe(() => {
			GoBack();
		});
		
		GestureHelper.onSwipeDown += downHandler;

		leftHandler = new GestureHelper.OnSwipeLeft(() => {
			if(!IsPopupDisplayed()) {
				if(zoomLevel > -1.5f) {
					zoomLevel -= 0.5f;
				}
			}
		});
		
		GestureHelper.swipeLeft += leftHandler;
		
		rightHandler = new GestureHelper.OnSwipeRight(() => {
			if(!IsPopupDisplayed()) {
				if(zoomLevel < -0.5f) {
					zoomLevel += 0.5f;
				}
			}
		});
		
		GestureHelper.swipeRight += rightHandler;
		
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
	
    /// <summary>
    /// Resets gyro offset against the screen, visually it setts screen to the "zero" position
    /// </summary>
    /// <returns></returns>
    public void ResetGyro()
    {
#if !UNITY_EDITOR 
        Platform.Instance.ResetGyro();
        cameraDefaultRotation = ConvertOrientation(Platform.Instance.GetPlayerOrientation(), out cameraMoveOffset);
#endif
    }

    /// <summary>
    /// function which converts orientation quaternion into pitch and yaw, suitable for moving cam up/down, left/right on 2D menu
    /// </summary>
    /// <param name="q">input orientation</param>
    /// <param name="dynamicCamPos">(yaw, pitch)</param>
    /// <return>the input quaternion</return>
    private Quaternion ConvertOrientation(PlayerOrientation p, out Vector2 dynamicCamPos)
    {
        
        dynamicCamPos = new Vector2(p.AsCumulativeYaw(), -p.AsPitch());
        dynamicCamPos.x *= CAMERA_SENSITIVITY_X;
	    dynamicCamPos.y *= CAMERA_SENSITIVITY_Y;
		
        //UnityEngine.Debug.Log("Yaw:" + -p.AsYaw() + ", x-offset:" + dynamicCamPos.x);
		Vector3 e = p.AsQuaternion().eulerAngles;
		UnityEngine.Debug.Log("Yaw:" + -p.AsYaw() + ", Pitch:" + -p.AsPitch() + ", Roll:" + -p.AsRoll() + ", x:" + e.x + ", y:" + e.y + ", z:" + e.z);
        //dynamicCamPos *= 0.02f;
        //return Quaternion.EulerRotation(q.eulerAngles.x, 0, q.eulerAngles.z);
        return p.AsQuaternion();
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

            PlayButtonEnter(buttons[buttonNextEnterIndex], true);

           /* UISprite[] sprites = buttons[buttonNextEnterIndex].GetComponentsInChildren<UISprite>() as UISprite[];
            foreach (UISprite spr in sprites)
            {
                if (spr.name == "Foreground")
                {
                    spr.gameObject.SetActive(GetButtonData()[buttonNextEnterIndex].locked);                    
                    break;
                }
            }*/

            buttonNextEnterIndex++;             
            
            buttonNextEnterDelay = buttonEnterDelay;
        }

        //		if(Input.touchCount == 1) 
        //		{
        //			if(Input.GetTouch(0).phase == TouchPhase.Began) {
        //				ResetGyro();
        //			}
        //		}
        
        //if button enter delay is below 0 at this stage then screen has finished loading
        if (parent.state == FlowState.State.Idle && guiCamera != null)
        {
            /*Quaternion rot = SensorHelper.rotation;
            if (!float.IsNaN(rot.x) && !float.IsNaN(rot.y) && !float.IsNaN(rot.z) && !float.IsNaN(rot.w))
            {
                Quaternion newOffset = Quaternion.Inverse(cameraStartingRotation) * rot;*/
#if !UNITY_EDITOR
                float pitchHeight;
				Vector2 newCameraOffset;
				ConvertOrientation(Platform.Instance.GetPlayerOrientation(), out newCameraOffset);
				Vector3 camPos = guiCamera.transform.position;
				newCameraOffset -= cameraMoveOffset;
                guiCamera.transform.position = new Vector3(newCameraOffset.x, newCameraOffset.y, zoomLevel);
#endif
            /*}
            else
            {
                Debug.LogError("Sensor data invalid");
            }*/
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
                            TweenPosition.Begin(tp.gameObject, 0.3f, cameraPosition);
                        }

                        if (parent != null)
                        {
                            FlowButton fb = selection.GetComponent<FlowButton>();
                            if (fb != null)
                            {
                                parent.OnHover(fb, false);
                            }

                            HexButtonData hbd = (fb.userData["HexButtonData"] as HexButtonData);
                            if (hbd.displayPlusMarker == true)
                            {
                                UISprite[] sprites = selection.GetComponentsInChildren<UISprite>() as UISprite[];
                                if (sprites != null)
                                {
                                    foreach (UISprite sprite in sprites)
                                    {
                                        switch (sprite.gameObject.name)
                                        {
                                            case "Plus":
                                                sprite.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                                                break;
                                        }
                                    }
                                }
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

                        HexButtonData hbd = (fb.userData["HexButtonData"] as HexButtonData);
                        if (hbd.displayPlusMarker == true)
                        {
                            UISprite[] sprites = selection.GetComponentsInChildren<UISprite>() as UISprite[];
                            if (sprites != null)
                            {
                                foreach (UISprite sprite in sprites)
                                {
                                    switch (sprite.gameObject.name)
                                    {
                                        case "Plus":
                                            sprite.transform.localScale = new Vector3(3.0f, 3.0f, 1.0f);
                                            break;
                                    }
                                }
                            }
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
                    Vector3 cameraCoreAxis = cameraPosition;
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
            else
            {
				
                dragging = false;

                if (selection != null)
                {
                    TweenPosition tp = guiCamera.GetComponent<TweenPosition>();
                    if (tp != null)
                    {
                        Vector3 pos = cameraPosition;

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
                            break;
                    }
                }
            }

            if (i >= buttons.Count)
            {
                AddTileToLists(tile, data);
            }

            if (buttonSprites.Count > i && buttonSprites[i].ContainsKey("Plus"))
            {
                buttonSprites[i]["Plus"].gameObject.SetActive(data.displayPlusMarker);
            }

            if (buttonSprites.Count > i && buttonSprites[i].ContainsKey("Foreground"))
            {
                buttonSprites[i]["Foreground"].gameObject.SetActive(data.locked);
            }

            if (buttonSprites.Count > i && buttonSprites[i].ContainsKey("Background"))
            {                
                uint color = data.backgroundTileColor;
                buttonSprites[i]["Background"].color = new Color((float)((color >> 24) & 0xFF) / 256.0f,
                                                                 (float)((color >> 16) & 0xFF) / 256.0f,
                                                                 (float)((color >> 8) & 0xFF) / 256.0f);
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
        if (buttons.Count == 0)
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
        if (parent == null || radius == 0.0f)
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
                guiCamera.transform.position = cameraPosition;
            }
        }
		
		GestureHelper.onTap -= tapHandler;
		GestureHelper.onSwipeDown -= downHandler;
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
}
