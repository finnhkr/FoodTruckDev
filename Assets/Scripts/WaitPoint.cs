using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitPoint : MonoBehaviour
{
    // If current point being occupied by a customer;
    public bool isOccupied = false;
    void Awake()
    {
        isOccupied = false;
    }
}
