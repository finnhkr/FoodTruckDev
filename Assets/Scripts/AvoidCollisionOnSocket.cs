using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class AvoidCollisionOnSocket : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void Awake()
    {
        XRSocketInteractor socket = GetComponent<XRSocketInteractor>();
        socket.selectEntered.AddListener(OnSelectEntered);
        socket.selectExited.AddListener(OnSelectExited);
    }

    public void OnSelectEntered(SelectEnterEventArgs args)
    {
        var tmp = args.interactableObject.transform.gameObject;

        Physics.IgnoreCollision(gameObject.GetComponent<Collider>(), tmp.GetComponent<Collider>(), true);
    }

    public void OnSelectExited(SelectExitEventArgs args)
    {
        var tmp = args.interactableObject.transform.gameObject;

        Physics.IgnoreCollision(gameObject.GetComponent<Collider>(), tmp.GetComponent<Collider>(), false);
    }
}
