using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshManager : MonoBehaviour
{

    [SerializeField] private NavMeshAgent Agent;

    public Transform[] points; // target locations
    private int destPoint = 0; // index of current target

    public GameObject Model;
    public GameObject Buildings;

    // Start is called before the first frame update
    void Start()
    {
        var surface = Model.GetComponent<NavMeshSurface>();
        //var mod = Buildings.AddComponent<NavMeshModifier>();
        //mod.applyToChildren = true;
        //surface.navMeshData = new NavMeshData();
        Agent.autoBraking = false;
        GoToNextPoint();
    }

    // Update is called once per frame
    void Update()
    {
        if (!Agent.pathPending && Agent.remainingDistance < 0.5f) GoToNextPoint();
    }

    private void GoToNextPoint()
    {
        if (points.Length == 0) return;

        Agent.destination = points[destPoint].position;
        destPoint = Random.Range(0, points.Length);
        
    }
}
