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

        // This is so weird that state switch back to instartpoint after invoke WalkToWaitPoint???
        // currentState = CustomerState.InStartPoint;
    }
    // invoke for customer walk to assigned wait point
    public void WalkToWaitPoint(GameObject waitPoint, bool isFirstRow)
    {
        // Debug.Log($"User for {waitPoint.name} are walking to the wait point");
        navMeshAgent = gameObject.GetComponent<NavMeshAgent>();
        // Debug.Log($"WaitPoint {waitPoint} {waitPoint.transform.position} {navMeshAgent.IsUnityNull()}");
        navMeshAgent.SetDestination(waitPoint.transform.position);
        animator.SetTrigger("FinishOrder");
        assignedWaitPoint = waitPoint;
        currentState = CustomerState.WalkingToWaitPoint;
        onFirstRow = isFirstRow;
    }
    // leave food truck
    public void LeaveFoodTruck(GameObject leavePoint)
    {
        Debug.Log("Call Leave FoodTruck");
        currentState = CustomerState.WalkingToLeavePoint;
        // Trigger finish order to walk animation.
        animator.SetTrigger("FinishOrder");
        navMeshAgent = gameObject.GetComponent<NavMeshAgent>();
        navMeshAgent.SetDestination(leavePoint.transform.position);
    }

    // Update is called once per frame
    private void FixedUpdate()
    {

        if (navMeshAgent.IsUnityNull())
        {
            navMeshAgent = gameObject.GetComponent<NavMeshAgent>();
        }
        // Reach to the wait point
        if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance && !navMeshAgent.pathPending)
        {
            // Debug.Log($"here{navMeshAgent.remainingDistance}, {navMeshAgent.stoppingDistance}, {navMeshAgent.pathPending}");
            // only continue when customer had stopped 
            if (!navMeshAgent.hasPath || navMeshAgent.velocity.sqrMagnitude < 0.01f)
            {
                switch (currentState)
                {
                    case CustomerState.WalkingToWaitPoint:
                        // Debug.Log($"User leave {assignedWaitPoint.name}");
                        // Clear current path, to made it fully stopped;
                        navMeshAgent.ResetPath();
                        // switch to idle animation for order;
                        animator.SetTrigger("ReachStandPoint");
                        // And invoke gama manager to generate order for the customer, by using the wait point to identify the customer.
                        // Debug.Log("Assigned" + assignedWaitPoint.name);

                        if (onFirstRow)
                        {
                            // Only order when in firstRow;
                            // Otherwise maintain current state;
                            GameManager.Instance.GenerateOrder(assignedWaitPoint);
                            currentState = CustomerState.WaitingForOrder;
                        } else {
                            currentState = CustomerState.WaitingInLine;
                            return;
                        }

                        // Rotate the customer to look at the kiosk
                        Quaternion newRotation = Quaternion.Euler(0, 180, 0);
                        transform.rotation = newRotation;

                        break;
                    case CustomerState.WaitingForOrder:
                        break;
                    case CustomerState.WalkingToLeavePoint:
                        // when user reached to leave point, destroy user;
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
}
