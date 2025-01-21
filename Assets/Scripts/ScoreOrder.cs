using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandInZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        GameManager.Instance.HandingIn(other);

        //Destroy(other.gameObject);

        Debug.Log($"Object '{objectName}' handed in and scored!");
    }
}
