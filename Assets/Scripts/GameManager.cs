using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem.XR;
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
        // PrepingOrder,   // Playing state, where player prepares the Food requested by order
        EvaluateOrder,  // Triggered by handing in an order from the player, evaluates success of player and further game state
        End,            // End screen after failing
    }

    private gameState state;



    [SerializeField]
    private GameObject ui;

    [SerializeField]
    private GameObject gameScene;

    private GameObject startScreen;
    private GameObject endScreen;
    private GameObject gameInfo;

    public GameObject player;
    // Timer for generate orders
    float orderTimer = 0f;

    private List<string> hotDogs = new List<string>
        {
        "HotDog"
        };

    private List<string> drinks = new List<string>
    {
        "YellowDrink", "GreenDrink", "BlueDrink"
    };

    private List<string> currentOrder;

    private List<string> handedInFood;

    private int score = 0;

    // The chosen item will be used to set up the cooking challenge
    private List<GameConstants.ProductsOption> foodList = new List<GameConstants.ProductsOption>();

    public List<GameConstants.ProductsOption> FoodList
    {
        get => foodList;
        set
        {
            foodList = value ?? new List<GameConstants.ProductsOption>();
            Debug.Log("User Selections:" + string.Join(",", foodList.Select(p => p.name)));
        }
    }

    // current playmode 0->time attack 1->endless mode
    private int currentMode;
    public int Playmode
    {
        get => currentMode;
        set
        {
            if (value < 0)
            {
                Debug.LogWarning("Invalid Playmode value, set to time attack (0) instead");
                currentMode = GameConstants.MODE_TIMEATTACK;
            }
            else
            {
                currentMode = value;
                Debug.Log($"PlayMode setted to: {(currentMode == GameConstants.MODE_TIMEATTACK ? "Time Attack" : "Endless Mode")}");
            }

        }
    }

    // =====================================================================
    // =====================================================================


    // Start is called before the first frame update
    void Start()
    {
        state = gameState.Start;

        currentOrder = new List<string>();

        startScreen = ui.transform.Find("StartScreen").gameObject;
        endScreen = ui.transform.Find("EndScreen").gameObject;
        gameInfo = ui.transform.Find("GameInfo").gameObject;

        endScreen.SetActive(false);
        gameInfo.SetActive(false);
        gameScene.transform.Find("Spawner").gameObject.SetActive(false);

        // disable User moving for start, end and etc. -> like settings?
        LockPlayerMovement();
    }

    // Update is called once per frame
    void Update()
    {
        orderTimer += Time.deltaTime;
        switch (state)
        {
            case gameState.Start:
                startScreen.SetActive(true);
                break;

            case gameState.CreateOrder:
                // Use fixedUpdate instead.
                // CreateRandomOrder();
                // state = gameState.PrepingOrder;
                break;
            // case gameState.PrepingOrder:
            //     break;

            case gameState.EvaluateOrder:
                EvaluateOrder();
                break;

            case gameState.End:
                endScreen.SetActive(true);
                gameScene.transform.Find("Spawner").gameObject.SetActive(false);
                break;

            default:
                Debug.Log("ERROR: Unknown game state: " + state);
                break;
        }
    }
    public void EvaluateOrder()
    {
        if (CompareOrders())
        {
            score += 1;
            state = gameState.CreateOrder;
        }
        else
        {
            gameInfo.SetActive(false);
            state = gameState.End;
        }
    }

    public void RecieveHandedInOrder(List<string> tmp)
    {
        handedInFood = tmp;

        state = gameState.EvaluateOrder;
    }

    private bool CompareOrders()
    {
        List<string> currentOrderCopy = currentOrder;
        List<string> handedInFoodCopy = handedInFood;

        foreach (string i in currentOrder)
        {
            foreach (string j in handedInFoodCopy)
            {
                if (i.Equals(j))
                {
                    handedInFoodCopy.Remove(j);
                    currentOrderCopy.Remove(i);
                }
                break;
            }
        }

        if (handedInFoodCopy.Any())
        {
            return false;
        }
        if (currentOrderCopy.Any())
        {
            return false;
        }

        return true;
    }

    public void CreateRandomOrder()
    {
        currentOrder.Clear();

        currentOrder.Add(hotDogs[Random.Range(0, hotDogs.Count)]);
        currentOrder.Add(drinks[Random.Range(0, drinks.Count)]);
    }

    public void StartGame()
    {
        // Debug for verify if correct params are passed to game manager.
        Debug.Log($"Current Mode:{(currentMode == GameConstants.MODE_TIMEATTACK ? "Time Attack" : "EndlessMode")}, Food Selections: {string.Join(", ", foodList.Select(p => p.name))}");
        // Close startScreen
        startScreen.SetActive(false);
        gameInfo.SetActive(true);
        // Active Spawner and arrange the ingredients there by invoke the function SpawnIngredients
        GameObject spawner = gameScene.transform.Find("Spawner").gameObject;
        spawner.SetActive(true);
        spawner.GetComponent<Spawner>().SpawnIngredients(foodList);
        // unlock player Movement for game playing.
        UnlockPlayerMovement();
        state = gameState.CreateOrder;
    }

    public void ResetGame()
    {
        score = 0;
        currentOrder.Clear();
        handedInFood.Clear();

        endScreen.SetActive(false);
        state = gameState.Start;
    }

    public int GetScore()
    {
        return score;
    }

    public List<string> GetCurrentOrder()
    {
        return currentOrder;
    }
    // Lock for fixed option screen like start game screen, score screen, etc.

    public void LockPlayerMovement()
    {
        player.GetComponent<TrackedPoseDriver>().enabled = false;
        player.transform.SetLocalPositionAndRotation(new Vector3(-0.22f, 2, -0.43f), Quaternion.Euler(6, 0, 0));
    }
    public void UnlockPlayerMovement()
    {
        player.GetComponent<TrackedPoseDriver>().enabled = true;

    }
}