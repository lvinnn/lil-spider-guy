using System.Collections.Generic;
using UnityEngine;

public class LegManager : MonoBehaviour
{
    public static LegManager Instance;

    public IkFeetSolver foot1;
    public IkFeetSolver foot2;
    public IkFeetSolver foot3;
    public IkFeetSolver foot4;
    public GameObject root;

    public int lastMoved;

    [SerializeField] private List<IkFeetSolver> footList = new List<IkFeetSolver>();
    //[SerializeField] private List<float> footDistances = new List<float> { 0f, 0f, 0f, 0f };

    //[SerializeField] private float maxFootDist;

    //a leg is currently moving
    [SerializeField] private bool anyLegMoving = false;
    [SerializeField] private float timeSinceStopped;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
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