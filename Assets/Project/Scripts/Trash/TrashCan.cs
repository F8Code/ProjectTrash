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
    [SerializeField] Color _highlightColor;

    [Header("VFX Success - ΝΕΟ!")]
    [SerializeField] ParticleSystem successVFXPrefab; // Σύρε το Particle System prefab εδώ

    [Header("References")]
    [SerializeField] PlayerWrist _playerWrist;

    HashSet<Trash> _ignoredTrash = new();
    public event Action<Trash, int> OnTrashCollected;

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

        // **VFX TRIGGER: Μόνο αν ΣΩΣΤΟ match!**
        if (trash.Type == _acceptedTrash && successVFXPrefab != null)
        {
            // Instantiate VFX στο bin position + λίγο πάνω
            ParticleSystem vfx = Instantiate(successVFXPrefab,
                                           transform.position + Vector3.up * 0.5f,
                                           Quaternion.identity);
            vfx.Play();
            Destroy(vfx.gameObject, vfx.main.duration); // Auto-destroy
            Debug.Log("🎉 SUCCESS VFX PLAYED!");
        }

        OnTrashCollected?.Invoke(trash, (trash.Type == _acceptedTrash ? 1 : -1) * (int)trash.Score);
        Debug.Log($"Trash {trash.Name} collected. Score: {(trash.Type == _acceptedTrash ? 1 : -1) * (int)trash.Score}");
    }

    public void StopIgnoringTrash(Trash trash) => _ignoredTrash.Remove(trash);

    private void HandleTrashGrabbed(Trash grabbedTrash, bool isGrabbed, Vector3 velocity)
    {
        if (isGrabbed && grabbedTrash.Type == _acceptedTrash)
        {
            StartCoroutine(HighlightLightCoroutine(_highlightColor));
        }
    }

    private IEnumerator HighlightLightCoroutine(Color color)
    {
        if (_highlightLight == null)
        {
            Debug.LogWarning("No light assigned!");
            yield break;
        }

        float originalIntensity = _highlightLight.intensity;
        Color originalColor = _highlightLight.color;

        _highlightLight.intensity *= 8f;
        //_highlightLight.color = color;

        yield return new WaitForSeconds(0.5f);

        _highlightLight.intensity = originalIntensity;
        //_highlightLight.color = originalColor;

        yield break;
    }
}