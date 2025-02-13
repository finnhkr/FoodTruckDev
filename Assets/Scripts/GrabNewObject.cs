using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GrabNewObject : MonoBehaviour
{
    public GameObject newObject;
    public XRInteractionManager intManager;


    // Start is called before the first frame update
    void Start()
    {
        XRSimpleInteractable tmp = GetComponent<XRSimpleInteractable>();
        tmp.selectEntered.AddListener(grabNewObject);
    }


    public void grabNewObject(SelectEnterEventArgs arg)
    {
        GameObject bottom = Instantiate(newObject, new Vector3(0, 0, -3), Quaternion.identity);

        intManager.SelectEnter(arg.interactorObject, bottom.GetComponent<XRGrabInteractable>());
    }
}
