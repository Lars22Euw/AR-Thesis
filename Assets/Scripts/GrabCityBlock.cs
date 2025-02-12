using Oculus.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GrabCityBlock : MonoBehaviour
{
    private OVRInput.Controller activeController = OVRInput.Controller.RTouch; // Right hand controller by default

    // Start is called before the first frame update
    void Start()
    {
        
    }



    // Update is called once per frame
    void Update()
    {

        CheckRayHit();
        
    }

    private void CheckRayHit()
    {
        // Create a ray from the controller's position and direction
        Ray ray = new Ray(OVRInput.GetLocalControllerPosition(activeController),
                          OVRInput.GetLocalControllerRotation(activeController) * Vector3.forward);

        // Raycast to check if it hits anything
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Debug.Log("Ray hit: " + hit.collider.gameObject.name);

            // Example: only interact with objects tagged as "Interaction"
            if (hit.collider.CompareTag("Interaction"))
            {
                HandleRayInteraction(hit.collider.gameObject);
            }
        }
    }

    private void HandleRayInteraction(GameObject hitObject)
    {
        Debug.Log("Interacting with: " + hitObject.name);

        // Example action: change color of the object
        Renderer renderer = hitObject.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.green;
        }

        // Check if the trigger button is pressed
        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, activeController))
        {
            Debug.LogWarning("Hit Interaction and trigger pressed");
        }
    }
}
