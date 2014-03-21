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

}