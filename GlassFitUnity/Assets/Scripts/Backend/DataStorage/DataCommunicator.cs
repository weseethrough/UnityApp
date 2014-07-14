using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;



/// <summary>
/// 
/// </summary>
public class DataCommunicator : MonoBehaviour
{
    #region Data Vault    
    static public void SetDataVault(string dataName, object data, bool perisistent)
    {        
        DataVault.Set(dataName, data);
        DataVault.SetPersistency(dataName, perisistent);
    }

    static public object GetDataVault(string dataName)
    {
        return DataVault.Get(dataName);
    }

    static public void SaveDataVault()
    {
        DataVault.SaveToBlob();
    }    
    #endregion

    #region Race settings
    static public void SetRace(string type, string raceType, bool indoor)
    {
        DataVault.Set("type", type);
        DataVault.Set("race_type", raceType);
        Platform.Instance.SetIndoor(indoor);
    }

   /* static public void SetPause(bool show)
    {

    }*/
    #endregion

}
