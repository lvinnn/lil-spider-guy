using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SecurityCamManager : MonoBehaviour
{
    public static SecurityCamManager instance;
    
    public Transform target;
    public float lookSpeed = 10f;
    public GameObject light;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    // Start is called before the first frame update

    void Start()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).AddComponent<SecurityCamController>();
            
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}