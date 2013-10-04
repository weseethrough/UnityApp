using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class SimpleLocalizationEditor : MonoBehaviour
{
	static public SimpleLocalizationManager instance;
	
	IDictionary<string, string> localizationData;
	
	void Awake()
	{
		instance = this;
		localizationData = new Dictionary();
	}		
	
	public void Populate()
	{
		localizationData = 
	}
}
