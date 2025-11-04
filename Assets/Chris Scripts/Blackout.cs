using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightFlickerHazard : MonoBehaviour
{
    [Header("Flicker Settings")]
    public float blackoutDuration = 3f;     
    public float flickerDuration = 2f;     
    public float flickerMinIntensity = 0.1f; 
    public float flickerMaxIntensity = 1f;   

    [Header("Auto Trigger")]
    public float hazardInterval = 25f;      
    private Light[] allLights;
    private Dictionary<Light, float> originalIntensities = new Dictionary<Light, float>();

    void Start()
    {
        
        allLights = FindObjectsOfType<Light>();
        foreach (Light light in allLights)
        {
            originalIntensities[light] = light.intensity;
        }

        

       
        InvokeRepeating("TriggerHazard", 10f, hazardInterval);
    }

    void Update()
    {
       
        if (Input.GetKeyDown(KeyCode.F))
        {
            
            TriggerHazard();
        }
    }

 
    public void TriggerHazard()
    {
        StartCoroutine(FlickerSequence());
    }
    private IEnumerator FlickerSequence()
    {
       

       
        foreach (Light light in allLights)
        {
            light.intensity = 0f; 
        }

        yield return new WaitForSeconds(blackoutDuration);
        Debug.Log("💡 FLICKER START!");
        float flickerElapsed = 0f;

        while (flickerElapsed < flickerDuration)
        {
          
            float randomIntensity = Random.Range(flickerMinIntensity, flickerMaxIntensity);

            foreach (Light light in allLights)
            {
                light.intensity = originalIntensities[light] * randomIntensity;
            }

            flickerElapsed += Time.deltaTime;
            yield return null; 
        }

        foreach (Light light in allLights)
        {
            light.intensity = originalIntensities[light];
        }
    }
}