using UnityEngine;
using System.Collections;

// Graph nodes can have parameters which can be manipulated distinct from input connectors.
[System.Serializable]
public class GParameter
{
	public string Key;
	public string Value;
	public GraphValueType Type;
	
	public float FloatMin,FloatMax; // optional range
	
	public bool HasRange()
	{
		return FloatMin != FloatMax;
	}
	public void SetRange(float min, float max)
	{
		FloatMin = min;
		FloatMax = max;
	}
}

public enum GraphValueType
{
	Invalid,
	String,
	Boolean,
	Integer,
	Float,
	Vector2,
	Vector3,
	Vector4,
    UIPrefab,
    Settings,
    HexButtonManager,
	// Add any desired types!
};