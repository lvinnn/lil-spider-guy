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

    private void Update()
    {
        //if (!isMoving)
        //{
        //    timeSinceStopped += Time.deltaTime;
        //}
        //for (int i = 0; i < footList.Count; i++)
        //{
        //    float distToSafe = Vector3.Distance(footList[i].transform.position, footList[i].safeZone.transform.position);
        //    float distToRoot = Vector3.Distance(footList[i].transform.position, root.transform.position);

        //    if (!footList[i].isSafe) footDistances[i] = distToRoot + distToSafe;
        //    else footDistances[i] = 0;
        //}

        //get max distance fro leg to body/safeZone out of all the legs NOT currently in its safe zone
        //maxFootDist = Mathf.Max(footDistances[0], footDistances[1], footDistances[2], footDistances[3]);
        //Debug.Log(maxFootDist);

    }


    //public bool CanMove(IkFeetSolver foot)
    //{
    //    if (anyLegMoving) return false;
    //    Debug.Log(lastMoved);

    //    int footNum = lastMoved + 2;
    //    if (footNum > 4) footNum = footNum % 4;
    //    return foot.name.EndsWith($"{footNum}");

    //    //float distToZone = Vector3.Distance(foot.transform.position, foot.safeZone.transform.position);
    //    //float distToRoot = Vector3.Distance(foot.transform.position, root.transform.position);

    //    //float x = Mathf.Abs(distToRoot + distToZone - maxFootDist);
    //    //return (x < .5f);
    //}
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