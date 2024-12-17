using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GameManager : MonoBehaviour
{

    public List<GameObject> currentOrder;
    public GameObject bell;

    // Start is called before the first frame update
    void Start()
    {
        currentOrder = new List<GameObject>();

        XRGrabInteractable tmp = bell.GetComponent<XRGrabInteractable>();
        tmp.activated.AddListener(handInOrder);
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void handInOrder(ActivateEventArgs args)
    {
        foreach (GameObject order in currentOrder) {
            Destroy(order);
        }
    }
}
