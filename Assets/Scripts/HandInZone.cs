using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using static Unity.Burst.Intrinsics.X86.Avx;

public class HandInZone : MonoBehaviour
{
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Customer")
        {
            return;
        }

        if (other.gameObject.tag == "Product")
        {
            string objectName = other.gameObject.name.Replace("(Clone)", "");

            List<string> objectsToHandIn = new List<string> { objectName };
            Debug.LogWarning($"{other.gameObject.tag} Handed In");

            GameManager.Instance.RecieveHandedInOrder(objectsToHandIn);

            Destroy(other.gameObject);

            Debug.Log($"Object '{objectName}' handed in and scored!");
        }
        else if (other.gameObject.name == "HotDogBottom(Clone)")
        {
            GameObject bottom = other.gameObject;
            GameObject sausage = null;
            GameObject top = null;

            if (bottom.GetComponent<XRSocketInteractor>().isSelectActive)
            {
                sausage = bottom.GetComponent<XRSocketInteractor>().interactablesSelected[0].transform.gameObject;
            }
            if (sausage.GetComponent<XRSocketInteractor>().isSelectActive)
            {
                top = sausage.GetComponent<XRSocketInteractor>().interactablesSelected[0].transform.gameObject;
            }
            if (sausage != null || top != null)
            {
                GameManager.Instance.RecieveHandedInOrder(new List<string> { "HotDog" });
                Destroy(bottom);
                Destroy(sausage);
                Destroy(top);
            }
        }
        else
        {
            Physics.IgnoreCollision(gameObject.GetComponent<Collider>(), other.gameObject.GetComponent<Collider>(), true);
        }
    }
}
