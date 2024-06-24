using System;
using UnityEngine;

public class SpeedController : MonoBehaviour
{

    public static SpeedController instance;

    public float factor;
    private float factor2;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha1)) //freeze time
            Time.timeScale = Time.timeScale == 0 ? 1 : 0;
        else if (Input.GetKeyDown(KeyCode.LeftShift)) //sprint
        {
            factor2 = factor;
            factor *= 2;
        }
        else if(Input.GetKeyUp(KeyCode.LeftShift)) //unsprint
            factor = factor2;
        else if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            factor2 = factor;
            factor = 0.03f;
        }
        else if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            factor = factor2;
        }
        
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
