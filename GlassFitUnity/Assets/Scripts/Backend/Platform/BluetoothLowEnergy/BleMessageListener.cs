using System;
using UnityEngine;
using RaceYourself.Models;
using SimpleJSON;
using Sqo;
using SiaqodbUtils;

public class BleMessageListener : MonoBehaviour
{
    private Log log = new Log("BleMessageListener");
    private Siaqodb db = DatabaseFactory.GetInstance();

    void Awake()
    {
        log.info("Awake()");
        DontDestroyOnLoad(gameObject);
    }

    // TODO: add delegates to do useful things with the incoming data
    // These methods currently pick up incoming unity messages

    public void OnBleHeartrateData (String message)
    {
        log.info ("New BLE heart-rate: " + message + "bpm");
    }

    public void OnBleCadenceData (String message)
    {
        log.info ("New BLE cadence: " + message + "rpm");
    }

    public void OnBleWheelSpeedData (String message)
    {
        log.info ("New BLE wheel-speed: " + message + "rpm");
    }
}

