using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandInZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Extract object name and pass it to GameManager
        if (other.gameObject.tag == "Customer") return;
        string objectName = other.gameObject.name.Replace("(Clone)", "");

        List<string> objectsToHandIn = new List<string> { objectName };
        Debug.LogWarning($"{other.gameObject.tag} Handed In");

        //GameManager.Instance.RecieveHandedInOrder(objectsToHandIn);

        Destroy(other.gameObject);
        Debug.LogError("D10");

        Debug.Log($"Object '{objectName}' handed in and scored!");
    }
}
