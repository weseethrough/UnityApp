using System;
using UnityEngine;

using RaceYourself.Models;
#if UNITY_ANDROID
public class AndroidNotification : Notification
{
	public AndroidJavaObject ajo = null;
}
#endif
