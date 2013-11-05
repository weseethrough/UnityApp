using UnityEngine;
using System.Collections;

public class DynamicHexList : MonoBehaviour 
{
    Camera  guiCamera;
    Vector2 lineOffset;

	void Start () 
    {
        Camera[] camList = (Camera[])Camera.FindObjectsOfType(typeof(Camera));
        foreach (Camera c in camList)
        {
            if (c.gameObject.layer == LayerMask.NameToLayer("GUI"))
            {
                guiCamera = c;
                break;
            }
        }

        if (guiCamera != null)
        {            
            InitializeItems(3);
        }        
	}

    void CleanupChildren(int elementsToKeep)
    {
        if (transform.childCount < 1)
        {
            Debug.LogError("List doesn't have at least one button element to clone later");
            return;
        }

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

    void InitializeItems(int count)
    {
        if (transform.childCount < 1) 
        {
            Debug.LogError("List doesn't have at least one button element to clone");            
            return;
        }        
        CleanupChildren(count);                    

        Transform child = transform.GetChild(0);
        float Z = child.transform.position.z;
        for (int i = 0; i < count; i++)
        {
            //ensure we have 
            GameObject tile = null;
            if (i >= transform.childCount)
            {
                tile = (GameObject)GameObject.Instantiate(child.gameObject);
                tile.transform.parent       = child.parent;
                //tile.transform.position     = child.position;
                tile.transform.rotation     = child.rotation;
                tile.transform.localScale   = child.localScale;
            }
            else
            {
                tile = transform.GetChild(i).gameObject;
                tile.SetActive(true);
            }
            Vector2 pos = GetLocation(i);

            tile.transform.position = new Vector3(pos.x, pos.y, Z);

           // tile.
        }        
    }

    Vector2 GetLocation(int index)
    {        
        if (lineOffset == Vector2.zero)
        {
            if (transform.childCount == 0) return Vector2.zero;

            Transform child = transform.GetChild(0);
            BoxCollider c = child.GetComponentInChildren<BoxCollider>();
            Bounds b = c.bounds;
            float upOffset = b.extents.y;
            float sideOffset = Mathf.Sqrt(3 * upOffset* upOffset);
            lineOffset = new Vector2(sideOffset, upOffset);
        }

            //our design expect some hardcoded positioning of the hexes

        switch (index)
        {
            case 0:
                return Vector2.zero;
            case 1:
                return new Vector2(0, lineOffset.y * 2);
            case 2:
                return new Vector2(lineOffset.x, lineOffset.y );
            case 3:
                return new Vector2(lineOffset.x, -lineOffset.y);
            case 4:
                return new Vector2(0, -lineOffset.y * 2);
            case 5:
                return new Vector2(-lineOffset.x, -lineOffset.y);
            case 6:
                return new Vector2(-lineOffset.x, lineOffset.y);

            case 7:
                return new Vector2(lineOffset.x, lineOffset.y * 3);
            case 8:
                return new Vector2(lineOffset.x * 2, lineOffset.y * 2);
            case 9:
                return new Vector2(lineOffset.x * 2, 0);
            case 10:
                return new Vector2(lineOffset.x * 2, -lineOffset.y * 2);
            case 11:
                return new Vector2(lineOffset.x, -lineOffset.y * 3);

            case 12:
                return new Vector2(-lineOffset.x, -lineOffset.y * 3);
            case 13:
                return new Vector2(-lineOffset.x * 2, -lineOffset.y * 2);
            case 14:
                return new Vector2(-lineOffset.x * 2, 0);
            case 15:
                return new Vector2(-lineOffset.x * 2, lineOffset.y * 2);
            case 16:
                return new Vector2(-lineOffset.x, lineOffset.y * 3);

            default:
                int sequentalID = index - 17;
                int stage = (int)(sequentalID / 14);
                int step = sequentalID % 14;
                if (step < 4)
                {
                    return new Vector2(lineOffset.x * (3+stage*2), lineOffset.y * (3 - 2 * step));
                }
                else if (step < 8)
                {
                    return new Vector2(- lineOffset.x * (3 + stage * 2), lineOffset.y * (3 - 2 * (step - 4)));
                }
                else if (step < 11)
                {
                    return new Vector2(lineOffset.x * (4 + stage * 2), lineOffset.y * (2 - 2 * (step - 8)));
                }
                else
                {
                    return new Vector2(- lineOffset.x * (4 + stage * 2), lineOffset.y * (2 - 2 * (step - 11)));
                }                
        }        
    }
}
