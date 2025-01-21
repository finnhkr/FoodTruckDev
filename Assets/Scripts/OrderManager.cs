using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class OrderManager : MonoBehaviour
{


    [System.Serializable]
    public class waitPointRow
    {
        public GameObject leavePoint;
        // Head of the queue => 0, tail of the queue => n - 1
        public List<GameObject> waitPoints;
    }
    [Header("Customer Settings")]
    public List<GameObject> customerPrefabs;
    public GameObject customerContainer;

    [Header("Customer Waiting points")]
    // Don't pass transform directly for there has weird bug with it!!!
    // waitPointsRow
    public List<waitPointRow> waitPoints;
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
        // invoked in GameManager.cs, so don't invoke it here;
        // StartGenerate();
    }
    public void StartGenerate()
    {
        isOnGame = true;
        pointCustomerRelation.Clear();
        // Reset all occupied points;
        foreach (var row in waitPoints)
        {
            foreach (var wp in row.waitPoints)
            {
                var wpt = wp.GetComponent<WaitPoint>();
                if (wpt) wpt.isOccupied = false;
            }
        }
        customerCoroutine = StartCoroutine(SpawnCustomer());
        Debug.Log("Begin to generate customer");
    }
    // invoke by game manager when game end;
    public void StopGenerate()
    {
        // Stop customer generate coroutine;
        isOnGame = false;
        if (customerCoroutine != null)
        {
            StopCoroutine(customerCoroutine);
        }
        foreach (Transform child in customerContainer.transform)
        {
            Destroy(child.gameObject);
        }
        pointCustomerRelation.Clear();
    }
    // Use lock for there has some weird bug with it...
    private readonly object waitPointLock = new object();

    IEnumerator SpawnCustomer()
    {
        Debug.Log("Begin Generating Customer");

        while (isOnGame)
        {
            // Get all freePoints here
            List<(GameObject rowWp, int index)> freePoints = new List<(GameObject, int)>();

            lock (waitPointLock)
            {
                // traverse through all waitPointRows;
                foreach (var row in waitPoints)
                {
                    for (int i = 0; i < row.waitPoints.Count; i++)
                    {
                        var wp = row.waitPoints[i];
                        var wpt = wp.GetComponent<WaitPoint>();
                        if (wpt == null) continue;

                        if (!wpt.isOccupied)
                        {
                            // Set occupied
                            wpt.isOccupied = true;

                            // Log this point for generate customer later;
                            freePoints.Add((wp, i));
                        }
                    }
                }
            }

            // Generate customers here, while not influce the lock
            foreach (var fp in freePoints)
            {
                GameObject wp = fp.rowWp;
                int idx = fp.index;

                GameObject prefab = customerPrefabs[Random.Range(0, customerPrefabs.Count)];
                GameObject newCustomer = Instantiate(prefab, startPoint.transform.position, Quaternion.Euler(0, 90, 0));
                newCustomer.transform.SetParent(customerContainer.transform, true);
                newCustomer.name = newCustomer.name.Replace("(Clone)", "_" + currentCustomerIndex++);

                var customerBehavior = newCustomer.GetComponent<CustomerBehavior>();
                if (customerBehavior != null)
                {
                    customerBehavior.WalkToWaitPoint(wp, idx == 0);
                }
                pointCustomerRelation[wp.name] = newCustomer;

                // generate delay;
                yield return new WaitForSeconds(1f);
            }

            // wait for next round;
            yield return new WaitForSeconds(Random.Range(1f, 1.5f));
        }
    }
    public void finishOrderOnPoint(string pointName)
    {
        // Find which row finished, I'm getting confused about my current code, so just use another way to find which point is finished;
        waitPointRow targetRow = null;
        int targetIndex = -1;
        foreach (var row in waitPoints)
        {
            int idx = row.waitPoints.FindIndex(w => w.name == pointName);
            if (idx != -1)
            {
                targetRow = row;
                targetIndex = idx;
                break;
            }
        }
        if (targetRow == null || targetIndex < 0)
        {
            Debug.LogWarning($"Unable to find point {pointName}");
            return;
        }
        // In my logic, the customer in the front of the line can order, so for point that not in the first row is unexpected;
        if (targetIndex != 0)
        {
            Debug.LogWarning($"Unexpected finished order : {pointName}");
            return;
        }
        // Get the customer of the specific point
        if (pointCustomerRelation.TryGetValue(pointName, out GameObject frontCustomer))
        {
            pointCustomerRelation.Remove(pointName);
            if (frontCustomer != null)
            {
                CustomerBehavior cb = frontCustomer.GetComponent<CustomerBehavior>();
                if (cb != null)
                {
                    // Let customer walk to the leavePoint;
                    cb.transform.LookAt(targetRow.leavePoint.transform.position);
                    cb.LeaveFoodTruck(targetRow.leavePoint);
                }
            }
        }
        // Customer behind move forward one by one;
        GameObject lastPosition = null;
        for (int i = targetIndex + 1; i < targetRow.waitPoints.Count; i++)
        {
            var wpBehind = targetRow.waitPoints[i].GetComponent<WaitPoint>();
            if (wpBehind && wpBehind.isOccupied)
            {

                // Get the customer behind
                if (pointCustomerRelation.TryGetValue(targetRow.waitPoints[i].name, out GameObject behindCustomer))
                {
                    // Move to the previous position;
                    GameObject frontWpObj = targetRow.waitPoints[i - 1];
                    // Save current point;
                    lastPosition = targetRow.waitPoints[i];

                    // Update mapping
                    pointCustomerRelation.Remove(targetRow.waitPoints[i].name);
                    pointCustomerRelation[frontWpObj.name] = behindCustomer;

                    // Walk
                    var cb = behindCustomer.GetComponent<CustomerBehavior>();
                    if (cb != null)
                    {
                        cb.WalkToWaitPoint(frontWpObj, i - 1 == 0);
                    }

                }
            }
        }
        // Only release the last occupied point because that's the last customer leave the current standing point;
        if (lastPosition != null)
        {
            var lastWp = lastPosition.GetComponent<WaitPoint>();
            if (lastWp != null)
            {
                lastWp.isOccupied = false;
            }
        }

        //        GameObject point = waitPoints.Find(p => p.name == pointName);
        //        // trigger leave animation first;
        //        if (pointCustomerRelation.ContainsKey(pointName))
        //        {
        //            GameObject customer = pointCustomerRelation[pointName];
        //            // weird here...
        //            if (customer.IsUnityNull())
        //            {
        //                pointCustomerRelation.Remove(pointName);
        //                point.GetComponent<WaitPoint>().isOccupied = false;
        //                return;
        //            }
        //            var customerBehavior = customer.GetComponent<CustomerBehavior>();
        //            if (customerBehavior != null)
        //            {
        //                pointCustomerRelation.Remove(pointName);
        //                customerBehavior.LeaveFoodTruck(point);
        //            }
        //        }
        //        // Then set that point as not occupied.
        //        point.GetComponent<WaitPoint>().isOccupied = false;
    }

}
