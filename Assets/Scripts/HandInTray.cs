using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class HandInTray : MonoBehaviour
{
    public List<GameObject> foodOnTray;
    public GameObject hotDog;

    // Start is called before the first frame update
    void Start()
    {
        foodOnTray = new List<GameObject>();

        XRGrabInteractable tmp = gameObject.GetComponent<XRGrabInteractable>();
        tmp.activated.AddListener(HandingIn);
    }

    // hotdog create, by test from the bottom bread.
    public void OnCollisionEnter(Collision collision)
    {

        // It should be change if you want to add sauce to it, but not for milistone 3;
        // Generate hotdog;
        if (collision.gameObject.tag == "TransformToHotDog")
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
                Debug.LogError("D8");

                Instantiate(hotDog, pos, rot);
            }
        }


        if (collision.gameObject.tag == "Product")
        {
            foodOnTray.Add(collision.gameObject);
        }
    }

    public void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Product")
        {
            foodOnTray.Remove(collision.gameObject);
        }
    }

    public void HandingIn(ActivateEventArgs args)
    {
        List<string> tmp = new List<string>();

        foreach (GameObject obj in foodOnTray)
        {
            tmp.Add(obj.name.Replace("(Clone)", ""));
            Destroy(obj);
        }
        Destroy(gameObject);
        // Pass the name of the food on the tray to the game manager;
        GameManager.Instance.RecieveHandedInOrder(tmp);

        foodOnTray.Clear();
    }
}
