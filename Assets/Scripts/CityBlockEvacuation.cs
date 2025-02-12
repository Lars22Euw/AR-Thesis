using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CityBlockEvacuation : MonoBehaviour
{
    private Material prevMaterial;
    private Color prevColor;

    // Start is called before the first frame update
    void Start()
    {
        Debug.LogWarning("Start of CityBlock Evacuation Script");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnEvacAreaHit()
    {
        // Define the building's response to being hit by the evacuation area
        Debug.Log($"{gameObject.name} was hit by the expanding evacuation area!");

        // Example: Change building color to indicate interaction
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            prevMaterial = renderer.material;
            prevColor = prevMaterial.color;
            renderer.material.color = Color.yellow; // Change to yellow on hit
        }

        // Add any additional behavior here, such as triggering animations or gameplay effects
    }

    internal void OnEvacAreaExit()
    {
        Debug.Log($"{gameObject.name} left the evacuation area!");

        // Example: Change building color to indicate interaction
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = prevColor;
        }
    }
}
