using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class BlurBackground : MonoBehaviour {

    private int noBlurLayerIdx;
    private int guiLayerIdx;

    private int eventReceiverMask;
    //private Panel panel;

    public GameObject noBlurCamera;
    public Shader blurShader;

    /// <summary>
    /// TODO clean up hack. Blur should be applied when panel is shown and removed when panel is dismissed. Doing it all in one class
    /// for symmetry/neatness, but in OnEnable(), physicalWidgetRoot hasn't yet been set and so is null. As such I resort to using
    /// Update() to apply the blurring.
    /// </summary>
    private bool blurred = false;

	// Note: in Unity <4.5, attaching a blur to a camera will trigger the following error in the console (it can be safely ignored):
    // rect[2] == rt->GetGLWidth() && rect[3] == rt->GetGLHeight()
    // Further info: http://issuetracker.unity3d.com/issues/camera-with-depth-only-or-dont-clear-throws-error-when-image-effects-are-applied
	void Awake ()
    {
        noBlurLayerIdx = LayerMask.NameToLayer("NoBlur");
        guiLayerIdx = LayerMask.NameToLayer("GUI");
    }

    void Update()
    {
        if (blurred)
            return;

        // change layer (recursive!) of physicalWidgetRoot (challenge sent's) to NoBlur
        Panel panel = (Panel) FlowStateMachine.GetCurrentFlowState();
        GameObject widgetRoot = panel.physicalWidgetRoot;
        SetLayer(widgetRoot, noBlurLayerIdx);

        // find Main Camera (the GUI one)
        List<GameObject> cameras = new List<GameObject>(GameObject.FindGameObjectsWithTag("UICamera"));
        GameObject guiCamera2d = cameras.FirstOrDefault(c => c.layer == guiLayerIdx);

        // temporarily disable interaction via the main camera
        UICamera guiCamera2dUiCamera = guiCamera2d.GetComponent<UICamera>();
        eventReceiverMask = guiCamera2dUiCamera.eventReceiverMask;
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

        blurred = true;
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

    void OnDisable()
    {
        // remove Non Blurred Camera
        GameObject noBlurCamera = GameObject.Find("NoBlur Camera");
        Destroy(noBlurCamera);
        
        // remove blur script from MC
        List<GameObject> cameras = new List<GameObject>(GameObject.FindGameObjectsWithTag("UICamera"));
        GameObject guiCamera2d = cameras.FirstOrDefault(c => c.layer == guiLayerIdx);
        Destroy(guiCamera2d.GetComponent<Blur>());
        
        // restore eventReceiverMask to GUI
        UICamera guiCamera2dUiCamera = guiCamera2d.GetComponent<UICamera>();
        guiCamera2dUiCamera.eventReceiverMask = eventReceiverMask;

        blurred = false;
    }
}
