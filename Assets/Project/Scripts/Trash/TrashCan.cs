using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashCan : MonoBehaviour
{
    [Header("Core settings")]
    [Tooltip("Type of trash that is accepted by this can")]
    [SerializeField] TrashType _acceptedTrash;

    [Header("Light Settings")]
    [SerializeField] Light _highlightLight;
    [SerializeField] Color _highlightColor = Color.yellow;

    [Header("VFX Success")]
    [SerializeField] ParticleSystem successVFXPrefab;

    [Header("References")]
    [SerializeField] PlayerWrist _playerWrist;

    HashSet<Trash> _ignoredTrash = new();
    public event Action<Trash, int> OnTrashCollected;

    private bool _isHighlighted = false; // Track αν είναι ενεργό

    void Awake()
    {
        // **ΒΗΜΑ 1: Σβήσε το φως από την αρχή**
        if (_highlightLight != null)
        {
            _highlightLight.enabled = false;
        }
    }

    void OnEnable()
    {
        if (_playerWrist != null)
        {
            _playerWrist.OnTrashGrabbed += HandleTrashGrabbed;
        }
        else
        {
            Debug.LogError("No PlayerWrist reference assigned in " + gameObject.name);
        }
    }

    void OnDisable()
    {
        if (_playerWrist != null)
        {
            _playerWrist.OnTrashGrabbed -= HandleTrashGrabbed;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != GameConstants.Layer.Trash) return;

        Trash trash = other.GetComponentInParent<Trash>();
        if (_ignoredTrash.Contains(trash)) return;

        _ignoredTrash.Add(trash);

        // **VFX: Μόνο αν σωστός κάδος**
        if (trash.Type == _acceptedTrash && successVFXPrefab != null)
        {
            ParticleSystem vfx = Instantiate(successVFXPrefab,
                                           transform.position + Vector3.up * 0.5f,
                                           Quaternion.identity);
            vfx.Play();
            Destroy(vfx.gameObject, vfx.main.duration);
            Debug.Log("SUCCESS VFX PLAYED!");
        }

        OnTrashCollected?.Invoke(trash, (trash.Type == _acceptedTrash ? 1 : -1) * (int)trash.Score);
        Debug.Log($"Trash {trash.Name} collected. Score: {(trash.Type == _acceptedTrash ? 1 : -1) * (int)trash.Score}");
    }

    public void StopIgnoringTrash(Trash trash) => _ignoredTrash.Remove(trash);

    // **ΒΗΜΑ 2: Ενεργοποίηση/Απενεργοποίηση φωτός**
    private void HandleTrashGrabbed(Trash grabbedTrash, bool isGrabbed, Vector3 velocity)
    {
        if (grabbedTrash.Type == _acceptedTrash)
        {
            if (isGrabbed && !_isHighlighted)
            {
                TurnOnLight();
            }
            else if (!isGrabbed && _isHighlighted)
            {
                TurnOffLight();
            }
        }
    }

    private void TurnOnLight()
    {
        if (_highlightLight == null) return;

        _highlightLight.enabled = true;
        _highlightLight.color = _highlightColor;
        _highlightLight.intensity = 8f; // Φωτεινό
        _isHighlighted = true;

        StopAllCoroutines(); // Ακύρωσε fade αν τρέχει
        StartCoroutine(FadeLight(8f, 0.2f));
    }

    private void TurnOffLight()
    {
        if (_highlightLight == null) return;

        StopAllCoroutines();
        StartCoroutine(FadeLight(0f, 0.3f, () => _highlightLight.enabled = false));
        _isHighlighted = false;
    }

    // **Ομαλό fade (Lerp)**
    private IEnumerator FadeLight(float targetIntensity, float duration, System.Action onComplete = null)
    {
        float startIntensity = _highlightLight.intensity;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            _highlightLight.intensity = Mathf.Lerp(startIntensity, targetIntensity, elapsed / duration);
            yield return null;
        }

        _highlightLight.intensity = targetIntensity;
        onComplete?.Invoke();
    }
}