using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SetGameInfoMenu : MonoBehaviour
{
    private int score;
    private TextMeshProUGUI text;
    private List<string> currentOrder;


    // Start is called before the first frame update
    void Start()
    {
        text = gameObject.GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        score = GameManager.Instance.GetScore();
        currentOrder = GameManager.Instance.GetCurrentOrder();

        text.text = $"Current Score:\r\n{score}\r\n\r\n\r\nCurrent Order:\r\n\r\n1x {currentOrder[0]}\r\n1x {currentOrder[1]}";
    }
}
