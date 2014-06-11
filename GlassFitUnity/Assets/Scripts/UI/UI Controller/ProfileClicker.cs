using System;
using UnityEngine;
public class ProfileClicker : MonoBehaviour
{
	public GameObject defaultProfilePicture;
	public GameObject mainProfilePicture;

	private bool toggled = false;

	public void Awake () {
		toggled = !toggled;
		OnClick();
	}

	public void OnClick () {
		UnityEngine.Debug.Log("Toggling profile image");
		toggled = !toggled;
		defaultProfilePicture.SetActive(!toggled);
		mainProfilePicture.SetActive(toggled);
	}
}

