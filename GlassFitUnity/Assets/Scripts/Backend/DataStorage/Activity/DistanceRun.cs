using UnityEngine;
using System.Collections;

/// <summary>
/// component designed to be base structure for distance activities 
/// </summary>
public class DistanceRun : MonoBehaviour 
{
    /// <summary>
    /// Time specified in milliseconds
    /// </summary>
    public double targetTimeMS;
    /// <summary>
    /// Distance player need to travel to achieve success in cm
    /// </summary>
    public double targetDistanceCM;
}
