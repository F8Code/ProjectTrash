using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public float shakeDuration = 4f;
    public float shakeMagnitude = 0.2f;  // аявийа 0.2 - дойиласе 0.5!
    public float shakeFrequency = 10f;

    private Vector3 originalPos;

    void Start()
    {
        originalPos = transform.localPosition;
        Debug.Log("CameraShake READY! Press T to test."); // <-- TEST MESSAGE
    }

    void Update()
    {
        // **TEST BUTTON: пэТА T ЦИА IMMEDIATE shake**
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("TRIGGER SHAKE!");
            TriggerShake();
        }
    }

    public void TriggerShake()
    {
        StartCoroutine(Shake());
    }

    private IEnumerator Shake()
    {
        Debug.Log("SHAKE STARTED!");
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
        Debug.Log("SHAKE ENDED!");
    }
}