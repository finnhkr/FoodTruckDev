using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIButtonClick : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        Button btn = gameObject.GetComponent<Button>();
        btn.onClick.AddListener(ActionOnClick);
    }

    public void ActionOnClick()
    {
        GameManager.Instance.returnStartScreen();
    }
}
