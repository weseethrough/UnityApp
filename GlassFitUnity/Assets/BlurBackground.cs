using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class BlurBackground : MonoBehaviour {

    public GameObject noBlurCamera;
    public Shader blurShader;

	// Note: in Unity <4.5, attaching a blur to a camera will trigger the following error in the console (it can be safely ignored):
    // rect[2] == rt->GetGLWidth() && rect[3] == rt->GetGLHeight()
    // Further info: http://issuetracker.unity3d.com/issues/camera-with-depth-only-or-dont-clear-throws-error-when-image-effects-are-applied
	void Start ()
    {
        int noBlurLayerIdx = LayerMask.NameToLayer("NoBlur");
        int guiLayerIdx = LayerMask.NameToLayer("GUI");

        // change layer (recursive!) of physicalWidgetRoot (challenge sent's) to NoBlur
        GameObject widgetRoot = ((Panel) FlowStateMachine.GetCurrentFlowState()).physicalWidgetRoot;
        SetLayer(widgetRoot, noBlurLayerIdx);

        // find Main Camera (the GUI one)
        List<GameObject> cameras = new List<GameObject>(GameObject.FindGameObjectsWithTag("UICamera"));
        GameObject guiCamera2d = cameras.FirstOrDefault(c => c.layer == guiLayerIdx);

        // temporarily disable interaction via the main camera
        UICamera guiCamera2dUiCamera = guiCamera2d.GetComponent<UICamera>();
        guiCamera2dUiCamera.eventReceiverMask = 0;

        // add blur script to the Main Camera (2D GUI)
        Blur song2 = guiCamera2d.AddComponent<Blur>();
        song2.blurShader = blurShader; // not sure why this is necessary. Via editor it's set automatically...
        song2.enabled = true;

        // instantiate noBlurCamera; attach to Stage

        GameObject stage = guiCamera2d.transform.parent.gameObject;
        GameObject noBlurCameraInst = (GameObject) Instantiate(noBlurCamera);
        noBlurCameraInst.name = "NoBlur Camera";
        noBlurCameraInst.transform.parent = stage.transform;

        // TODO on transition,
        //remove Non Blurred Camera
        //remove blur script from MC
        //restore eventReceiverMask to GUI
	}

    private void SetLayer(GameObject gameObject, int layer)
    {
        gameObject.layer = layer;
        Transform[] childTransforms = gameObject.GetComponentsInChildren<Transform>();
        foreach (Transform child in childTransforms)
        {
            child.gameObject.layer = layer;
        }
    }
}
