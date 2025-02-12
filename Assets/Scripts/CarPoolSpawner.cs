using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class CarPoolSpawner : MonoBehaviour
{
    [Header("Car Prefabs")]
    [SerializeField] private string prefabFolder = "Car Prefabs/Prefabs";
    [SerializeField] private GameObject[] TrafficPrefabs;

    [Header("Spawn Settings")]
    public int poolSize = 10; // Number of vehicles to spawn
    public GameObject targetFolder; // Folder for target / spawn locations
    [SerializeField] private Transform[] targetPoints; // Target locations (NavMesh points)

    private List<GameObject> agentPool = new List<GameObject>(); // used for navigation
    private List<GameObject> carPool = new List<GameObject>();   // used for visuals
    private List<CarAgent> activeCars = new List<CarAgent>();
    private Dictionary<GameObject, GameObject> agentToVisualMap = new Dictionary<GameObject, GameObject>();

    [Header("Buttons")]
    private bool isTrafficShown = false;
    private bool isTrafficActive = false;
    [SerializeField] private Button toggleButton; // Button to toggle active, paused traffic
    public TextMeshProUGUI showTrafficText;
    public TextMeshProUGUI pauseTrafficText;

    [Header("Sim to Vis")]
    private float carScale = 200f;
    [SerializeField] public GameObject VisModel; // The visual model
    [SerializeField] public GameObject Simulation; // The simulation model
    private GameObject VisTraffic, SimTraffic;


    void Start()
    {
        LoadPrefabs(); // StartCoroutine(
        VisTraffic = VisModel.transform.Find("Traffic").gameObject;
        SimTraffic = Simulation.transform.Find("Traffic").gameObject;
    }

    private void Update()
    {
        foreach (var pair in agentToVisualMap)
        {
            GameObject simulationAgent = pair.Key;
            GameObject car = pair.Value;

            // Update position and rotation
            UpdateVisualModel(simulationAgent, car);
        }
    }

    // Updates Object from Sim -> Visual Model
    public void UpdateVisualModel(GameObject simObj, GameObject visObj)
    {
        Vector3 relativePosition = (simObj.transform.position - Simulation.transform.position) / 1000f;
        Quaternion rotation = Quaternion.Euler(VisModel.transform.rotation.eulerAngles);
        visObj.transform.position = VisModel.transform.position + rotation * relativePosition;

        Quaternion relativeRotation = Quaternion.Inverse(Simulation.transform.rotation) * simObj.transform.rotation;
        Quaternion correction = Quaternion.Euler(0, -90, 0); // offset for car rotation - not needed for other obj
        Quaternion adjustedRotation = VisModel.transform.rotation * relativeRotation * correction;
        visObj.transform.rotation = adjustedRotation;
    }


    // Updates Object from Vis -> Sim Model
    public void UpdateSimulationModel(GameObject visObj, GameObject simObj)
    {
        Vector3 relativePosition = visObj.transform.position - VisModel.transform.position;

        // Apply the inverse rotation to return to the original space
        Quaternion inverseRotation = Quaternion.Inverse(Quaternion.Euler(VisModel.transform.rotation.eulerAngles));
        Vector3 adjustedPosition = inverseRotation * relativePosition;
        simObj.transform.position = Simulation.transform.position + adjustedPosition * 1000f;

        Quaternion relativeRotation = Quaternion.Inverse(VisModel.transform.rotation) * visObj.transform.rotation;
        simObj.transform.rotation = Simulation.transform.rotation * relativeRotation;
    }

    public Vector3 ConvertVisToSimPosition(Vector3 visPosition)
    {
        Vector3 relativePosition = visPosition - VisModel.transform.position;

        Quaternion inverseRotation = Quaternion.Inverse(Quaternion.Euler(VisModel.transform.rotation.eulerAngles));
        Vector3 adjustedPosition = inverseRotation * relativePosition;
        Vector3 simPosition = Simulation.transform.position + adjustedPosition * 1000f;
        return simPosition;
    }

    private void LoadPrefabs()
    {
        TrafficPrefabs = Resources.LoadAll<GameObject>(prefabFolder);

        var targets = targetFolder.transform.childCount;
        targetPoints = new Transform[targets];
        for (int i = 0; i < targets; i++)
        {
            var child = targetFolder.transform.GetChild(i);
            targetPoints[i] = child.gameObject.transform;
        }
        InitializeCarPool();
    }

    private void InitializeCarPool()
    {
        if (!isTrafficShown) return;
        for (int i = 0; i < poolSize; i++)
        {
            // Randomly select a car prefab and spawn point
            GameObject prefab = TrafficPrefabs[Random.Range(0, TrafficPrefabs.Length)];
            Transform spawnPoint = targetPoints[Random.Range(0, targetPoints.Length)];

            // Instantiate the car and add to the pool
            GameObject car = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);// position is wrong, but we dont care yet
            car.transform.SetParent(VisTraffic.transform);
            car.transform.localScale = new Vector3(carScale, carScale, carScale);
            car.SetActive(isTrafficShown);

            var agent = GameObject.CreatePrimitive(PrimitiveType.Cube);
            agent.transform.SetParent(SimTraffic.transform);
            agent.transform.position = spawnPoint.position;
            agent.transform.rotation = spawnPoint.rotation;
            agent.gameObject.layer = 6; // make dummy invisble to cameras
            agent.SetActive(true);
            
            startCar(agent, car);

            agentPool.Add(agent);
            carPool.Add(car);
            if (!agentToVisualMap.ContainsKey(agent))
            {
                agentToVisualMap.Add(agent, car);
            }
        }
    }


    private void startCar(GameObject dummy, GameObject car)
    {
        var agent = dummy.GetComponent<NavMeshAgent>();
        if (agent == null) agent = dummy.AddComponent<NavMeshAgent>();
        agent.speed = Random.Range(20f, 30f); // Assign random speed for variety
        var carAgent = dummy.AddComponent<CarAgent>();
        carAgent.Initialize(agent, targetPoints);
        activeCars.Add(carAgent);
    }

    public void HideTraffic()
    {
        isTrafficShown = !isTrafficShown;

        toggleButton.gameObject.SetActive(isTrafficShown);

        foreach (var pair in agentToVisualMap)
        {
            pair.Value.SetActive(isTrafficShown);
        }

        if (!isTrafficShown)
        {
            PauseTraffic();
        }
        else
        {
            // if sim paused, start again
            if (!isTrafficActive) PauseTraffic();
            if (activeCars.Count < poolSize) InitializeCarPool();
        }

        if (showTrafficText != null)
        {
            showTrafficText.text = isTrafficShown ? "Hide Traffic" : "Show Traffic";
        }
    }

    public void PauseTraffic()
    {
        isTrafficActive = !isTrafficActive;

        // Enable or disable all active cars
        foreach (CarAgent car in activeCars)
        {
            car.ToggleMovement(isTrafficActive);
        }

        if (pauseTrafficText != null)
        {
            pauseTrafficText.text = isTrafficActive ? "Pause Traffic" : "Continue Traffic";
        }
    }

    bool RemoveAgent(GameObject agent)
    {
        return agentToVisualMap.Remove(agent);
    }
}
