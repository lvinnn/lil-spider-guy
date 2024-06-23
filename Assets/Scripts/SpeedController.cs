using System;
using UnityEngine;

public class SpeedController : MonoBehaviour
{

    public static SpeedController instance;

    public float factor;
    private float initFactor;

    private void Start()
    {
        initFactor = factor;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha1)) //freeze time
            Time.timeScale = Time.timeScale == 0 ? initFactor : 0;
        else if(Input.GetKey(KeyCode.LeftShift)) //sprint
            factor = 2*initFactor;
        else if(Input.GetKeyUp(KeyCode.LeftShift)) //unsprint
            factor = initFactor;
    }


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
}
