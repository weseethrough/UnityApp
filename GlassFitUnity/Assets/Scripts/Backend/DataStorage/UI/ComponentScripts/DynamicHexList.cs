using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Component which provides management functionality for dynamic hex screens
/// </summary>
public class DynamicHexList : MonoBehaviour
{
    const float CAMERA_SENSITIVITY = 3.0f;
	
    HexPanel parent = null;

    UICamera guiCamera;
    Vector2 hexLayoutOffset = new Vector2(0.4330127f, 0.25f);
    Vector3 distanceVector;
    Vector3 cameraPosition;
    float radius;

    //default height and rotation is used as a default offset when reseting sensors
    Quaternion cameraDefaultRotation;
    Vector2 cameraMoveOffset;

    float screenEnterTime = 0.8f;
    float buttonEnterDelay = 0.0f;
    float buttonNextEnterDelay = 0.0f;
    int buttonNextEnterIndex = 0;

    int buttonCount = 0;

    List<GameObject> buttons;
    UIImageButton selection;
    private string btEnterAnimation = "HexEnter";

    bool buttonsReady = false;

    Vector2 draggingStartPos = Vector2.zero;
    bool dragging = false;
    int draggingFingerID = -1;
	
	private GestureHelper.OnTap tapHandler = null;
	
	private GestureHelper.OnSwipeLeft leftHandler = null;
    bool initialized = false;


    /// <summary>
    /// List initialization process
    /// </summary>
    /// <returns></returns>
    void Start()
    {

        initialized = false;

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

        //        if (!SensorHelper.gotFirstValue)
        //        {
        //            SensorHelper.ActivateRotation();
        //        }

        if (guiCamera != null)
        {
            cameraPosition = guiCamera.transform.position;
            distanceVector = transform.position - cameraPosition;
            distanceVector.y = 0;
            cameraDefaultRotation = guiCamera.transform.rotation;

            radius = distanceVector.magnitude;

            /* Quaternion rot = SensorHelper.rotation;
             if (!float.IsNaN(rot.x) && !float.IsNaN(rot.y) && !float.IsNaN(rot.z) && !float.IsNaN(rot.w))
             {
                // cameraStartingRotation = rot * Quaternion.Inverse(guiCamera.transform.rotation);

                 Quaternion newOffset = Quaternion.Inverse(cameraStartingRotation) * rot;*/
#if !UNITY_EDITOR
            Platform.Instance.ResetGyro();
            cameraDefaultRotation = ConvertOrientation(Platform.Instance.GetOrientation(), out cameraMoveOffset);

            //Quaternion newOffset = Quaternion.Inverse(cameraDefaultRotation) * cameraDefaultRotation;
            //guiCamera.transform.rotation = newOffset;
#endif
            /* }
            else
            {
                Debug.LogError("Sensor data invalid");
            }*/

        }
		
		tapHandler = new GestureHelper.OnTap(() => {
			EnterGame();
		});
		
		GestureHelper.onTap += tapHandler;
		
		leftHandler = new GestureHelper.OnSwipeLeft(() => {
			GoBack();
		});
		
		GestureHelper.swipeLeft += leftHandler;

        InitializeItems();
    }

    /// <summary>
    /// Resets gyro offset against the screen, visually it setts screen to the "zero" position
    /// </summary>
    /// <returns></returns>
    public void ResetGyro()
    {
#if !UNITY_EDITOR 
        Platform.Instance.ResetGyro();
        cameraDefaultRotation = ConvertOrientation(Platform.Instance.GetOrientation(), out cameraMoveOffset);
#endif
    }

    /// <summary>
    /// function which converts orienation quaternion into pitch and yaw, suitable for moving cam up/down, left/right on 2D menu
    /// </summary>
    /// <param name="q">input orientation</param>
    /// <param name="dynamicCamPos">(yaw, pitch)</param>
    /// <return>the input quaternion</return>
    private Quaternion ConvertOrientation(Quaternion q, out Vector2 dynamicCamPos)
    {
        float pitch = Mathf.Atan2(2*(q.w*q.x + q.y*q.z), 1-2*(q.x*q.x + q.y*q.y));
        float roll = Mathf.Asin(2*(q.w*q.y - q.z*q.x));
        float yaw = Mathf.Atan2(2*(q.w*q.z + q.x*q.y), 1-2*(q.y*q.y + q.z*q.z));
		
        dynamicCamPos = new Vector2(-yaw, -pitch);
        dynamicCamPos *= CAMERA_SENSITIVITY;
        UnityEngine.Debug.Log("MenuPosition:" + yaw + ", " + pitch + ", " + roll);
        //dynamicCamPos *= 0.02f;
        //return Quaternion.EulerRotation(q.eulerAngles.x, 0, q.eulerAngles.z);
        return q;
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

        buttonNextEnterDelay -= Time.deltaTime;
		if(buttons == null) {
			UnityEngine.Debug.Log("DynamicHexList: buttons is null");
		}
        if (buttonNextEnterDelay <= 0 && buttons.Count > buttonNextEnterIndex)
        {

            PlayButtonEnter(buttons[buttonNextEnterIndex], true);

            UISprite[] sprites = buttons[buttonNextEnterIndex].GetComponentsInChildren<UISprite>() as UISprite[];
            foreach (UISprite spr in sprites)
            {
                if (spr.name == "Foreground")
                {
                    spr.gameObject.SetActive(GetButtonData()[buttonNextEnterIndex].locked);
                    if (GetButtonData()[buttonNextEnterIndex].locked)
                    {
                        Debug.Log("Button Locked at column: " + GetButtonData()[buttonNextEnterIndex].column + " row: " + GetButtonData()[buttonNextEnterIndex].row);
                    }
                    break;
                }
            }

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
				ConvertOrientation(Platform.Instance.GetOrientation(), out newCameraOffset);
				Vector3 camPos = guiCamera.transform.position;
				newCameraOffset -= cameraMoveOffset;
                guiCamera.transform.position = new Vector3(newCameraOffset.x, newCameraOffset.y, camPos.z);
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

            foreach (RaycastHit hit in hits)
            {
                UIImageButton button = hit.collider.GetComponent<UIImageButton>();
                if (button == null) continue;

                newSelection = button;
                if (newSelection == selection)
                {
                    selectionStillActive = true;
                    break;
                }
            }

            if (!selectionStillActive && newSelection != null)
            {
                if (selection != null)
                {
                    selection.SendMessage("OnHover", false, SendMessageOptions.DontRequireReceiver);
                    TweenPosition tp = guiCamera.GetComponent<TweenPosition>();
                    if (tp != null)
                    {

                        TweenPosition.Begin(tp.gameObject, 0.3f, cameraPosition);
                    }
                }
                selection = newSelection;
                newSelection.SendMessage("OnHover", true, SendMessageOptions.DontRequireReceiver);

                //selection changed we want to stop dragging, user need to start drag from the start
                dragging = false;
            }

            bool debugMouse = Input.GetMouseButton(0);

            if (selection != null && (Input.touchCount > 0 || debugMouse))
            {

                Touch touch = new Touch();
                bool found = false;

                if (!debugMouse)
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
                    if (!debugMouse)
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
        buttonsReady = false;

        if (elementsToKeep < 1) elementsToKeep = 1;

        while (transform.childCount > elementsToKeep)
        {
            GameObject.Destroy(transform.GetChild(transform.childCount - 1));
        }
    }

    /// <summary>
    /// Builds screen buttons based on first prefab button it contains
    /// </summary>
    /// <returns></returns>
    public  void InitializeItems()
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
        buttonEnterDelay = screenEnterTime / count;

        Transform child = transform.GetChild(0);
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

        float Z = child.transform.position.z;
        for (int i = 0; i < count; i++)
        {
            GameObject tile = null;
            if (i >= transform.childCount)
            {
                tile = (GameObject)GameObject.Instantiate(child.gameObject);
                tile.transform.parent = child.parent;
                tile.transform.rotation = child.rotation;
                tile.transform.localScale = child.localScale;
            }
            else
            {
                tile = transform.GetChild(i).gameObject;
            }
            Vector3 pos = GetLocation(GetButtonData()[i].column, GetButtonData()[i].row);
            //if (radius == 0)
            //{
            //    Debug.LogError("RADIUS 0!");
            //}

            //float angle = pos.x / (Mathf.PI * radius);
            //0.989 is value I found matching best to close ui line behind players back
            //angle *= 180 * 0.989f;
            //Quaternion rotation = Quaternion.Euler(new Vector3(0.0f, angle, 0.0f));
            //Vector3 rotationalPos = rotation * distanceVector;

            Vector3 hexPosition = new Vector3(pos.x, pos.y, pos.z);
            tile.transform.position = hexPosition;
            //tile.transform.Rotate(new Vector3(0.0f, angle, 0.0f));
            tile.name = GetButtonData()[i].buttonName;

            FlowButton fb = tile.GetComponentInChildren<FlowButton>();
            if (fb != null)
            {
                fb.owner = parent;
                fb.name = GetButtonData()[i].buttonName;
                UIImageButton graphics = fb.GetComponentInChildren<UIImageButton>();
                graphics.pressedSprite = GetButtonData()[i].imageName;
                graphics.hoverSprite = graphics.pressedSprite;
                graphics.normalSprite = graphics.pressedSprite;
                graphics.disabledSprite = graphics.pressedSprite;                
            }

            buttons.Add(tile);
        }

        foreach (GameObject go in buttons)
        {
            go.SetActive(false);
        }
    }

    /// <summary>
    /// Get location in 2d space for the element based on its column and row position
    /// </summary>
    /// <param name="column">horizontal position (might be negative)</param>
    /// <param name="row">vertical position (might be negative)</param>
    /// <returns>flat 2d position for hex </returns>
    Vector2 GetLocation(int column, int row)
    {
        int Yoffset = -(Mathf.Abs(column) % 2);
        return new Vector2(hexLayoutOffset.x * column, -hexLayoutOffset.y * (Yoffset + row * 2));
    }

    /// <summary>
    /// Finds generic poistion for element using some predefined algorithm
    /// </summary>
    /// <param name="index">button index requested</param>
    /// <returns>flat 2d position for hex </returns>
    Vector2 GetLocation(int index)
    {
        if (hexLayoutOffset == Vector2.zero)
        {
            if (transform.childCount == 0) return Vector2.zero;

            Transform child = transform.GetChild(0);
            BoxCollider c = child.GetComponentInChildren<BoxCollider>();
            Bounds b;
            if (c != null)
            {
                b = c.bounds;
            }
            else
            {
                SphereCollider sc = child.GetComponentInChildren<SphereCollider>();
                b = sc.bounds;
            }
            float upOffset = b.extents.y;
            float sideOffset = Mathf.Sqrt(3 * upOffset * upOffset);
            hexLayoutOffset = new Vector2(sideOffset, upOffset);
        }

        //our design expect some hard coded positioning of the hexes

        switch (index)
        {
            case 0:
                return Vector2.zero;
            case 1:
                return new Vector2(0, hexLayoutOffset.y * 2);
            case 2:
                return new Vector2(hexLayoutOffset.x, hexLayoutOffset.y);
            case 3:
                return new Vector2(hexLayoutOffset.x, -hexLayoutOffset.y);
            case 4:
                return new Vector2(0, -hexLayoutOffset.y * 2);
            case 5:
                return new Vector2(-hexLayoutOffset.x, -hexLayoutOffset.y);
            case 6:
                return new Vector2(-hexLayoutOffset.x, hexLayoutOffset.y);

            case 7:
                return new Vector2(hexLayoutOffset.x, hexLayoutOffset.y * 3);
            case 8:
                return new Vector2(hexLayoutOffset.x * 2, hexLayoutOffset.y * 2);
            case 9:
                return new Vector2(hexLayoutOffset.x * 2, 0);
            case 10:
                return new Vector2(hexLayoutOffset.x * 2, -hexLayoutOffset.y * 2);
            case 11:
                return new Vector2(hexLayoutOffset.x, -hexLayoutOffset.y * 3);

            case 12:
                return new Vector2(-hexLayoutOffset.x, -hexLayoutOffset.y * 3);
            case 13:
                return new Vector2(-hexLayoutOffset.x * 2, -hexLayoutOffset.y * 2);
            case 14:
                return new Vector2(-hexLayoutOffset.x * 2, 0);
            case 15:
                return new Vector2(-hexLayoutOffset.x * 2, hexLayoutOffset.y * 2);
            case 16:
                return new Vector2(-hexLayoutOffset.x, hexLayoutOffset.y * 3);

            default:
                int sequentalID = index - 17;
                int stage = (int)(sequentalID / 14);
                int step = sequentalID % 14;
                if (step < 4)
                {
                    return new Vector2(hexLayoutOffset.x * (3 + stage * 2), hexLayoutOffset.y * (3 - 2 * step));
                }
                else if (step < 8)
                {
                    return new Vector2(-hexLayoutOffset.x * (3 + stage * 2), hexLayoutOffset.y * (3 - 2 * (step - 4)));
                }
                else if (step < 11)
                {
                    return new Vector2(hexLayoutOffset.x * (4 + stage * 2), hexLayoutOffset.y * (2 - 2 * (step - 8)));
                }
                else
                {
                    return new Vector2(-hexLayoutOffset.x * (4 + stage * 2), hexLayoutOffset.y * (2 - 2 * (step - 11)));
                }
        }
    }

    /// <summary>
    /// Plays animation to enter button
    /// </summary>
    /// <param name="buttonRoot">Game Object which is root element in button structure.</param>
    /// <param name="forward">should animation go forward or backward</param>
    /// <returns></returns>
    public void PlayButtonEnter(GameObject buttonRoot, bool forward)
    {
        buttonRoot.SetActive(true);
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
                    buttonLocker.OnButtonAnimStarted();
                    EventDelegate.Add(anim.onFinished, buttonLocker.OnButtonAnimFinished, true);
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
		GestureHelper.swipeLeft -= leftHandler;
    }

}
