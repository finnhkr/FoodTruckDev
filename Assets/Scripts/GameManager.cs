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
using TMPro;
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
    // This will be reset in time mode for countdown.
    float countdownTime = 0f;

    private List<string> hotDogs = new List<string>
        {
        "HotDog"
        };

    private List<string> drinks = new List<string>
    {
        "YellowDrink", "GreenDrink", "BlueDrink"
    };

    private List<string> currentOrder;
    // latest one for currentOrders.
    public class OrderList
    {
        // Current Order List for displaying the order to the score board;
        public List<GameConstants.orderInfo> currentOrderList;
        // remainingOrderList for compare if orders finished;
        public List<GameConstants.orderInfo> remainingOrderList;
    }
    // private List<GameConstants.orderInfo> orderList = new List<GameConstants.orderInfo>();
    // Current orders - include order list for display and remaining for compare;
    private OrderList orderListInfo = new OrderList();
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
    private int userScore = 0;
    public int UserScore
    {
        get => userScore;
        set
        {
            userScore = value;
            TextMeshProUGUI scoreText = gameInfo.transform.Find("Info/Score/currentScore").GetComponent<TextMeshProUGUI>();
            if (!scoreText.IsUnityNull())
            {
                scoreText.text = userScore.ToString();
            }
        }
    }
    // Start is called before the first frame update
    // Display for user sequence, and details.
    [Header("Order Settings")]
    public GameObject orderContainer;
    // Details for every order in the order container.
    public GameObject orderDetailContainer;
    // UI for display current Score;
    public TMP_Text scoreText;

    // order size control -  adjust according to increasing difficulty for milestone 4, but fixed it now.
    public int minOrderSize = 1;
    public int maxOrderSize = 2;
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

        returnStartScreen();
        // endScreen.SetActive(false);
        // gameInfo.SetActive(false);
        // gameScene.transform.Find("Spawner").gameObject.SetActive(false);

        // // disable User moving for start, end and etc. -> like settings?
        // LockPlayerMovement();

    }
    public void returnStartScreen()
    {
        startScreen.SetActive(true);
        gameInfo.SetActive(false);
        endScreen.SetActive(false);
        gameScene.transform.Find("Spawner").gameObject.SetActive(false);
        LockPlayerMovement();


    }

    // Update is called once per frame
    void Update()
    {

        switch (state)
        {
            // case gameState.Start:
            //     startScreen.SetActive(true);
            //     break;

            case gameState.CreateOrder:
                // Use fixedUpdate instead.
                // CreateRandomOrder();
                // state = gameState.PrepingOrder;
                break;
            // case gameState.PrepingOrder:
            //     break;

            // case gameState.EvaluateOrder:
            //     EvaluateOrder();
            //     break;

            // case gameState.End:
            //     endScreen.SetActive(true);
            //     gameScene.transform.Find("Spawner").gameObject.SetActive(false);
            //     break;

            default:
                // Debug.Log("ERROR: Unknown game state: " + state);
                break;
        }
    }
    private void FixedUpdate()
    {

        if (state == gameState.CreateOrder)
        {
            // Debug.Log($"state {state} {state == gameState.CreateOrder} mode {currentMode}");
            if (currentMode == GameConstants.MODE_TIMEATTACK)
            {
                countdownTime -= Time.deltaTime;
                // Only judge if end game when time consumed in Time attack mode;
                if (countdownTime > 0)
                {
                    TextMeshProUGUI clock = gameInfo.transform.Find("Info/Clock/countDown").GetComponent<TextMeshProUGUI>();
                    if (!clock.IsUnityNull())
                    {
                        int minutes = (int)(countdownTime / 60);
                        int seconds = (int)(countdownTime % 60);
                        clock.text = $"{minutes}: {seconds:D2}";
                    }
                }
                else
                {
                    // EndGame when time consumed
                    TextMeshProUGUI clock = gameInfo.transform.Find("Info/Clock/countDown").GetComponent<TextMeshProUGUI>();
                    if (!clock.IsUnityNull())
                    {
                        clock.text = "0:00";
                    }
                    state = gameState.End;
                    EndGame();
                }
            }
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
    // Get food name list for submit.
    public void RecieveHandedInOrder(List<string> tmp)
    {
        handedInFood = tmp;

        // invoke to process current submit order;
        EvaluateFood(tmp);



        // state = gameState.EvaluateOrder;
    }
    // Evaluate food for submission to get score;
    public void EvaluateFood(List<string> tmp)
    {
        // Temporary list to store orders that need removal
        List<GameConstants.orderInfo> ordersToRemove = new List<GameConstants.orderInfo>();

        // Traves remaining order list and record it.
        foreach (var order in orderListInfo.remainingOrderList)
        {
            foreach (string submitFood in tmp)
            {
                if (order.products.ContainsKey(submitFood))
                {
                    order.products[submitFood] -= 1;
                    // If current food request be fulfilled, then remove it.
                    if (order.products[submitFood] <= 0)
                    {
                        order.products.Remove(submitFood);
                    }
                }
            }

            // If order finished, record to remove later;
            if (order.products.Count == 0)
            {
                ordersToRemove.Add(order);
            }
        }
        // execute remove and leave animation together.
        foreach (var finishedOrder in ordersToRemove)
        {
            string pointName = finishedOrder.waitPointName;
            UserScore += finishedOrder.difficulty;

            // Remove order requests from orderListInfo
            orderListInfo.remainingOrderList.Remove(finishedOrder);
            orderListInfo.currentOrderList.RemoveAll(od => od.waitPointName == pointName);

            // And then invoke to request for new customer and let current request corresponding
            // customer leave wait point to exit point.
            gameInfo.GetComponent<OrderManager>().finishOrderOnPoint(pointName);
        }
        // Update board
        GenerateOrderBoard();
    }
    // Change contents on the order Board;
    private void GenerateOrderBoard()
    {
        // Delete all current order list;
        GameObject boardContainer = gameInfo.transform.Find("OrderBoard").Find("Viewport/Content").gameObject;
        GameConstants.Instance.ClearChildren(boardContainer);
        // traverse through current order to generate order board;
        // Debug.Log($"Is orderList currentOrder emtpy {orderListInfo.currentOrderList.IsUnityNull()}");
        orderListInfo.currentOrderList.ForEach(currentOrder =>
        {
            // Instantiate orderContiner first, which include wait Point info
            GameObject orderInfo = Instantiate(orderContainer);
            orderInfo.transform.SetParent(boardContainer.transform, false);
            // orderInfo.transform.localScale = Vector3.one;
            orderInfo.name = orderInfo.name.Replace("(Clone)", "");
            orderInfo.transform.Find("waitPoint").GetComponent<TextMeshProUGUI>().text = currentOrder.waitPointName;
            foreach (KeyValuePair<string, int> kvp in currentOrder.products)
            {
                GameObject orderDetail = Instantiate(orderDetailContainer);
                orderDetail.transform.SetParent(orderInfo.transform, false);
                // Set Food name
                TextMeshProUGUI foodNameText = orderDetail.transform.Find("Infos/food").GetComponent<TextMeshProUGUI>();
                foodNameText.text = kvp.Key;
                // Set total count;
                GameObject countObj = orderDetail.transform.Find("Infos/count").gameObject;
                TextMeshProUGUI countText = countObj.GetComponent<TextMeshProUGUI>();
                GameObject remainCountObj = orderDetail.transform.Find("Infos/remainCount").gameObject;
                countText.text = "*" + kvp.Value.ToString();
                // Get current remaining count to determining if we need to set delete line to food name and count
                // or hide remain
                // Find current remaining Count;
                List<GameConstants.orderInfo> tmp = orderListInfo.remainingOrderList.FindAll(p => p.waitPointName == currentOrder.waitPointName);
                if (tmp.Count == 0)
                {
                    // If waitpoint does exists, then there must be sth weird...
                    Debug.Log($"Order do not exists any more => {currentOrder.waitPointName}");
                    break;
                }
                else
                {
                    // To find if food been consumed
                    bool isFoodStillRemain = tmp[0].products.ContainsKey(kvp.Key);
                    if (isFoodStillRemain)
                    {
                        // If food still remain, then can set delete line for current food count if not equal;
                        int remainingFoodCount = tmp[0].products[kvp.Key];
                        if (remainingFoodCount != kvp.Value)
                        {
                            // Set delete line and color;
                            countText.text = $"<s>*{kvp.Value}</s>";
                            UnityEngine.ColorUtility.TryParseHtmlString("#1D1D1D", out Color newColor);
                            countText.color = newColor;
                            // Set showing remaining count
                            remainCountObj.GetComponent<TextMeshPro>().text = $"*{tmp[0].products[kvp.Key]}";
                        }
                        else
                        {
                            UnityEngine.ColorUtility.TryParseHtmlString("#ffffff", out Color newColor);
                            countText.color = newColor;
                            // hide remaining count;
                            remainCountObj.SetActive(false);
                        }

                    }
                    else
                    {
                        // If food already consumed;
                        // Then set delete line and color to both food name and count while hide remaining count;
                        remainCountObj.SetActive(false);
                        UnityEngine.ColorUtility.TryParseHtmlString("#1D1D1D", out Color newColor);
                        // countText
                        countText.text = $"<s>*{kvp.Value}</s>";
                        countText.color = newColor;
                        // Foodtext;
                        foodNameText.text = $"<s>{kvp.Key}</s>";


                    }
                }

            }
        });
        // TextMeshProUGUI scoreText = gameInfo.transform.Find("Info/Score/currentScore").GetComponent<TextMeshProUGUI>();
        // if (!scoreText.IsUnityNull())
        // {
        //     scoreText.text = userScore.ToString();
        // }
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

    public void GenerateOrder(GameObject waitPoint)
    {
        Debug.Log($"Generate New Order in {waitPoint.name}");
        GameConstants.orderInfo orderInfo = new GameConstants.orderInfo();
        int orderSize = Random.Range(minOrderSize, maxOrderSize + 1);
        orderInfo.products = new Dictionary<string, int>();
        // create new List to avoid duplicate in orderInfo.products
        List<GameConstants.ProductsOption> availableFoods = new List<GameConstants.ProductsOption>(FoodList);
        for (int i = 0; i < orderSize; i++)
        {
            // If no more food can be select, then quit loop.
            if (availableFoods.Count == 0) break;
            // Add food name to the order;
            int randomIndex = Random.Range(0, availableFoods.Count);
            orderInfo.products.Add(availableFoods[randomIndex].prefab.name, Random.Range(1, 2));
            // Remove from List to avoid duplicate;
            availableFoods.RemoveAt(randomIndex);
        }
        // use waitPoint name to uniquely identify the order;
        orderInfo.waitPointName = waitPoint.gameObject.name;
        // fixed for milistone 3;
        orderInfo.difficulty = 1;
        //  Add new order to order List and remainingOrderList
        // if (orderListInfo.currentOrderList.IsUnityNull())
        // {
        //     orderListInfo.currentOrderList = new List<GameConstants.orderInfo>();
        // }
        // if (orderListInfo.remainingOrderList.IsUnityNull())
        // {
        //     orderListInfo.remainingOrderList = new List<GameConstants.orderInfo>();
        // }
        orderListInfo.currentOrderList.Add(orderInfo);
        orderListInfo.remainingOrderList.Add(orderInfo);
        Debug.Log($"Generate New Order in {waitPoint.name}\n\nOrder Info:\n\n{string.Join("", orderInfo.products.Select(o => $"{o.Key}*{o.Value}\n"))}");
        GenerateOrderBoard();
        // Test order;
        StartCoroutine(MockCompleteOrder(waitPoint.name, 3f, 8f));
    }

    public void StartGame()
    {
        // Set initial score
        UserScore = 0;
        // Initialize order List
        orderListInfo.currentOrderList = new List<GameConstants.orderInfo>();
        orderListInfo.remainingOrderList = new List<GameConstants.orderInfo>();
        // Debug for verify if correct params are passed to game manager.
        Debug.Log($"Current Mode:{(currentMode == GameConstants.MODE_TIMEATTACK ? "Time Attack" : "EndlessMode")}, Food Selections: {string.Join(", ", foodList.Select(p => p.name))}");

        if (currentMode == GameConstants.MODE_TIMEATTACK)
        {
            // default Time 70s;
            countdownTime = 10;
        }
        // Close startScreen
        startScreen.SetActive(false);
        gameInfo.SetActive(true);
        if (currentMode == GameConstants.MODE_TIMEATTACK)
        {
            gameInfo.transform.Find("Info/Clock").gameObject.SetActive(true);
            gameInfo.transform.Find("Info/EndGame").gameObject.SetActive(false);
        }
        else
        {
            gameInfo.transform.Find("Info/Clock").gameObject.SetActive(false);
            gameInfo.transform.Find("Info/EndGame").gameObject.SetActive(true);
        }
        // Active Spawner and arrange the ingredients there by invoke the function SpawnIngredients
        GameObject spawner = gameScene.transform.Find("Spawner").gameObject;
        spawner.SetActive(true);
        spawner.GetComponent<Spawner>().SpawnIngredients(foodList);
        // unlock player Movement for game playing.
        UnlockPlayerMovement();
        gameInfo.GetComponent<OrderManager>().StartGenerate();
        // no use for now.
        state = gameState.CreateOrder;
    }
    // End Game
    public void EndGame()
    {
        cleanUpGameScene();
        gameInfo.SetActive(false);
        endScreen.SetActive(true);
        Debug.Log("score" + UserScore);
        // Present final score
        TextMeshProUGUI finalScoreText = endScreen.transform.Find("Panel/finalScore").gameObject.GetComponent<TextMeshProUGUI>();
        if (!finalScoreText.IsUnityNull())
        {
            finalScoreText.text = $"Your final Score {UserScore}";
        }

        gameScene.transform.Find("Spawner").gameObject.SetActive(false);
        UserScore = 0;
    }
    // Clean generated objects
    public void cleanUpGameScene()
    {
        // Destroy ingredients.
        gameScene.transform.Find("Spawner").gameObject.GetComponent<Spawner>().DestroyIngredients();
        // Stop generating customers;
        gameInfo.GetComponent<OrderManager>().StopGenerate();
        // Clean orders
        orderListInfo.currentOrderList.Clear();
        orderListInfo.remainingOrderList.Clear();

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
        // Why no effect here???
        player.transform.SetLocalPositionAndRotation(new Vector3(-0.22f, 2, -0.43f), Quaternion.identity);

    }
    // ===================================================================
    // ===================== Mock completion logic ========================
    // ===================================================================

    /// <summary>
    /// A coroutine to simulate order completion in two steps:
    /// 1) Partially complete the order (submit a few items).
    /// 2) Wait for a certain amount of time, then fully complete the order.
    /// </summary>
    /// <param name="waitPointName">The name of the waitPoint (customer location) to mock completion</param>
    /// <param name="partialDelay">How many seconds to wait before submitting partial items</param>
    /// <param name="finalDelay">After partial submission, how many seconds to wait before fully completing the order</param>
    public IEnumerator MockCompleteOrder(string waitPointName, float partialDelay = 5f, float finalDelay = 10f)
    {
        // Wait for partialDelay seconds, then submit a part of the order (e.g. one item)
        yield return new WaitForSeconds(partialDelay);
        // if not found that wait point, then return default one.
        var targetOrder = orderListInfo.remainingOrderList.FirstOrDefault(o => o.waitPointName == waitPointName);
        // If still have some unfinished food request in the order.
        if (targetOrder != null && targetOrder.products.Count > 0)
        {
            // Take the first item from the order for partial completion
            string firstFoodKey = targetOrder.products.Keys.First();

            // For partial submission, let's only hand in one instance of that item
            List<string> partialFoods = new List<string>() { firstFoodKey };

            Debug.Log($"[MockCompleteOrder] First submission: partially submitting => {firstFoodKey}");
            EvaluateFood(partialFoods);
            // This will update the scoreboard via GenerateOrderBoard()
        }
        else
        {
            Debug.LogWarning($"[MockCompleteOrder] No order found at {waitPointName} for partial completion!");
            yield break;
        }
        // Submit to final the order, to check the animation and logics after finish the order.
        // Wait for finalDelay seconds, then fully complete the remaining items
        yield return new WaitForSeconds(finalDelay);

        var targetOrder2 = orderListInfo.remainingOrderList.FirstOrDefault(o => o.waitPointName == waitPointName);
        if (targetOrder2 != null && targetOrder2.products.Count > 0)
        {
            // Gather all remaining items in this order
            List<string> allRemainFoods = new List<string>();
            foreach (var kvp in targetOrder2.products)
            {
                for (int i = 0; i < kvp.Value; i++)
                {
                    allRemainFoods.Add(kvp.Key);
                }
            }
            Debug.Log($"[MockCompleteOrder] Second submission: completing all => {string.Join(",", allRemainFoods)}");
            EvaluateFood(allRemainFoods);
        }
        else
        {
            Debug.LogWarning($"[MockCompleteOrder] No remaining order found at {waitPointName} or it's already completed.");
        }
    }
}