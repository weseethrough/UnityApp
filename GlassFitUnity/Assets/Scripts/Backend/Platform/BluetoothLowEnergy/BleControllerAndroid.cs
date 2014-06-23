using System;
using UnityEngine;
#if UNITY_ANDROID
public class BleControllerAndroid : BleController
{
    private Log log = new Log("BleControllerAndroid");
    private AndroidJavaObject AndroidBlePlugin;

    public BleControllerAndroid (AndroidJavaObject androidContext)
    {
        try {
            AndroidBlePlugin = new AndroidJavaObject("com.glassfitgames.glassfitplatform.BLE.BluetoothLeUnityPlugin", androidContext);
        } catch (Exception e) {
            log.error (e, "Failed to start Android BLE plugin - device may not have BLE support");
        }
    }

    public override void StartListening()
    {
        AndroidBlePlugin.Call ("startListening");
    }

    public override void StopListening()
    {
        AndroidBlePlugin.Call ("stopListening");
    }
}
#endif

