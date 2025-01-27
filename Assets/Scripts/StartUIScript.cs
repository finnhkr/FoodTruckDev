using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using System;
using System.Linq;
public class StartUIScript : MonoBehaviour
{

    // [System.Serializable]
    // public class ProductsOption
    // {
    //     // human readable name
    //     public string name;
    //     public GameObject prefab;
    //     public List<GameObject> ingredientsPrefab;
    //     // For Spawner
    //     public List<GameObject> AssembledIngredientsPrefab;
    // }
    // Available Products can be used in food truck gane
    [Header("Available Products")]
    public List<GameConstants.ProductsOption> productOptions;
    // User selected products
    // invoke from UI toggles - for products in food truck game
    // Get prefab of selected products.
    [Header("UI Elements")]
    public GameObject togglePrefab; // Toggle Prefab;
    public Transform toggleParent; // Toggle parent component
    public Button StartGameButton;
    public GameObject timeSelect;
    // Toggle Group for mode (Endless mode / Time Attack)
    public ToggleGroup modeToggleGroup;

    public ToggleGroup timeDurationSelectGroup;
    // Start is called before the first frame update
    void Start()
    {
        // StartGame Button
        StartGameButton.onClick.AddListener(OnStartGame);
    }
    void OnStartGame()
    {

        string currentMode = modeToggleGroup.ActiveToggles().ToArray()[0].gameObject.name.Trim();
        string currentTimeDuration = timeDurationSelectGroup.ActiveToggles().ToArray()[0].gameObject.name.Trim();
        Debug.Log($"Current Mode:{currentMode} timeDuration:{currentTimeDuration}");
        if (currentMode == "TimeMode")
        {
            // 0->Time Attack
            GameManager.Instance.Playmode = GameConstants.MODE_TIMEATTACK;
            if (currentTimeDuration == "Time0")
            {
                GameManager.Instance.playTimeDuration = 30;
            }
            else if (currentTimeDuration == "Time1")
            {
                GameManager.Instance.playTimeDuration = 60;
            }
            else if (currentTimeDuration == "Time2")
            {
                GameManager.Instance.playTimeDuration = 120;
            }
        }
        else
        {
            // 1->Endless Mode
            GameManager.Instance.Playmode = GameConstants.MODE_ENDLESS;
        }
        List<GameConstants.ProductsOption> userSelects = productOptions;
        // Set User selects and pass to game manager;
        GameManager.Instance.FoodList = userSelects;

        GameManager.Instance.StartGame();
    }

    // Update is called once per frame
    void Update()
    {

    }
    private void FixedUpdate()
    {
        // Only show when in TimeMode
        string currentMode = modeToggleGroup.ActiveToggles().ToArray()[0].gameObject.name.Trim();
        if (currentMode == "TimeMode")
        {
            timeSelect.gameObject.SetActive(true);
        }
        else
        {
            timeSelect.gameObject.SetActive(false);
        }
    }
}
