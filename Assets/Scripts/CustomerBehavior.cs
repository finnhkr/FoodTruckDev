using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class CustomerBehavior : MonoBehaviour
{
    private Animator animator;
    private NavMeshAgent navMeshAgent;
    // assigned wait point
    private GameObject assignedWaitPoint;
    // Start is called before the first frame update
    private bool hasReachedDestination = false;
    // User current state
    private enum CustomerState
    {
        InStartPoint,
        WalkingToWaitPoint,
        WaitingForOrder,
        WalkingToLeavePoint,
        WaitingInLine
    }
    private int myID;
    private bool onFirstRow = false;
    private static int customerCounter = 0;
    void Awake()
    {
        myID = customerCounter++;
        animator = GetComponent<Animator>();
    }

    private CustomerState currentState = CustomerState.InStartPoint;
    void Start()
    {
        Debug.Log($"Customer #{myID} - Start()");
        transform.name = $"{GameConstants.Instance.generateRandomNameForCustomer()}";

        // This is so weird that state switch back to instartpoint after invoke WalkToWaitPoint???
        // currentState = CustomerState.InStartPoint;
    }
    // invoke for customer walk to assigned wait point
    public void WalkToPoint(GameObject waitPoint, bool isFirstRow)
    {
        // Debug.Log($"User for {waitPoint.name} are walking to the wait point");
        if (Vector3.Distance(waitPoint.transform.position, transform.position) < 0.3f)
        {
            // prevent duplicate walking
            return;
        }
        hasReachedDestination = false;
        navMeshAgent = gameObject.GetComponent<NavMeshAgent>();
        // Debug.Log($"WaitPoint {waitPoint} {waitPoint.transform.position} {navMeshAgent.IsUnityNull()}");
        // Debug.Log($"customer #{myID} at {waitPoint.name} trigger here, distance {Vector3.Distance(waitPoint.transform.position, transform.position)}");
        animator.SetBool("IsWalking", true);
        assignedWaitPoint = waitPoint;
        navMeshAgent.ResetPath();
        navMeshAgent.SetDestination(waitPoint.transform.position);
        currentState = CustomerState.WalkingToWaitPoint;
        onFirstRow = isFirstRow;

    }
    // leave food truck
    public void LeaveFoodTruck(GameObject leavePoint)
    {
        // Debug.Log("Call Leave FoodTruck");
        hasReachedDestination = false;
        currentState = CustomerState.WalkingToLeavePoint;
        // Trigger finish order to walk animation.
        animator.SetBool("IsWalking", true);
        navMeshAgent = gameObject.GetComponent<NavMeshAgent>();
        navMeshAgent.ResetPath();
        navMeshAgent.SetDestination(leavePoint.transform.position);

        // Debug.Log($"Trigger Leave #{myID}");
    }

    // Update is called once per frame
    void Update()
    {

        if (navMeshAgent.IsUnityNull())
        {
            navMeshAgent = gameObject.GetComponent<NavMeshAgent>();
        }
        bool hasReached = !navMeshAgent.pathPending
                  && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance
                  && navMeshAgent.pathStatus != NavMeshPathStatus.PathInvalid;
        // Reach to the wait point
        // if (Vector3.Distance(navMeshAgent.destination, navMeshAgent.nextPosition) > Mathf.Epsilon)
        // {
        //      Debug.Log($"Customer #{myID} Reached {hasReached} {currentState} {hasReachedDestination} Distance {Vector3.Distance(navMeshAgent.destination, navMeshAgent.nextPosition)} {navMeshAgent.pathPending}");
        // }

        if (hasReached)
        {
            // Debug.Log($"here{navMeshAgent.remainingDistance}, {navMeshAgent.stoppingDistance}, {navMeshAgent.pathPending}");
            // only continue when customer had stopped 

            switch (currentState)
            {
                case CustomerState.WalkingToWaitPoint:
                    if (hasReachedDestination) break;
                    hasReachedDestination = true;
                    Debug.Log($"Customer #{myID} HERE");
                    // Debug.Log($"User leave {assignedWaitPoint.name}");
                    // Clear current path, to made it fully stopped;

                    // switch to idle animation for order;

                    animator.SetBool("IsWalking", false);
                    navMeshAgent.ResetPath();
                    // And invoke gama manager to generate order for the customer, by using the wait point to identify the customer.
                    // Debug.Log("Assigned" + assignedWaitPoint.name);

                    // Rotate the customer to look at the kiosk
                    Quaternion newRotation = Quaternion.Euler(0, 180, 0);
                    transform.rotation = newRotation;

                    if (onFirstRow)
                    {
                        // Only order when in firstRow;
                        // Otherwise maintain current state;
                        GameManager.Instance.GenerateOrder(assignedWaitPoint, transform.name);
                        currentState = CustomerState.WaitingForOrder;
                    }
                    else
                    {
                        currentState = CustomerState.WaitingInLine;
                        return;
                    }
                    Debug.Log($"Customer #{myID} set to Idle");

                    break;
                case CustomerState.WaitingForOrder:
                    break;
                case CustomerState.WalkingToLeavePoint:
                    // when user reached to leave point, destroy user;
                    Debug.Log($"Destroying customer {myID}");
                    navMeshAgent.ResetPath();
                    Destroy(gameObject);
                    break;
                case CustomerState.InStartPoint:
                    // Not evalutate the navMeshAgent at the starting point, otherwise it will stop at the intiial point
                    Debug.Log($"Current In startPoint {assignedWaitPoint.name}");
                    return;
                case CustomerState.WaitingInLine:
                    return;
                default:
                    break;

            }


        }

    }
}
