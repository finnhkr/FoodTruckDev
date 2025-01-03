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
    private List<GameConstants.ProductsOption> selectedProducts = new List<GameConstants.ProductsOption>();

    // invoke from UI toggles - for products in food truck game
    public void ToggleProduct(GameConstants.ProductsOption option, bool isSelected)
    {
        // select product
        if (isSelected)
        {
            // Add if not selected before
            if (!selectedProducts.Contains(option))
            {
                selectedProducts.Add(option);
            }
        }
        else
        {
            // Remove if selected before
            if (selectedProducts.Contains(option))
            {
                selectedProducts.Remove(option);
            }
        }

    }
    // Get prefab of selected products.
    public List<GameObject> GetSelectedProductPrefabs()
    {
        List<GameObject> selectedPrefabs = new List<GameObject>();
        // Only get prefabs for instantiate in the GameManager
        foreach (var option in selectedProducts)
        {
            selectedPrefabs.Add(option.prefab);
        }
        return selectedPrefabs;
    }
    // Get name of selected products

    public List<string> GetSelectedProductNames()
    {
        List<string> selectedNames = new List<string>();
        foreach (var option in selectedProducts)
        {
            selectedNames.Add(option.name);
        }
        return selectedNames;
    }
    [Header("UI Elements")]
    public GameObject togglePrefab; // Toggle Prefab;
    public Transform toggleParent; // Toggle parent component
    public Button StartGameButton;
    // Toggle Group for mode (Endless mode / Time Attack)
    public ToggleGroup modeToggleGroup;
    // Start is called before the first frame update
    void Start()
    {
        foreach (var option in productOptions)
        {
            // instantiate toggles by options;
            GameObject toggleObj = Instantiate(togglePrefab, toggleParent);
            // Get toggle component -> then get Content -> Text, and Content->symbol for text and image.
            Toggle toggle = toggleObj.GetComponent<Toggle>();
            toggleObj.GetComponentInChildren<TMP_Text>().text = option.name;
            toggle.isOn = false;
            Transform symbolObj = toggleObj.GetComponent<Transform>().Find("Content/Symbol");
            if (symbolObj)
            {
                // Set image if object exists.
                // As I can't extract thumbnail from prefabs, so I just use the name now.
                // Texture2D thumbnail = AssetPreview.GetAssetPreview(option.prefab);
                // symbolObj.GetComponent<Image>().sprite =
                // As you can see from the StartScreen, I use the cube image instead of ingreadients' images.
            }
            else
            {
                Debug.LogWarning("No image object in Toggle [" + option.name + "]");
            }
            var currentOption = option;
            // Add the option to selectedOption if it is on.
            toggle.onValueChanged.AddListener((isOn) =>
            {
                ToggleProduct(currentOption, isOn);
            });
        }
        // StartGame Button
        StartGameButton.onClick.AddListener(OnStartGame);
    }
    void OnStartGame()
    {
        Debug.Log($"Current Mode:{modeToggleGroup.ActiveToggles().ToArray()[0].gameObject.name.Trim()}");
        string currentMode = modeToggleGroup.ActiveToggles().ToArray()[0].gameObject.name.Trim();
        if (currentMode == "Time Attack")
        {
            // 0->Time Attack
            GameManager.Instance.Playmode = GameConstants.MODE_TIMEATTACK;
        }
        else
        {
            // 1->Endless Mode
            GameManager.Instance.Playmode = GameConstants.MODE_ENDLESS;
        }
        List<GameConstants.ProductsOption> userSelects = selectedProducts;
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
        // If current selectedProducts is empty, disable StartGameButton
        if (selectedProducts.Count == 0)
        {
            StartGameButton.gameObject.SetActive(false);
        }
        else
        {
            StartGameButton.gameObject.SetActive(true);
        }
    }
}
