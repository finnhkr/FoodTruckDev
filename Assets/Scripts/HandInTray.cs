using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class HandInTray : MonoBehaviour
{
    // public List<GameObject> foodOnTray;
    public GameObject hotDog;

    // Start is called before the first frame update
    void Start()
    {
        // foodOnTray = new List<GameObject>();
    }

    // hotdog create, by test from the bottom bread.
    public void OnCollisionEnter(Collision collision)
    {

        //if (collision.gameObject.tag == "TransformToHotDog")
        if (collision.gameObject.name == "HotDogBottom(Clone)")
        {
            GameObject bottom = collision.gameObject;
            GameObject sausage = null;
            GameObject top = null;

            if (bottom.GetComponent<XRSocketInteractor>().isSelectActive)
            {
                sausage = bottom.GetComponent<XRSocketInteractor>().interactablesSelected[0].transform.gameObject;
            }
            if (sausage.GetComponent<XRSocketInteractor>().isSelectActive)
            {
                // if hotdog top is on top of the sausage
                top = sausage.GetComponent<XRSocketInteractor>().interactablesSelected[0].transform.gameObject;

                Vector3 pos = bottom.GetComponent<Transform>().position;
                Quaternion rot = bottom.GetComponent<Transform>().rotation;

                Destroy(bottom);
                Destroy(sausage);
                Destroy(top);

                Instantiate(hotDog, pos, rot);
            }
        }

        /*
        if (collision.gameObject.tag == "Product")
        {
            foodOnTray.Add(collision.gameObject);
        }
        */
    }

    /*
    public void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Product")
        {
            foodOnTray.Remove(collision.gameObject);
        }
    }
    */
}
