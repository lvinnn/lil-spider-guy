using System;
using System.Collections.Generic;
using UnityEngine;

public class LegManager : MonoBehaviour
{
    public static LegManager instance;

    public IkFeetSolver foot1;
    public IkFeetSolver foot2;
    public IkFeetSolver foot3;
    public IkFeetSolver foot4;

    public List<IkFeetSolver> footList = new List<IkFeetSolver>();

    //a leg is currently moving
    [SerializeField] private bool anyLegMoving = false;
    [SerializeField] private float timeSinceStopped;

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

    private void Start()
    {
        footList.Add(foot1);
        footList.Add(foot2);
        footList.Add(foot3);
        footList.Add(foot4);
    }

    public int GetNUmHovering()
    {
        var count = 0;
        foreach (var foot in new[] {foot1, foot2, foot3, foot4})
        {
            if (foot.isHovering) count++;
        }

        return count;
    }

    public void SetMoving(bool b)
    {
        anyLegMoving = b;
    }
    public bool GetAnyLegMoving()
    {
        return anyLegMoving;
    }

    public float GetTime()
    {
        return timeSinceStopped;
    }
    public void SetTime(float t)
    {
        timeSinceStopped = t;
    }

    private void OnDrawGizmosSelected()
    {
        //for(int i = 0; i<footList.Count; i++)
        //{
        //    Handles.Label(footList[i].transform.position+Vector3.up*4, $"{footDistances[i]}");
        //}
    }

}