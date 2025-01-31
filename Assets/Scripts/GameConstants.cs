using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameConstants : MonoBehaviour
{
    #region Singleton
    private static GameConstants instance;
    public static GameConstants Instance
    {
        get
        {
            if (instance == null)
            {
                Debug.LogError("GameConstants is null");
            }

            return instance;
        }
    }
    private void Awake()
    {
        if (instance)
            Destroy(gameObject);
            Debug.LogError("D6");
        else
            instance = this;

        DontDestroyOnLoad(this);
    }

    #endregion
    public const int MODE_TIMEATTACK = 0;
    public const int MODE_ENDLESS = 1;
    [System.Serializable]
    public class ProductsOption
    {
        // human readable name
        public string name;
        public GameObject prefab;
        public List<GameObject> ingredientsPrefab;
        // For Spawner
        public List<GameObject> AssembledIngredientsPrefab;
    }
    [System.Serializable]
    public class orderInfo
    {
        // identifier by using waitpoint name;
        public string waitPointName;
        public Dictionary<string, int> products;
        // difficulty (for different score) - for milestone 4 - I'm tired to add it now ^.&^
        public int difficulty;
    }
    public void ClearChildren(GameObject parent)
    {
        foreach (Transform child in parent.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }
}
