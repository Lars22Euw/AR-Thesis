using Meta.XR.MRUtilityKit;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;
using static UnityEngine.UI.Image;

public class EvacutaionManager : MonoBehaviour
{
    // Target radius in meters
    [SerializeField] [Range(0, 5)] private float evacRadius = 1f;

    // Speed of expansion in meters per second
    [SerializeField] [Range(0, 5)] private float expansionSpeed = 0.5f;

    public bool isRunning = false; // toggled by UI button
    private float CylinderHeight = 0.05f;

    [SerializeField] private GameObject VisArea, SimArea;
    [SerializeField] private TMP_Text buttonText;
    [SerializeField] private TMP_Text evacText;

    private NavMeshObstacle obstacle;
    private CarPoolSpawner CarPoolSpawner;

    private void Start()
    {
        CarPoolSpawner = FindObjectOfType<CarPoolSpawner>();
        obstacle = SimArea.GetComponent<NavMeshObstacle>();
    }

    private void Update()
    {
    }

    public void ExpandEvacArea(Vector3 origin)
    {
        // Set position and initial scale
        VisArea.transform.position = origin;
        VisArea.transform.localScale = new Vector3(0.01f, CylinderHeight, 0.01f);
        CarPoolSpawner.UpdateSimulationModel(VisArea, SimArea);

        StopAllCoroutines();
        var expansion = StartCoroutine(ExpandEvacArea());
    }

    private System.Collections.IEnumerator ExpandEvacArea()
    {
        float currentRadius = VisArea.transform.localScale.x;

        while (currentRadius < evacRadius)
        {
            // Increase the radius
            currentRadius += expansionSpeed * Time.deltaTime;
            VisArea.transform.localScale = new Vector3(currentRadius, CylinderHeight, currentRadius);
            SimArea.transform.localScale = new Vector3(evacRadius * 1000f, CylinderHeight * 1000f, evacRadius * 1000f);
            obstacle.radius = evacRadius * 0.46f;

            // Wait until the next frame
            yield return null;
        }

        // Clamp the radius to ensure precision
        VisArea.transform.localScale = new Vector3(evacRadius, CylinderHeight, evacRadius);
        SimArea.transform.localScale = new Vector3(evacRadius * 1000f, CylinderHeight * 1000f, evacRadius * 1000f);
        
        obstacle.radius = evacRadius * 0.46f;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.LogWarning("Evac hit Trigger");
        CityBlockEvacuation building = other.GetComponent<CityBlockEvacuation>();
        if (building != null)
        {
            building.OnEvacAreaHit();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        CityBlockEvacuation building = other.GetComponent<CityBlockEvacuation>();
        if (building != null)
        {
            building.OnEvacAreaExit();
        }
    }

    public void ToggleRunning()
    {
        if (isRunning)
        {
            // turn off
            VisArea.SetActive(false);
            SimArea.SetActive(false);
        }
        else
        {
            // turn on
            VisArea.SetActive(true);
            SimArea.SetActive(true);
        }
        this.isRunning = !this.isRunning;

        if (evacText.gameObject.activeSelf != isRunning)
        {
            evacText.gameObject.SetActive(isRunning);
        }

        if (buttonText != null)
        {
            buttonText.text = isRunning ? "Stop Evacuation" : "Start Evacuation";
        }
    }
}
