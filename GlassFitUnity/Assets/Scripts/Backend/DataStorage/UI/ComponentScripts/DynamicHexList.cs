using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DynamicHexList : MonoBehaviour
{
    HexPanel    parent                  = null;

    UICamera    guiCamera;
    Vector2     hexLayoutOffset         = new Vector2(0.4330127f, 0.25f);
    Vector3     distanceVector;
    Vector3     cameraPosition;
    float       radius;

    //default height and rotation is used as a default offset when reseting sensors
    Quaternion  cameraDefaultRotation;
    float       heightDefaultOffset;    

    float       screenEnterTime         = 0.8f;
    float       buttonEnterDelay        = 0.0f;
    float       buttonNextEnterDelay    = 0.0f;
    int         buttonNextEnterIndex    = 0;

    int         buttonCount             = 0;

    List<GameObject> buttons;
    UIImageButton selection;
    private string btEnterAnimation     = "HexEnter";

    bool        buttonsReady            = false;

    Vector2     draggingStartPos        = Vector2.zero;
    bool        dragging                = false;
    int         draggingFingerID        = -1;    

    void Start()
    {        

        Camera[] camList = (Camera[])Camera.FindObjectsOfType(typeof(Camera));
        foreach (Camera c in camList)
        {
            UICamera uicam = c.GetComponent<UICamera>();
            if (uicam != null && c.gameObject.layer == LayerMask.NameToLayer("GUI"))
            {
                guiCamera = uicam;
                break;
            }
        }

        if (!SensorHelper.gotFirstValue)
        {
            SensorHelper.ActivateRotation();
        }

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
            Platform.Instance.resetGyro();
            cameraDefaultRotation = ConvertOrientation(Platform.Instance.getOrientation(), out heightDefaultOffset);
			//cameraStartingRotation = Quaternion.Euler(0, cameraStartingRotation.eulerAngles.y, 0);

            Quaternion newOffset = Quaternion.Inverse(cameraDefaultRotation) * cameraDefaultRotation;
            guiCamera.transform.rotation = newOffset;
#endif
            /* }
            else
            {
                Debug.LogError("Sensor data invalid");
            }*/

        }

        InitializeItems();
    }

    public void ResetGyro()
    {
#if !UNITY_EDITOR 
        Platform.Instance.resetGyro();
        cameraDefaultRotation = ConvertOrientation(Platform.Instance.getOrientation(), out heightDefaultOffset);
#endif
    }

    private Quaternion ConvertOrientation(Quaternion q,out float height)
    {
        //we stop pitch for the sake of height
        height = q.eulerAngles.x;
        return q;
    }

    //height is a result of the pich calculation so it should be between 0 and 360
    private float HeightToPositionValue(float height)
    {
        while(height < 0)
        {
            height += 360.0f;
        }
        height %= 360;
        if (height > 90 && height <= 270) {height = 180 - height;}
        else if (height > 270) {height = height - 360;}


        return height * 0.1f;
    }

    public List<HexButtonData> GetButtonData()
    {
        if (parent == null) return null;

        return parent.buttonData;
    }

    public void SetParent(HexPanel _parent)
    {
        parent = _parent;
    }

    void Update()
    {

        buttonNextEnterDelay -= Time.deltaTime;
        if (buttonNextEnterDelay <= 0 && buttons.Count > buttonNextEnterIndex)
        {

            PlayButtonEnter(buttons[buttonNextEnterIndex], true);
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
                Quaternion newOffset = Quaternion.Inverse(cameraDefaultRotation) * Platform.Instance.getOrientation();
                guiCamera.transform.rotation = newOffset;
                Vector3 cameraPos = guiCamera.transform.position;
                //cameraPos.y = HeightToPositionValue(pitchHeight - heightDefaultOffset);
                //guiCamera.transform.position = cameraPos;
#endif
            /*}
            else
            {
                Debug.LogError("Sensor data invalid");
            }*/
            Vector3 forward = guiCamera.transform.forward;

            RaycastHit[] hits = Physics.RaycastAll(guiCamera.transform.position, forward, 5.0f);// ,LayerMask.NameToLayer("GUI"));

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

    private void InitializeItems()
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
            if (radius == 0)
            {
                Debug.LogError("RADIUS 0!");
            }

            float angle = pos.x / (Mathf.PI * radius);
            //0.989 is value I found matching best to close ui line behind players back
            angle *= 180 * 0.989f;
            Quaternion rotation = Quaternion.Euler(new Vector3(0.0f, angle, 0.0f));
            Vector3 rotationalPos = rotation * distanceVector;

            Vector3 hexPosition = cameraPosition + new Vector3(rotationalPos.x, pos.y, rotationalPos.z);
            tile.transform.position = hexPosition;// transform.worldToLocalMatrix.MultiplyVector(hexPosition);            
            tile.transform.Rotate(new Vector3(0.0f, angle, 0.0f));
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

    Vector2 GetLocation(int column, int row)
    {
        int Yoffset = - (Mathf.Abs(column) % 2);
        return new Vector2(hexLayoutOffset.x * column, -hexLayoutOffset.y * (Yoffset + row*2));
    }

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
    }

}
