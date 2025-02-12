using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.UI;

public class Daylight : MonoBehaviour
{

    public GameObject daylight;
    public Scrollbar timeScrollbar;
    public TextMeshProUGUI daylightText;

    //[Range(0f, 1f)] public float time = 0.5f; // Time of day, 0 sunrise, 1 sundown
    // Start is called before the first frame update
    void Start()
    {
        timeScrollbar.value = 0.5f;
        UpdateSunRotation(timeScrollbar.value);
    }

    public void OnScrollbarValueChanged()
    {
        // Get the value from the scrollbar (from 0 to 1)
        float timeOfDay = timeScrollbar.value;

        // Update timeOfDay text
        //var text = daylightText.GetComponent<TextMeshPro>();
        daylightText.text = $"Time of Day: {6+ timeOfDay * 16:F2} hours";  // Convert to 24-hour format for display

        // Update light's rotation based on time of day
        UpdateSunRotation(timeOfDay);
    }

    // Update is called once per frame
    void Update()
    {
        
        //UpdateSunRotation(time);
    }

    // Function to update the sun's rotation based on timeOfDay value
    void UpdateSunRotation(float time)
    {
        // Assume the sun rotates around the X axis (you can change this if needed)
        float angle = Mathf.Lerp(0f, 180f, time); // Interpolate between -90 (sunrise) and 90 (sundown)
        daylight.transform.rotation = Quaternion.Euler(angle, -160, -180);
    }
}
