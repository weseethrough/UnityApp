using UnityEngine;
using System.Collections;

public class GStyleColor
{
    public float r = 0.0f;
    public float g = 0.0f;
    public float b = 0.0f;
    public float a = 0.0f;
    
    public GStyleColor(float r, float g, float b) 
    {
        this.r = r;
        this.g = g;
        this.b = b;
        this.a = 0.0f;
    }

    public GStyleColor(float r, float g, float b, float a)
    {
        this.r = r;
        this.g = g;
        this.b = b;
        this.a = a;
    }
    
    public UnityEngine.Color GetColor()
    {
        return new UnityEngine.Color(r, g, b, a);
    }
};
