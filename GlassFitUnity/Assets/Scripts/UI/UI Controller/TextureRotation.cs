using UnityEngine;
using System.Collections;

public class TextureRotation : MonoBehaviour
{
    private float rotation = 0f;

    // Update is called once per frame
    void Update ()
    {
        rotation -= 360.0f * Time.deltaTime;
        
        transform.rotation = Quaternion.Euler(0, 0, rotation);
    }
}
