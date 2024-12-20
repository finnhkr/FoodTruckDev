using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SetScoreInUI : MonoBehaviour
{
    private int score;
    private TextMeshProUGUI text;

    // Start is called before the first frame update
    void Start()
    {
        score = 0;
        text = gameObject.GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        score = GameManager.Instance.getScore();

        text.text = $"You failed to hand in the last order correctly !\r\n\r\nYou priviously handed in \r\n{score}\r\norders successfully.";
    }
}
