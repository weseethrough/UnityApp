using UnityEngine;
using System.Collections;

public class InitSignupForm : MonoBehaviour
{
    protected static Log log = new Log("InitSignupForm");

	// Use this for initialization
	void Start ()
    {
        object o = DataVault.Get("fb");
        bool fb = false;
        if (o != null)
            fb = (bool) o;
        GameObject[] passwordFields = GameObject.FindGameObjectsWithTag("PasswordInputs");
        
        foreach (GameObject passwordField in passwordFields)
        {
            passwordField.SetActive(!fb);
        }
        
        if (fb)
        {
            string firstName = (string) DataVault.Get("first_name");
            if (firstName != null)
                UpdateLabel (gameObject, "ForenameInput", firstName);

            string surname = (string) DataVault.Get("surname");
            if (surname != null)
                UpdateLabel (gameObject, "SurnameInput", surname);

            string email = (string) DataVault.Get("email");
            if (email != null)
                UpdateLabel (gameObject, "EmailInput", email);
        }
    }

    // TODO copy-paste + small modif from ButtonFunctionCollection.UpdateLabel
    private static void UpdateLabel(GameObject widgetRoot, string inputName, string value)
    {
        GameObject inputField = GameObjectUtils.SearchTreeByName(widgetRoot, inputName);
        if (inputField == null)
        {
            log.error("Unable to find " + inputName + " field in mobile signup panel!");
        }
        else
        {
            string valOrEmpty = value == null ? "" : value;
            UIBasiclabel basicLabel = inputField.GetComponent<UIBasiclabel>();
            basicLabel.SetLabel(valOrEmpty);

            UIInput input = inputField.GetComponentInChildren<UIInput>();
            input.defaultText = "Keep Labels Value";
            
            UILabel label = inputField.GetComponentInChildren<UILabel>();
            label.text = valOrEmpty;
        }
    }
}
