using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GrabNewObject : MonoBehaviour
{
    public GameObject newObject;
    public XRInteractionManager intManager;
    private XRSimpleInteractable interactable;


    // Start is called before the first frame update
    void Start()
    {
        XRSimpleInteractable tmp = GetComponent<XRSimpleInteractable>();
        tmp.selectEntered.AddListener(grabNewObject);

        //Initialize();
    }

    /*
    public void Initialize(XRInteractionManager manager = null)
    {
        if (manager != null)
        {
            intManager = manager;
        }
        if (interactable != null)
        {
            // Remove the listener first to prevent multiple listeners if it already realized;
            interactable.selectEntered.RemoveListener(grabNewObject);
        }
        interactable = GetComponent<XRSimpleInteractable>();
        if (interactable != null)
        {

            interactable.selectEntered.AddListener(grabNewObject);
        }
    }


    private void OnDisable()
    {
        if (interactable != null)
            interactable.selectEntered.RemoveListener(grabNewObject);
    }
    */

    public void grabNewObject(SelectEnterEventArgs arg)
    {
        GameObject bottom = Instantiate(newObject, new Vector3(0, 0, -3), Quaternion.identity);

        /*
        if (intManager == null)
        {
            Debug.LogWarning($"Int Manager lost in {newObject.gameObject.name}");
            return;
        }
        */


        intManager.SelectEnter(arg.interactorObject, bottom.GetComponent<XRGrabInteractable>());
    }
}
