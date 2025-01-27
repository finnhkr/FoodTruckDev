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
        Initialize();
    }
    public void Initialize(XRInteractionManager manager = null)
    {
        if (manager == null)
        {
            intManager = manager;
        }
        interactable = GetComponent<XRSimpleInteractable>();
        if (interactable != null)
        {
            interactable.selectEntered.AddListener(grabNewObject);
        }
    }

    public void grabNewObject(SelectEnterEventArgs arg)
    {
        GameObject bottom = Instantiate(newObject, new Vector3(0, 0, 0), Quaternion.identity);
        if (intManager == null)
        {
            Debug.LogWarning($"Int Manager lost in {newObject.gameObject.name}");
            return;
        }
        intManager.SelectEnter(arg.interactorObject, bottom.GetComponent<XRGrabInteractable>());
    }
}
