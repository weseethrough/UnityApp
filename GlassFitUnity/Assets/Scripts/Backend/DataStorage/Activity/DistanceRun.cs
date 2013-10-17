using UnityEngine;
using System.Collections;

public class DistanceRun : MonoBehaviour 
{
    /// <summary>
    /// Time specified in seconds, although its safe to use 0.01 parts of the seconds as well.
    /// </summary>
    public float targetTimeSec = 60.0f;
    /// <summary>
    /// Distance player need to travel to achieve success. Feel free to use 0.01 parts of th emeter if you need.
    /// </summary>
    public float targetDistanceMeter = 100.0f;
}
