using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class Trash : MonoBehaviour
{
    [Tooltip("Minimum velocity magnitude required for the throw sound to play")]
    [SerializeField, Range(0f, 10f)] private float _minimumThrowVelocity = 2f;

    [Header("Audio Settings")]
    [Tooltip("Grab sound volume multiplier")]
    [SerializeField, Range(0f, 1f)] private float _grabSoundVolume = 0.5f;
    [Tooltip("Throw sound volume multiplier")]
    [SerializeField, Range(0f, 1f)] private float _throwSoundVolume = 0.5f;

    TrashData _data;
    Renderer _renderer;
    MeshFilter _meshFilter;
    Rigidbody _rb;
    GameObject _activeColliderSet;
    bool _markedForDespawn = true;

    public string Name => _data.Name;
    public TrashType Type => _data.Type;
    public uint Score => 1 + _data.MassScore + _data.SizeScore;

    public event Action<Trash, int> OnTrashCollected;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _meshFilter = GetComponent<MeshFilter>();
        _rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        PlayerManager.Instance.Arm.Wrist.OnTrashGrabbed += PlayFeedbackActions;
    }

    public void Initialize(TrashData data)
    {
        //Simple data
        _data = data;

        //Core data
        _meshFilter.mesh = data.Mesh;
        _renderer.material = data.Material;

        //Physics data
        ReplaceColliders(data);
        _rb.mass = data.RigidBodyMass;
        _rb.linearVelocity = Vector3.zero;

        //Visual variety data
        transform.localScale = Vector3.one * Random.Range(data.RandomSizeMultiplierRange.x, data.RandomSizeMultiplierRange.y);
        Color modifiedColor = data.RandomColorTintRange.Evaluate(Random.value);
        modifiedColor.a *= Random.Range(data.RandomColorBrightnessMultiplierRange.x, data.RandomColorBrightnessMultiplierRange.y);
        _renderer.material.color = modifiedColor;

        //Logic
        _markedForDespawn = false;
    }

    void ReplaceColliders(TrashData data)
    {
        //Disable the old colliders
        if (_activeColliderSet != null)
            _activeColliderSet.SetActive(false);

        //If data holds colliders that have already been attached, enable them
        Transform newSet = transform.Find(data.Name + "_ColliderSet");
        if (newSet != null)
        {
            _activeColliderSet = newSet.gameObject;
            _activeColliderSet.SetActive(true);
            return;
        }

        //If data holds new colliders, attach them
        GameObject colliders = Instantiate(data.ColliderPrefab, transform);
        Destroy(colliders.GetComponent<MeshRenderer>());
        Destroy(colliders.GetComponent<MeshFilter>());
        colliders.name = data.Name + "_ColliderSet";
        colliders.transform.localPosition = Vector3.zero;
        colliders.transform.localRotation = Quaternion.identity;
        colliders.transform.localScale = Vector3.one;

        SetLayerRecursively(colliders, GameConstants.Layer.Trash);

        _activeColliderSet = colliders;
    }

    void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, layer);
    }

    private void PlayFeedbackActions(Trash trash, bool isGrabbed, Vector3 handVelocity)
    {
        if (trash != this)
            return;

        //Debug.Log(gameObject.name + " played sound!");

        PlayerManager.Instance.Arm.SetArmSpeedDebuf(isGrabbed ? _data.HandSpeedMultiplier : 1f);
        if (isGrabbed && _data.PickupSound != null)
        {
			AudioManager.Instance.PlayAudio(_data.PickupSound, _grabSoundVolume, AudioPlaybackContext.PlaybackPriority.Medium, transform.position);
        }
        else if (!isGrabbed && _data.ThrowSound != null)
        {
            float velocityMagnitude = handVelocity.magnitude;

            Debug.Log($"Trash {Name} thrown. Hand velocity: {velocityMagnitude:F2} m/s (Vector: {handVelocity})");

            // Only play sound if velocity threshold are met
            if (velocityMagnitude >= _minimumThrowVelocity)
            {
                AudioManager.Instance.PlayAudio(_data.ThrowSound, _throwSoundVolume * Mathf.Clamp01(velocityMagnitude / 10f), AudioPlaybackContext.PlaybackPriority.Low, transform.position);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != GameConstants.Layer.TrashDespawnPlane)
            return;

        if (_markedForDespawn) return;
        
        _markedForDespawn = true;
        OnTrashCollected?.Invoke(this, -(int)Score);

        Debug.Log($"Trash {Name} fell to the floor. Score: {-(int)Score}");
    }

    private void OnDisable()
    {
        PlayerManager.Instance.Arm.Wrist.OnTrashGrabbed -= PlayFeedbackActions;
    }
}