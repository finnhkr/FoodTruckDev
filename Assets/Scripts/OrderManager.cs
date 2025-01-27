using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;

public class OrderManager : MonoBehaviour
{


    [System.Serializable]
    public class WaitLine
    {
        public GameObject leavePoint;
        // Head of the queue => 0, tail of the queue => n - 1
        public List<GameObject> waitPoints;

        public List<GameObject> customerQueue = new List<GameObject>();
        public void UpdatePosition()
        {
            for (int i = 0; i < customerQueue.Count; i++)
            {
                var customer = customerQueue[i];
                if (customer == null) continue;
                GameObject wp = waitPoints[i];
                var cb = customer.GetComponent<CustomerBehavior>();
                if (cb != null)
                {
                    cb.WalkToPoint(wp, i == 0);
                }
            }
        }
        public bool IsFull()
        {
            return customerQueue.Count >= waitPoints.Count;
        }
    }
    [Header("Customer Settings")]
    public List<GameObject> customerPrefabs;
    public GameObject customerContainer;

    [Header("Customer Waiting points")]
    // Don't pass transform directly for there has weird bug with it!!!
    // waitPointsRow
    public List<WaitLine> waitLines;
    [Header("Customer Start Point")]
    public GameObject startPoint;
    // Can have 3 orders simultaneously.
    int maxOrderNumber = 3;
    int currentCustomerIndex = 0;
    // is on game;
    public bool isOnGame = false;
    private Coroutine spawnCoroutine;
    // Relation between point and customer;
    private Dictionary<string, GameObject> pointCustomerRelation = new Dictionary<string, GameObject>();
    void Start()
    {
        // invoked in GameManager.cs, so don't invoke it here;
        // StartGenerate();
    }
    public void StartGenerate()
    {
        isOnGame = true;
        pointCustomerRelation.Clear();
        // Reset all occupied points;
        foreach (var line in waitLines)
        {
            line.customerQueue.Clear();
        }
        if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
        spawnCoroutine = StartCoroutine(SpawnCustomer());
        Debug.Log("Begin to generate customer");
    }
    // invoke by game manager when game end;
    public void StopGenerate()
    {
        // Stop customer generate coroutine;
        isOnGame = false;
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }
        foreach (Transform child in customerContainer.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (var line in waitLines)
        {
            line.customerQueue.Clear();
        }
        pointCustomerRelation.Clear();
    }
    // Use lock for there has some weird bug with it...
    private readonly object waitPointLock = new object();
    IEnumerator SpawnCustomer()
    {
        while (isOnGame)
        {
            // Find the unfull line
            var candidateLines = waitLines.Where(l => !l.IsFull()).ToList();
            if (candidateLines.Count > 0)
            {
                var targetLine = candidateLines[Random.Range(0, candidateLines.Count)];

                GameObject prefab = customerPrefabs[Random.Range(0, customerPrefabs.Count)];
                GameObject newCustomer = Instantiate(prefab, startPoint.transform.position, Quaternion.Euler(0, 90, 0));
                newCustomer.transform.SetParent(customerContainer.transform);

                // Add customer to back of the line
                targetLine.customerQueue.Add(newCustomer);

                // refresh line positions
                targetLine.UpdatePosition();

            }
            yield return new WaitForSeconds(Random.Range(1.5f, 2f));
        }
    }
    public void finishOrderOnPoint(string pointName)
    {
        WaitLine targetLine = null;
        int targetIndex = -1;
        foreach (var line in waitLines)
        {
            int idx = line.waitPoints.FindIndex(wp => wp != null && wp.name == pointName);
            if (idx != -1)
            {
                targetLine = line;
                targetIndex = idx;
                break;
            }
        }
        if (targetLine == null || targetIndex < 0)
        {
            Debug.LogWarning($"Can't find the line where pointName is {pointName}");
            return;
        }

        if (targetIndex != 0)
        {
            Debug.Log($"Point {pointName} is not the head of the line, but triggered finish order");
            return;
        }
        if (targetLine.customerQueue.Count <= 0)
        {
            Debug.LogWarning($"Point {pointName} belongs line is empty, but triggered finish order");
            return;
        }
        // Get the first customer
        GameObject frontCustomer = targetLine.customerQueue[0];
        if (frontCustomer != null)
        {
            var cb = frontCustomer.GetComponent<CustomerBehavior>();
            if (cb != null)
            {
                cb.LeaveFoodTruck(targetLine.leavePoint);
            }
        }
        targetLine.customerQueue.RemoveAt(0);
        targetLine.UpdatePosition();
    }

    // IEnumerator SpawnCustomer()
    // {
    //     Debug.Log("Begin Generating Customer");

    //     while (isOnGame)
    //     {
    //         // Get all freePoints here
    //         List<(GameObject rowWp, int index)> freePoints = new List<(GameObject, int)>();

    //         lock (waitPointLock)
    //         {
    //             // traverse through all waitPointRows;
    //             foreach (var row in waitLines)
    //             {
    //                 for (int i = 0; i < row.waitPoints.Count; i++)
    //                 {
    //                     var wp = row.waitPoints[i];
    //                     var wpt = wp.GetComponent<WaitPoint>();
    //                     if (wpt == null) continue;

    //                     if (!wpt.isOccupied)
    //                     {
    //                         // Set occupied
    //                         wpt.isOccupied = true;
    //                         // Log this point for generate customer later;
    //                         freePoints.Add((wp, i));
    //                     }
    //                 }
    //             }
    //         }

    //         // Generate customers here, while not influce the lock
    //         foreach (var fp in freePoints)
    //         {
    //             GameObject wp = fp.rowWp;
    //             int idx = fp.index;

    //             GameObject prefab = customerPrefabs[Random.Range(0, customerPrefabs.Count)];
    //             GameObject newCustomer = Instantiate(prefab, startPoint.transform.position, Quaternion.Euler(0, 90, 0));
    //             newCustomer.transform.SetParent(customerContainer.transform, true);
    //             newCustomer.name = newCustomer.name.Replace("(Clone)", "_" + currentCustomerIndex++);

    //             var customerBehavior = newCustomer.GetComponent<CustomerBehavior>();
    //             if (customerBehavior != null)
    //             {
    //                 customerBehavior.WalkToPoint(wp, idx == 0);
    //             }
    //             else
    //             {
    //                 // Debug.LogWarning($"Customer don't have behavior scipr {wp.name}");
    //             }
    //             pointCustomerRelation[wp.name] = newCustomer;

    //             // generate delay;
    //             yield return new WaitForSeconds(1.5f);
    //         }

    //         // wait for next round;
    //         yield return new WaitForSeconds(Random.Range(1f, 1.5f));
    //     }
    // }
    // public void finishOrderOnPoint(string pointName)
    // {
    //     lock (waitPointLock)
    //     {
    //         WaitLine targetRow = null;
    //         int targetIndex = -1;
    //         foreach (var row in waitLines)
    //         {
    //             int idx = row.waitPoints.FindIndex(w => w.name == pointName);
    //             // Find which line finished the order.
    //             // Because only the person on the first row can order
    //             // SO targetRow always be 0;
    //             if (idx != -1)
    //             {
    //                 targetRow = row;
    //                 targetIndex = idx;
    //                 break;
    //             }
    //         }
    //         if (targetRow == null || targetIndex < 0)
    //         {
    //             Debug.LogWarning($"Unable to find point {pointName}");
    //             return;
    //         }
    //         // In my logic, the customer in the front of the line can order, so for point that not in the first row is unexpected;
    //         if (targetIndex != 0)
    //         {
    //             Debug.LogWarning($"Unexpected finished order : {pointName}");
    //             return;
    //         }
    //         // Get the customer of the specific point
    //         if (pointCustomerRelation.TryGetValue(pointName, out GameObject frontCustomer))
    //         {
    //             pointCustomerRelation.Remove(pointName);
    //             if (frontCustomer != null)
    //             {
    //                 CustomerBehavior cb = frontCustomer.GetComponent<CustomerBehavior>();
    //                 // Let first row customer who finished the order leave the wait point
    //                 if (cb != null)
    //                 {
    //                     // Let customer walk to the leavePoint;
    //                     cb.transform.LookAt(targetRow.leavePoint.transform.position);
    //                     cb.LeaveFoodTruck(targetRow.leavePoint);
    //                 }
    //             }
    //         }
    //         // Customer behind move forward one by one;
    //         GameObject lastPosition = null;
    //         for (int i = targetIndex + 1; i < targetRow.waitPoints.Count; i++)
    //         {
    //             var wpBehind = targetRow.waitPoints[i].GetComponent<WaitPoint>();
    //             if (wpBehind && wpBehind.isOccupied)
    //             {

    //                 // Get the customer behind
    //                 if (pointCustomerRelation.TryGetValue(targetRow.waitPoints[i].name, out GameObject behindCustomer))
    //                 {
    //                     // Move to the previous position;
    //                     GameObject frontWpObj = targetRow.waitPoints[i - 1];

    //                     var frontWp = frontWpObj.GetComponent<WaitPoint>();
    //                     wpBehind.isOccupied = false;
    //                     frontWp.isOccupied = true;

    //                     // Update mapping
    //                     pointCustomerRelation.Remove(targetRow.waitPoints[i].name);
    //                     pointCustomerRelation[frontWpObj.name] = behindCustomer;

    //                     // Walk
    //                     var cb = behindCustomer.GetComponent<CustomerBehavior>();
    //                     if (cb != null)
    //                     {
    //                         // walk to front point;
    //                         cb.WalkToPoint(frontWpObj, i - 1 == 0);
    //                     }
    //                     // Record last originally moved position;
    //                     // lastPosition = targetRow.waitPoints[i];

    //                 }
    //             }
    //         }
    //         // If only first row has customer, we need addition logic to get if set it as unoccupied after the customer is done
    //         if (!pointCustomerRelation.TryGetValue(pointName, out GameObject firstRowCustomer))
    //         {
    //             var fr = targetRow.waitPoints[0].GetComponent<WaitPoint>();
    //             if (fr != null)
    //             {
    //                 fr.isOccupied = false;
    //             }
    //         }
    //         // Only release the last occupied point because that's the last customer leave the current standing point;
    //         // if (lastPosition != null)
    //         // {
    //         //     var lastWp = lastPosition.GetComponent<WaitPoint>();
    //         //     if (lastWp != null)
    //         //     {
    //         //         lastWp.isOccupied = false;
    //         //     }
    //         // }
    //     }
    // }

}
