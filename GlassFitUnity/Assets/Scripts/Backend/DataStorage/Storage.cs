using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;

/// <summary>
/// storage class is simply wrapping class around storage dictionary which allows to plug and unplug easily new classes without losing reference to this root point in places which uses it directly
/// </summary>
[System.Serializable]
public class Storage : ISerializable 
{
	public StorageDictionary dictionary;
	
	/// <summary>
	/// default constructor and initializator
	/// </summary>
	/// <returns></returns>
	public Storage()
	{
		dictionary = new StorageDictionary();	
	}
    
	/// <summary>
	/// deserialziation constructor
	/// </summary>
	/// <param name="info">serialziation info containing data about all subvariables</param>
	/// <param name="ctxt">serialziation context</param>	
	public Storage(SerializationInfo info, StreamingContext ctxt)
	{
		this.dictionary = (StorageDictionary)info.GetValue("Data", typeof(StorageDictionary));
	}
	
	/// <summary>
	/// serialziation function caleld by serializer
	/// </summary>
    /// <param name="info">serialziation info containing data about all subvariables</param>
    /// <param name="ctxt">serialziation context</param>
	/// <returns></returns>
	public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
   	{
    	info.AddValue("Data", this.dictionary);
   	}
}
