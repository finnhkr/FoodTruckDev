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
}
