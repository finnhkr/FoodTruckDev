using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class OrderManager : MonoBehaviour
{

    [Header("Customer Settings")]
    public List<GameObject> customerPrefabs;
    public GameObject customerContainer;

    [Header("Customer Waiting points")]
    // Don't pass transform directly for there has weird bug with it!!!
    public List<GameObject> waitPoints;
    [Header("Customer Exit Point")]
    public GameObject exitPoint;
    [Header("Customer Start Point")]
    public GameObject startPoint;
    // Can have 3 orders simultaneously.
    int maxOrderNumber = 3;
    int currentCustomerIndex = 0;
    // is on game;
    public bool isOnGame = false;
    private Coroutine customerCoroutine;
    // Relation between point and customer;
    private Dictionary<string, GameObject> pointCustomerRelation = new Dictionary<string, GameObject>();
    void Start()
    {
        isOnGame = true;
        // SpawnCustomer1();
        customerCoroutine = StartCoroutine(SpawnCustomer());
    }
    // invoke by game manager when game end;
    public void StopGenerate()
    {
        // Stop customer generate coroutine;
        isOnGame = false;
        StopCoroutine(customerCoroutine);
        foreach (GameObject child in customerContainer.transform)
        {
            Destroy(child);
        }
    }
    // Use lock for there has some weird bug with it...
    private readonly object waitPointLock = new object();

    IEnumerator SpawnCustomer()
    {
        Debug.Log("Begin Generating Customer");
        while (isOnGame)
        {
            GameObject availablePoint;
            // Lock waiting 
            lock (waitPointLock)
            {
                availablePoint = waitPoints.Find(point => !point.GetComponent<WaitPoint>().isOccupied);
                if (availablePoint != null)
                {
                    var waitPointComponent = availablePoint.GetComponent<WaitPoint>();
                    if (waitPointComponent == null)
                    {
                        Debug.LogError($"{availablePoint.name} does not have a WaitPoint component!");
                        yield return new WaitForSeconds(1f);
                        continue;
                    }
                    // // Set occupied
                    waitPointComponent.isOccupied = true;
                    // Debug.Log($"{availablePoint.name} isOccupied set to true");

                    // // Print out update
                    // Debug.Log($"WaitPoint States After Update: {string.Join(", ", waitPoints.Select(p => p.name + " - " + p.GetComponent<WaitPoint>().isOccupied))}");
                }
            }
            if (availablePoint != null)
            {
                // Generate New customer.
                GameObject customer = customerPrefabs[Random.Range(0, customerPrefabs.Count)];
                GameObject customerInstance = Instantiate(customer, startPoint.transform.position, Quaternion.Euler(0, 90, 0));
                customerInstance.transform.SetParent(customerContainer.transform);
                customerInstance.name = customerInstance.name.Replace("(Clone)", "_" + currentCustomerIndex++);
                // assign waiting point.
                CustomerBehavior customerBehavior = customerInstance.GetComponent<CustomerBehavior>();
                if (customerBehavior != null)
                {
                    customerBehavior.WalkToWaitPoint(availablePoint);
                }
                // Add relations to pointCustomerRelation
                pointCustomerRelation.Add(availablePoint.name, customerInstance);
            }
            // Generate next customer after 1~1.5 seconds.
            yield return new WaitForSeconds(Random.Range(1f, 1.5f));
        }
    }
    public void finishOrderOnPoint(string pointName)
    {
        GameObject point = waitPoints.Find(p => p.name == pointName);
        // trigger leave animation first;
        if (pointCustomerRelation.ContainsKey(pointName))
        {
            GameObject customer = pointCustomerRelation[pointName];
            var customerBehavior = customer.GetComponent<CustomerBehavior>();
            if (customerBehavior != null)
            {
                customerBehavior.LeaveFoodTruck(point);
            }
        }
        // Then set that point as not occupied.
        point.GetComponent<WaitPoint>().isOccupied = false;
    }

}
