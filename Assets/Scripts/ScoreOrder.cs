using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandInZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Extract object name and pass it to GameManager
        string objectName = other.gameObject.name.Replace("(Clone)", "");
        List<string> objectsToHandIn = new List<string> { objectName };

        GameManager.Instance.RecieveHandedInOrder(objectsToHandIn);

        Destroy(other.gameObject);

        Debug.Log($"Object '{objectName}' handed in and scored!");
    }
}
