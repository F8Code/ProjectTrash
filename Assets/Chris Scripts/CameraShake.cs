using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public float shakeDuration = 4f;
    public float shakeMagnitude = 0.2f;  
    public float shakeFrequency = 10f;

    private Vector3 originalPos;

    void Start()
    {
        originalPos = transform.localPosition;
        
    }

    void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.T))
        {
           
            TriggerShake();
        }
    }

    public void TriggerShake()
    {
        StartCoroutine(Shake());
    }

    private IEnumerator Shake()
    {
       
        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            float y = Random.Range(-1f, 1f) * shakeMagnitude;
            transform.localPosition = originalPos + new Vector3(x, y, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = originalPos;
        
    }
}