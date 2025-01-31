using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class HandInZone : MonoBehaviour
{
    public GameObject hotDog;

    private void OnTriggerEnter(Collider other)
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
        else
        {
            Destroy(other.gameObject);
        }
    }
}
