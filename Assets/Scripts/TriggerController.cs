using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerController : MonoBehaviour
{

    public static TriggerController instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // public float timeOnGround = 0;
    // private void OnTriggerStay(Collider other)
    // {
    //     timeOnGround += Time.deltaTime;
    //     // Debug.Log(timeOnGround);
    // }
}
