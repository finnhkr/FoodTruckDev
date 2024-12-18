using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GameManager : MonoBehaviour
{
    #region Singleton
    private static GameManager instance;
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                Debug.LogError("GameManager is null");
            }

            return instance;
        }
    }

    private void Awake()
    {
        if (instance)
            Destroy(gameObject);
        else
            instance = this;

        DontDestroyOnLoad(this);
    }
    #endregion

    private enum gameState
    {
        Start,          // Start screen 
        CreateOrder,    // order gets randomly chosen
        PrepingOrder,   // Playing state, where player prepares the Food requested by order
        EvaluateOrder,  // Triggered by handing in an order from the player, evaluates success of player and further game state
        End,            // End screen after failing
    }

    private gameState state;



    // List of possible orders set in Asstes/Orders - ScriptableObjects
    // If new order added in said folder, order also needs to be in Scene -> GameManager -> Component GameManager Script -> orders
    [SerializeField]
    private List<Order> orders = new List<Order>();


    // Current order randomly set upon game state: createOrder
    private Order currentOrder;

    // List of GameObjects turned in, set by the recieveHandedInOrderFunction
    private List<GameObject> handedInFood;

    // Amount of orders succesfully turned in
    private int score = 0;


    // =====================================================================
    // =====================================================================


    // Start is called before the first frame update
    void Start()
    {
        state = gameState.Start;
    }

    // Update is called once per frame
    void Update()
    {
        switch (state)
        {
            case gameState.Start:
                break;

            case gameState.CreateOrder:
                currentOrder = CreateRandomOrder();
                state = gameState.PrepingOrder;
                break;

            case gameState.PrepingOrder:
                break;

            case gameState.EvaluateOrder:
                EvaluateOrder();
                break;

            case gameState.End:
                break;

            default:
                Debug.Log("ERROR: Unknown game state: " + state);
                break;
        }
    }

    public void EvaluateOrder()
    {
        // if condition not fully functional because order in lists could differ
        if (currentOrder.content == handedInFood)
        {
            score += 1;
            state = gameState.CreateOrder;
        }
        else
        {
            score = 0;
            handedInFood = null;
            currentOrder = null;

            state = gameState.End;
        }
    }

    public void RecieveHandedInOrder(List<GameObject> tmp)
    {
        handedInFood = tmp;

        state = gameState.EvaluateOrder;
    }


    public Order CreateRandomOrder()
    {
        Order order = orders[Random.Range(0, orders.Count)];

        return order;
    }

    public void StartGame()
    {
        state = gameState.CreateOrder;
    }

    public void ResetGame()
    {
        state = gameState.Start;
    }
}