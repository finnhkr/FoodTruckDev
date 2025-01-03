using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrderManager : MonoBehaviour
{
    // Start is called before the first frame update
    // Display for user sequence, and details.
    public GameObject orderContainer;
    // Details for every order in the order container.
    public GameObject orderDetailContainer;
    public List<GameObject> customerPrefabs;

    public List<GameObject> waitPoints;
    // Can have 3 orders simultaneously.
    int maxOrderNumber = 3;
    void Start()
    {
        // When Start called, we begin to generate orders
    }

    // Update is called once per frame
    void Update()
    {

    }
}
