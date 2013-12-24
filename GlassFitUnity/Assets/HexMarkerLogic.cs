using UnityEngine;
using System.Collections;

/// <summary>
/// Class designed to manage hex selection. its independent from layer itself and is expecting position to be provided from 
/// </summary>
public class HexMarkerLogic : MonoBehaviour 
{

    public static HexMarkerLogic instance;    
    //public TweenPosition tween;

	/// <summary>
	/// Default unity initialization function which sets instance pointer allowing for later interaction 
	/// </summary>
	/// <returns></returns>
	void Awake ()
    {
        //targetPosition = Vector3.zero;
        instance = this;
        TweenPosition tween = gameObject.GetComponent<TweenPosition>();
        if (tween == null)
        {
            tween = gameObject.AddComponent<TweenPosition>();            
        }
        tween.method = UITweener.Method.EaseInOut;
        
        gameObject.SetActive(false);

	}
	
    /// <summary>
    /// Allows to set new position for marker to animate to
    /// </summary>
    /// <param name="target">target position for marker</param>
    /// <returns></returns>
    public static void SetTarget(Vector3 target)
    {
        if ( instance != null)
        {
            instance.gameObject.SetActive(true);            
            TweenPosition.Begin(instance.gameObject, 0.2f, target);
        }
    }	
}
