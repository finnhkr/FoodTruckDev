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
        PrepingOrder,   // Playing state, where player prepares the Food requested by order
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
    private List<GameConstants.ProductsOption> userSelection = new List<GameConstants.ProductsOption>();

    // current playmode 0->time attack 1->endless mode
    private int modeIndex;

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
        switch (state)
        {
            case gameState.Start:
                startScreen.SetActive(true);
                break;

            case gameState.CreateOrder:
                CreateRandomOrder();
                state = gameState.PrepingOrder;
                break;

            case gameState.PrepingOrder:
                break;

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
        Debug.Log($"Current Mode:{modeIndex}, Food Selections: {string.Join(", ", userSelection.Select(p => p.name))}");
        // startScreen.SetActive(false);
        // gameInfo.SetActive(true);
        // gameScene.transform.Find("Spawner").gameObject.SetActive(true);
        // state = gameState.CreateOrder;
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

    //  Set user selected food product options.
    public void SetUserSelection(List<GameConstants.ProductsOption> selections)
    {
        userSelection = selections;
        Debug.Log("User Selections:" + string.Join(",", userSelection));
    }

    public void SetPlayMode(int currentModeIndex)
    {
        modeIndex = currentModeIndex;
    }

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