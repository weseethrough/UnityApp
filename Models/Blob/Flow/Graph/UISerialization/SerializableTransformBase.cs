using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;

/// <summary>
/// special prepared for .net serialization transform data wrapper.
/// </summary>
[System.Serializable]
public class SerializableTransformBase : ISerializable 
{
    public float positionX;
    public float positionY;
    public float positionZ;

    public float rotationX;
    public float rotationY;
    public float rotationZ;
    public float rotationW;

    public float scaleX;
    public float scaleY;
    public float scaleZ;

    public SerializableTransformBase() { }

	/// <summary>
	/// constructor reading all required parameters from original transform
	/// </summary>
	/// <param name="source">transform source to be preared for serialziation</param>	
	public SerializableTransformBase(Transform source)
	{
        if (source != null)
        {
            Quaternion localRotation = source.localRotation;
            this.rotationX = localRotation.x;
            this.rotationY = localRotation.y;
            this.rotationZ = localRotation.z;
            this.rotationW = localRotation.w;

            Vector3 localPosition = source.localPosition;
            this.positionX = localPosition.x;
            this.positionY = localPosition.y;
            this.positionZ = localPosition.z;

            Vector3 localScale = source.localScale;
            this.scaleX = localScale.x;
            this.scaleY = localScale.y;
            this.scaleZ = localScale.z;
        }
	}

    /// <summary>
    /// deserialization constructor
    /// </summary>
    /// <param name="info">serialization info storing all configuration parameters</param>
    /// <param name="ctxt">serialziation context</param>    
    public SerializableTransformBase(SerializationInfo info, StreamingContext ctxt)
	{
        this.rotationX  = (float)info.GetValue("RotationX", typeof(float));
        this.rotationY  = (float)info.GetValue("RotationY", typeof(float));
        this.rotationZ  = (float)info.GetValue("RotationZ", typeof(float));
        this.rotationW  = (float)info.GetValue("RotationW", typeof(float));

        this.positionX  = (float)info.GetValue("PositionX", typeof(float));	
        this.positionY  = (float)info.GetValue("PositionY", typeof(float));	
        this.positionZ  = (float)info.GetValue("PositionZ", typeof(float));	                
        
        this.scaleX     = (float)info.GetValue("ScaleX", typeof(float));
        this.scaleY     = (float)info.GetValue("ScaleY", typeof(float));
        this.scaleZ     = (float)info.GetValue("ScaleZ", typeof(float));
	}
	
	/// <summary>
	/// serialization function called by the serializer
	/// </summary>
    /// <param name="info">serialization info storing all configuration parameters</param>
    /// <param name="ctxt">serialziation context</param>
	/// <returns></returns>
	public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
   	{
        info.AddValue("RotationX", this.rotationX);
        info.AddValue("RotationY", this.rotationY);
        info.AddValue("RotationZ", this.rotationZ);
        info.AddValue("RotationW", this.rotationW);

        info.AddValue("PositionX", this.positionX);
        info.AddValue("PositionY", this.positionY);
        info.AddValue("PositionZ", this.positionZ);
        
        info.AddValue("ScaleX", this.scaleX);
        info.AddValue("ScaleY", this.scaleY);
        info.AddValue("ScaleZ", this.scaleZ);       
   	}
	
    /// <summary>
    /// writes configuration back to transform allowing for proper setting reconstruction
    /// </summary>
    /// <param name="target">transform to write to</param>
    /// <returns></returns>
    public void WriteToTransform(Transform target)
    {
        if (target != null)
        {
            target.localPosition = new Vector3(this.positionX, this.positionY, this.positionZ);
            target.localRotation = new Quaternion(this.rotationX, this.rotationY, this.rotationZ, this.rotationW);
            target.localScale = new Vector3(this.scaleX, this.scaleY, this.scaleZ);
        }
    }
}
