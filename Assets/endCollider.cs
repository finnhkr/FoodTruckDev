using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class endCollider : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    void OnTriggerEnter(Collider other)
    {
        var cb = other.GetComponent<CustomerBehavior>();
        if (cb != null)
        {
            Destroy(cb.gameObject);
            Debug.LogError("D1");
        }
    }
}
