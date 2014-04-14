using UnityEngine;
using System.Collections;

public class GVector2
{
    public float x;
    public float y;    
    
    public GVector2()
    {
        this.x = 0.0f;
        this.y = 0.0f;
    }

    public GVector2(float x, float y) 
    {
        this.x = x;
        this.y = y;        
    }

    public static implicit operator GVector2(UnityEngine.Vector2 v)
    {
        return new GVector2(v.x, v.y);
    }

    public UnityEngine.Vector2 GetVector2()
    {
        return new UnityEngine.Vector2(x, y);
    }
};
