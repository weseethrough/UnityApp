using UnityEngine;
using System.Threading;
using System;

public class PlatformPartner : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        Platform.Instance.Update();
    }

    //SingletonBaseTestEnviro[] list;

	/*void Awake()
    {
        list = new SingletonBaseTestEnviro[100];
        for (int i=0; i<100; i++)
        {
            list[i] = new SingletonBaseTestEnviro();

        }
    }*/

    public void OnApplicationPause(bool paused)
    {
        Debug.Log("Pause order received to set to "+paused);
    }

    public void OnApplicationFocus(bool paused)
    {
        Debug.Log("Focus change order received with "+paused);
        Platform.Instance.OnApplicationFocus(paused);
    }

    public void OnApplicationQuit()
    {
        Debug.Log("Quit order received");
        Platform.Instance.OnApplicationQuit();
    }
}