using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DynamicHexList : MonoBehaviour 
{
    HexPanel parent = null;

    UICamera guiCamera;
    Vector2 hexLayoutOffset = new Vector2(0.4330127f, 0.25f);    
    Vector3 distanceVector;
    Vector3 cameraPosition;
    Quaternion cameraStartingRotation;
    float radius;

    float screenEnterTime = 0.8f;
    float buttonEnterDelay = 0.0f;
    float buttonNextEnterDelay = 0.0f;
    int buttonNextEnterIndex = 0;

    int buttonCount = 35;    

    List<GameObject> buttons;
    UIImageButton selection;
    public string buttonEnterAnimation = "HexEnter";

    bool buttonsReady = false;       

	void Start () 
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
            cameraStartingRotation = guiCamera.transform.rotation;
            
            radius = distanceVector.magnitude;

            Quaternion rot = SensorHelper.rotation;
            if (!float.IsNaN(rot.x) &&  !float.IsNaN(rot.y) && !float.IsNaN(rot.z) && !float.IsNaN(rot.w))
            {
                Quaternion newOffset = Quaternion.Inverse(cameraStartingRotation) * SensorHelper.rotation;
                guiCamera.transform.rotation = newOffset;
            }
            else
            {
                Debug.LogError("Sensor data invalid");
            }

          // FOR DEBUG ONLY COMMENTED OUT
          //  guiCamera.useMouse = false;
          //  guiCamera.useTouch = false;            
        }


        InitializeItems(buttonCount);
	}

    public void SetButtonCount(int count)
    {
        buttonCount = count;
    }

    public void SetParent( HexPanel _parent)
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

        //if button enter delay is below 0 at this stage then screen has finished loading
        if (guiCamera != null)
        {
            Quaternion rot = SensorHelper.rotation;
            if (!float.IsNaN(rot.x) && !float.IsNaN(rot.y) && !float.IsNaN(rot.z) && !float.IsNaN(rot.w))
            {
                Quaternion newOffset = Quaternion.Inverse(cameraStartingRotation) * SensorHelper.rotation;
                guiCamera.transform.rotation = newOffset;
            }
            else
            {
                Debug.LogError("Sensor data invalid");
            }
            Vector3 forward = guiCamera.transform.forward;

            RaycastHit[] hits = Physics.RaycastAll(cameraPosition, forward , 5.0f);// ,LayerMask.NameToLayer("GUI"));

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
                }
                selection = newSelection;
                newSelection.SendMessage("OnHover", true, SendMessageOptions.DontRequireReceiver);
            }
        }

        //we need to be sure all buttons are ready before setting parent owner.
        if (!buttonsReady)
        {            
            FlowButton[] fbs = gameObject.GetComponentsInChildren<FlowButton>();

            if (fbs.Length == buttons.Count)
            {
                buttonsReady = true;
                foreach (FlowButton fb in fbs)
                {
                    fb.owner        = parent;                    
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

        if (elementsToKeep == 0)
        {
            transform.GetChild(0).gameObject.SetActive(false);
        }

        if (elementsToKeep < 1) elementsToKeep = 1;

        while (transform.childCount > elementsToKeep)
        {
            GameObject.Destroy(transform.GetChild(transform.childCount - 1));
        }        
    }

    private void InitializeItems(int count)
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
        CleanupChildren(count);
        buttonEnterDelay = screenEnterTime / count;

        Transform child = transform.GetChild(0);


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
            //ensure we have 
            GameObject tile = null;
            if (i >= transform.childCount)
            {
                tile = (GameObject)GameObject.Instantiate(child.gameObject);
                tile.transform.parent       = child.parent;                
                tile.transform.rotation     = child.rotation;
                tile.transform.localScale   = child.localScale;                 
            }
            else
            {
                tile = transform.GetChild(i).gameObject;                
            }
            Vector3 pos = GetLocation(i);            
            if (radius == 0)
            {
                Debug.LogError("RADIUS 0!");
            }

            float angle = pos.x / (Mathf.PI * radius);
            //0.989 is value I found matching best to close ui behind players back
            angle *= 180 * 0.989f;            
            Quaternion rotation = Quaternion.Euler(new Vector3(0.0f, angle , 0.0f));
            Vector3 rotationalPos = rotation * distanceVector;

            Vector3 hexPosition = cameraPosition + new Vector3(rotationalPos.x, pos.y, rotationalPos.z);
            tile.transform.position = hexPosition;// transform.worldToLocalMatrix.MultiplyVector(hexPosition);
            tile.SetActive(false);
            tile.transform.Rotate(new Vector3(0.0f, angle, 0.0f));
            tile.name = "Hex " + i;            

            buttons.Add(tile);           
        }        
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
            float sideOffset = Mathf.Sqrt(3 * upOffset* upOffset);
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
                return new Vector2(hexLayoutOffset.x, hexLayoutOffset.y );
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
                    return new Vector2(hexLayoutOffset.x * (3+stage*2), hexLayoutOffset.y * (3 - 2 * step));
                }
                else if (step < 8)
                {
                    return new Vector2(- hexLayoutOffset.x * (3 + stage * 2), hexLayoutOffset.y * (3 - 2 * (step - 4)));
                }
                else if (step < 11)
                {
                    return new Vector2(hexLayoutOffset.x * (4 + stage * 2), hexLayoutOffset.y * (2 - 2 * (step - 8)));
                }
                else
                {
                    return new Vector2(- hexLayoutOffset.x * (4 + stage * 2), hexLayoutOffset.y * (2 - 2 * (step - 11)));
                }                
        }        
    }

    public void PlayButtonEnter(GameObject buttonRoot, bool forward)
    {
        buttonRoot.SetActive(true);

        PlayLockingAnimation(buttonRoot, buttonEnterAnimation, forward);
    }



    public void PlayLockingAnimation(GameObject buttonRoot, string animationName, bool forward)
    {
        Animation target = buttonRoot.GetComponentInChildren<Animation>();
        if (target != null && buttonEnterAnimation.Length > 0)
        {
            AnimationOrTween.Direction dir = forward ? AnimationOrTween.Direction.Forward : AnimationOrTween.Direction.Reverse;
            ActiveAnimation anim = ActiveAnimation.Play(target, animationName, dir);

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
    public void PlayClosingAnimation(FlowButton selectedButton)
    {

    }
}
