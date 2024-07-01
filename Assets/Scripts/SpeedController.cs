using System;
using UnityEngine;

public class SpeedController : MonoBehaviour
{

    public static SpeedController instance;

    public float factor = 1;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha1)) //freeze time
            Time.timeScale = Time.timeScale == 0 ? 1 : 0;
        else if (Input.GetKeyDown(KeyCode.LeftShift)) //sprint
            factor *= 2;
        else if(Input.GetKeyUp(KeyCode.LeftShift)) //unsprint
            factor /= 2;
        // else if (Input.GetKeyDown(KeyCode.Mouse0))
        //     factor /= 33f;
        // else if (Input.GetKeyUp(KeyCode.Mouse0))
        //     factor *= 33;
        
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
