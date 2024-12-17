using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class HandInOrder : MonoBehaviour
{

    public List<GameObject> order;
    public GameObject hotDog;

    // Start is called before the first frame update
    void Start()
    {
        order = new List<GameObject>();

        XRSimpleInteractable tmp = GetComponent<XRSimpleInteractable>();
        tmp.activated.AddListener(handingIn);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnCollisionEnter(Collision collision)
    {
        
        if (collision.gameObject.tag == "TransformToHotDog")
        {
            order.Add(collision.gameObject);

            GameObject bottom = collision.gameObject;
            GameObject sausage = null;
            GameObject top = null;

            if (bottom.GetComponent<XRSocketInteractor>().isSelectActive)
            {
                sausage = bottom.GetComponent<XRSocketInteractor>().interactablesSelected[0].transform.gameObject;
            }
            if (sausage.GetComponent<XRSocketInteractor>().isSelectActive)
            {
                order.Remove(collision.gameObject);
                top = sausage.GetComponent<XRSocketInteractor>().interactablesSelected[0].transform.gameObject;

                Vector3 pos = bottom.GetComponent<Transform>().position;
                Quaternion rot = bottom.GetComponent<Transform>().rotation;

                Destroy(bottom);
                Destroy(sausage);
                Destroy(top);

                Instantiate(hotDog, pos, rot);
            }
        }
        

        if (collision.gameObject.tag == "Product")
        {
            order.Add(collision.gameObject);
        }
    }

    public void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag != "notEditable")
        {
            order.Remove(collision.gameObject);
        }
    }

    public void handingIn(ActivateEventArgs args)
    {
        foreach (GameObject obj in order)
        {
            Destroy(obj);
        }
        order.Clear();
    }
}
