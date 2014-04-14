using UnityEngine;
using System.Collections;
using System.Collections.Generic;



[System.Serializable]
public class GConnectorBase //: ScriptableObject // i.e. referenced serialization
{
    public GNodeBase Parent; // owner of this connector
    public bool IsInput;
    public string Name;
    public string Type;
    public List<GConnectorBase> Link = new List<GConnectorBase>();
    public string EventFunction;

    public Vector2 GetPosition(GraphDataBase graph)
    {
        //Debug.Log("GetPosition");
        if (Parent != null)
        {
            //Debug.Log("Index = "+index);
            if (Parent.Inputs != null)
            {
                int index = Parent.Inputs.IndexOf(this);
                if (index >= 0)
                {
                    Rect r = Parent.GetInputRect(graph, index);
                    //Debug.Log("Input Rect = "+r.ToString());
                    return r.center;
                }
            }
            if (Parent.Outputs != null)
            {
                int index = Parent.Outputs.IndexOf(this);
                //Debug.Log("Index = "+index);
                if (index >= 0)
                {
                    Rect r = Parent.GetOutputRect(graph, index);
                    //Debug.Log("Output Rect = "+r.ToString());
                    return r.center;
                }
            }
        }
        return Vector2.zero;
    }
}
