using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu]
public class Order : ScriptableObject
{
    public List<GameObject> content;

    public string label;
}
