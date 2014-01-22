using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

/// <summary>
/// advanced panel which allows to display complex hex menu screens
/// </summary>
[Serializable]
public class TestHexPanel : HexPanel
{
    
    /// <summary>
    /// default constructor
    /// </summary>
    /// <returns></returns>
    public TestHexPanel() : base() { }

    /// <summary>
    /// deserialization constructor
    /// </summary>
    /// <param name="info">seirilization info conataining class data</param>
    /// <param name="ctxt">serialization context </param>
    /// <returns></returns>
    public TestHexPanel(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt) { }

    public override void EnterStart()
    {
        HexButtonData hbd = new HexButtonData();
        hbd.row = 0;
        hbd.column = 0;
        hbd.buttonName = "hex1";
        hbd.displayInfoData = false;
        hbd.textNormal = "DATA : Hex1 ";
        hbd.locked = false;

        buttonData.Add(hbd);

        hbd = new HexButtonData();
        hbd.row = -1;
        hbd.column = 0;
        hbd.buttonName = "hex2";
        hbd.displayInfoData = true;
        hbd.textNormal = "DATA : Hex2";
        hbd.locked = true;

        buttonData.Add(hbd);

        hbd = new HexButtonData();
        hbd.row = 0;
        hbd.column = 1;
        hbd.buttonName = "hex3";
        hbd.displayInfoData = true;
        hbd.textNormal = "DATA : Hex3";
        hbd.locked = true;

        buttonData.Add(hbd);

        hbd = new HexButtonData();
        hbd.row = 1;
        hbd.column = 0;
        hbd.buttonName = "hex4";
        hbd.displayInfoData = true;
        hbd.textNormal = "DATA : Hex4";
        hbd.locked = true;

        buttonData.Add(hbd);

        hbd = new HexButtonData();
        hbd.row = 1;
        hbd.column = -1;
        hbd.buttonName = "hex4";
        hbd.displayInfoData = true;
        hbd.textNormal = "DATA : Hex4";
        hbd.locked = false;

        buttonData.Add(hbd);

        base.EnterStart();
    }

}
