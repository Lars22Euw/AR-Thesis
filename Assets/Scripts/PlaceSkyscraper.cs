using Meta.XR.MRUtilityKit;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

public class PlaceSkyscraper : MonoBehaviour
{

    // Label filter for raycast
    public MRUKAnchor.SceneLabels labelFilter;
    public Transform rayStartPoint; // Right Hand Anchor
    public float rayLength = 10f;   // Maximum length of the ray
    public GameObject marker, obj, simObj;  // Marker and prefab

    public GameObject barrier, simBarrier;

    private CarPoolSpawner CarPoolSpawner;
    private EvacutaionManager evacManager;

    public Transform parentObject;

    public bool isReady = false;
    // Start is called before the first frame update
    void Start()
    {
        CarPoolSpawner = FindObjectOfType<CarPoolSpawner>();
        evacManager = FindAnyObjectByType<EvacutaionManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isReady) return;


        var room = MRUK.Instance.GetCurrentRoom();
        if (room == null) return;
        Ray ray = new Ray(rayStartPoint.position, rayStartPoint.forward);
        if (room.Raycast(ray, rayLength, LabelFilter.Included(labelFilter), out RaycastHit hit, out MRUKAnchor anchor))
        {
            marker.transform.position = hit.point;

            if (evacManager.isRunning && OVRInput.GetDown(OVRInput.Button.One))
            {
                Debug.Log("Placing bomb!");
                evacManager.ExpandEvacArea(hit.point);
            }

            if (OVRInput.GetDown(OVRInput.Button.Two))
            {
                Debug.Log("Placing skyscraper!");
                SpawnSkyscraper(hit.point + new Vector3(0, 0.1f, 0), obj);
            }

            if (OVRInput.GetDown(OVRInput.Button.Three))
            {
                Debug.Log("Triggered Block removal");
                RemoveCityBlock(hit.point);
            }

            if (OVRInput.GetDown(OVRInput.Button.Four))
            {
                Debug.Log("Placing barrier");
                PlaceBarrier(hit.point + new Vector3(0, 0.1f, 0));
            }
        }
    }

    void PlaceBarrier(Vector3 pos)
    {
        var i = Instantiate(barrier);
        i.transform.position = pos;
        var simInstance = Instantiate(simBarrier);
        CarPoolSpawner.UpdateSimulationModel(i, simInstance);
    }

    void SpawnSkyscraper(Vector3 pos, GameObject obj)
    {
        var instance = Instantiate(obj);
        instance.transform.position = pos;
        var simInstance = Instantiate(simObj);
        CarPoolSpawner.UpdateSimulationModel(instance, simInstance);
        simInstance.transform.position = simInstance.transform.position - new Vector3(0, 120, 0);
        simInstance.transform.rotation = Quaternion.identity;
    }

    GameObject GetClosestObjectToPoint(Vector3 pos, GameObject parent)
    {
        Transform closestObject = null;
        float minDistance = Mathf.Infinity;

        foreach (Transform child in parent.transform)
        {
            // Get MeshRenderer or Collider to find the actual position
            MeshRenderer meshRenderer = child.GetChild(0).GetComponent<MeshRenderer>();
            Collider collider = child.GetChild(0).GetComponent<Collider>();

            Vector3 objectCenter = Vector3.zero;

            if (meshRenderer != null)
            {
                objectCenter = meshRenderer.bounds.center;
            }
            else if (collider != null)
            {
                objectCenter = collider.bounds.center;
            }
            else
            {
                Debug.LogWarning($"No MeshRenderer or Collider found on {child.name}");
                continue;
            }

            // Calculate the distance to the target point
            float distance = Vector3.Distance(pos, objectCenter);

            // Check if this is the closest object
            if (distance < minDistance)
            {
                minDistance = distance;
                closestObject = child.GetChild(0);
            }
        }
        return closestObject.gameObject;
    }

    void RemoveCityBlock(Vector3 pos)
    {
        var VisCityBlockSelected = GetClosestObjectToPoint(pos, parentObject.gameObject);

        if (VisCityBlockSelected != null)
        {
            var simPos = CarPoolSpawner.ConvertVisToSimPosition(pos);
            var simBlock = GetClosestObjectToPoint(simPos, CarPoolSpawner.Simulation);

            obj.SetActive(false);
            simBlock.SetActive(false);
        }
        else
        {
            Debug.Log("No valid objects found.");
        }
    }
}
