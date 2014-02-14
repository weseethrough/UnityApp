using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;

/// <summary>
/// Class which can be extended to be saved/loaded by automatic restoration system. Currently doesn't support persistent data type
/// </summary>
[System.Serializable]
public class GameRestorableObject : ISerializable 
{   

	/// <summary>
	/// default constructor and initialization
	/// </summary>
	/// <returns></returns>
	public GameRestorableObject()
	{        
        
	}
    
	/// <summary>
	/// deserialization constructor
	/// </summary>
	/// <param name="info">serialziation info containing data about all subvariables</param>
	/// <param name="ctxt">serialziation context</param>	
    public GameRestorableObject(SerializationInfo info, StreamingContext ctxt)
	{
        
	}
	
	/// <summary>
	/// serialization function called by serializer
	/// </summary>
    /// <param name="info">serialziation info containing data about all subvariables</param>
    /// <param name="ctxt">serialziation context</param>
	/// <returns></returns>
	public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
   	{
   	}

}
