using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/* How to align the virtual scene with the physical environment:
At the start of the app, place the controller where the reference point A should be,
press button 1, place the controller where the reference point B should be, press button 1.
The scene should be aligned. Press button 1 again to stop the alignment and hide the reference 
points, or button 2 to start again. */


public class Alignment : MonoBehaviour
{
    /* Attach this script to an empty gameObject that is the parent of all the objects that
    should be impacted by the alignment, including the reference points.
    The XR camera / controllers shouldn't be a children of that object, as it's position doesnt
    depend on the alignment.*/

    /* Create two gameObjects that will be used as reference points (eg. a small sphere for point A and
    a small cube for point B), and place them in the scene, at a position that is easy to locate in 
    the physical environment (eg. corners of the room or corners of a table)*/
    [SerializeField]
    private GameObject referencePointA;

    [SerializeField]
    private GameObject referencePointB;

    // Positions of the reference points
    private Vector3 virtualPositionA;
    private Vector3 virtualPositionB;

    // Positions where the reference points should be
    private Vector3 realPositionA;
    private Vector3 realPositionB;

    // Progress of the alignment
    private int alignmentState = 0;


    // Start is called before the first frame update
    void Start()
    {
        virtualPositionA = referencePointA.transform.position; 
        virtualPositionB = referencePointB.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        //OVRInput.Update();

        if (alignmentState == -1)
        {
            alignmentState = 0;
        }

        if (alignmentState == 2)
        {
            // The alignment is done.
            // When button 1 pressed, stop and hide reference points
            if (OVRInput.GetDown(OVRInput.Button.One)) {
                referencePointA.SetActive(false);
                referencePointB.SetActive(false);

                alignmentState = 3;
                var modelScript = transform.Find("Model").GetComponent<MakeModelInteractable>();
                modelScript.enabled = true; // calls Start()
            }

            // When button 2 pressed, start again
            if (OVRInput.GetDown(OVRInput.Button.Two)) {
                virtualPositionA = referencePointA.transform.position;
                virtualPositionB = referencePointB.transform.position;

                alignmentState = -1;
            }
        }

        if (alignmentState == 1)
        {
            // When button 1 pressed again, get the position where the reference point B should be
            if (OVRInput.GetDown(OVRInput.Button.One)) {

                realPositionB = OVRInput.GetLocalControllerPosition(OVRInput.Controller.Active);
                Debug.Log("Point B set");

                // A little bit of geometry to rotate, translate and rescale the parent of the objects,
                // to make the reference points match. 
                //Note that the rotation should always be done before the translation and rescaling.

                // Rotate
                Quaternion rotationOffset = Quaternion.FromToRotation(virtualPositionB - virtualPositionA, realPositionB - realPositionA);
                transform.rotation = rotationOffset * transform.rotation;
                virtualPositionA = referencePointA.transform.position;
                virtualPositionB = referencePointB.transform.position;

                // Rescale 
                float scaleFactor = (realPositionB - realPositionA).magnitude / (virtualPositionB - virtualPositionA).magnitude;
                transform.localScale = transform.localScale * scaleFactor;
                virtualPositionA = referencePointA.transform.position;
                virtualPositionB = referencePointB.transform.position;

                // Translate 
                transform.position = transform.position + (realPositionA - virtualPositionA);

                Debug.Log("Alignment done, push button 1 again to confirm, button 2 to restart");

                alignmentState = 2;
            }
        }


        if (alignmentState==0){
            // Start of the alignment
            // When button 1 pressed, get the position where the reference point A should be
            if (OVRInput.GetDown(OVRInput.Button.One)) {

                realPositionA = OVRInput.GetLocalControllerPosition(OVRInput.Controller.Active);
                Debug.Log("Point A set");

                alignmentState = 1;
            }
        }
    }
}
