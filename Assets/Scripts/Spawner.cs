using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Spawner : MonoBehaviour
{
    // Start is called before the first frame update
    public XRInteractionManager InteractableManager;
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void SpawnIngredients(List<GameConstants.ProductsOption> selections)
    {
        // Calculate the current food stuff number.
        int currentFoodCount = 0;
        int currentDrinkCount = 0;
        List<GameObject> assembleIngredients = selections.SelectMany(objPrefab => objPrefab.AssembledIngredientsPrefab).GroupBy(ing => ing.gameObject.name).Select(group => group.First()).ToList();
        Debug.Log($"Current Ingredients [{assembleIngredients.Count}] {string.Join(",", assembleIngredients.Select(p => p.name))}");
        assembleIngredients.ForEach(assIng =>
            {
                GameObject obj = Instantiate(assIng, Vector3.zero, Quaternion.Euler(0, 90, 0));
                obj.name = obj.name.Replace("(Clone)", "");
                Debug.Log($"Current Ingredient Assemble: {assIng.name} - {assIng.tag}");

                if (assIng.tag == "Drink")
                {
                    obj.transform.SetParent(transform.Find("Drinks"));
                    // We should put Drink in frige, and for food stuff put on the table.
                    // height of the frige is
                    if (currentDrinkCount == 0)
                    {
                        obj.transform.SetLocalPositionAndRotation(new Vector3(0f, -0.56f, 0f), Quaternion.identity);
                    }
                    else if (currentDrinkCount == 1)
                    {
                        obj.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                    }
                    else if (currentDrinkCount == 2)
                    {
                        obj.transform.SetLocalPositionAndRotation(new Vector3(0f, 0.567f, 0f), Quaternion.identity);
                    }
                    currentDrinkCount++;

                }
                else
                {
                    obj.transform.SetParent(transform.Find("Food"));
                    // For food stuff, Start Placement Position is 0.45 0.91 1.3, Placement Interval for X-axis is 0.47
                    obj.transform.SetLocalPositionAndRotation(new Vector3(0.45f + 0.47f * currentFoodCount++, 0.91f, 1.3f), Quaternion.Euler(0, 90, 0));
                }
                obj.GetComponent<GrabNewObject>().intManager = InteractableManager;
            });
    }
}
