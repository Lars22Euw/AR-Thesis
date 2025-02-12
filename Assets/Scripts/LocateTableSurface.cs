using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Meta.XR.MRUtilityKit;

public class DisplayLabel : MonoBehaviour
{

    public Transform rayStartPoint;
    public float rayLength = 5;
    public MRUKAnchor.SceneLabels labelFilter;
    public TMPro.TextMeshPro debugText;

    public GameObject Model;      // Visualization Model
    public GameObject Simulation; // Simulation Model
    public GameObject Canvas;

    private bool SceneLoaded = false;
    private bool TableSelected = false;

    Vector3 modelOffset = new Vector3(-1.015f, -0.74f, -0.917f);

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Started Table Locator");
    }

    public void enableScene()
    {
        SceneLoaded = true;
        Debug.Log("Scene loaded!");
    }

    // Update is called once per frame
    void Update()
    {
        if (!SceneLoaded) return; // make sure scene loaded.

        if (!TableSelected) SelectTable();
    }

    void SelectTable()
    {
        Ray ray = new Ray(rayStartPoint.position, rayStartPoint.forward);
        var room = MRUK.Instance.GetCurrentRoom();
        if (room == null) return;
        bool ishit = room.Raycast(ray, rayLength, LabelFilter.Included(labelFilter), out RaycastHit hit, out MRUKAnchor anchor);

        if (ishit)
        {
            string label = anchor.Label.ToString();

            debugText.transform.position = hit.point;
            debugText.transform.rotation = Quaternion.LookRotation(-hit.normal);
            debugText.text = "Correct " + label + " Surface?\nPress Button A to confirm";

            if (OVRInput.GetDown(OVRInput.Button.One))
            {
                Debug.LogWarning("--- Placing Model on Table ---");
                debugText.text = "";
                //Simulation.transform.SetParent(anchor.transform, false);
                Model.SetActive(true);

                Model.transform.position = new Vector3(-1.4f, 0.75f, -0.17f);
                Model.transform.SetParent(anchor.transform, false);

                Model.transform.rotation = anchor.transform.rotation;
                Model.transform.up = hit.normal;
                var relativeForward = anchor.ParentAnchor.transform.rotation * Vector3.right - anchor.transform.rotation * Vector3.right;
                var finalRotation = Quaternion.LookRotation(relativeForward.normalized) * Quaternion.Euler(0, 180, 0);
                Model.transform.rotation *= finalRotation;

                
                //Model.transform.position += Vector3.forward * -0.79f;
                //Model.transform.position += Vector3.right * -1.39f;
                //Model.transform.position = anchor.transform.position + modelOffset;
                //Model.transform.SetPositionAndRotation(anchor.GetAnchorCenter() + modelOffset, finalRotation);

                TableSelected = true;
                var em = GetComponent<EffectMesh>();
                em.ToggleEffectMeshVisibility(false);
                em.enabled = false;

                var placeSkyScraper = FindObjectOfType<PlaceSkyscraper>();
                placeSkyScraper.isReady = true;
                //var trafficManager = FindObjectOfType<CarPoolSpawner>();
                //trafficManager.isReady = true;
            }
        }
    }

}
