using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class Trash : MonoBehaviour
{
    [Header("Audio settings")]
    [Tooltip("Minimum velocity magnitude required for the throw sound to play")]
    [SerializeField, Range(0f, 10f)] private float _minimumThrowVelocity = 2f;

    [Tooltip("Grab sound volume multiplier")]
    [SerializeField, Range(0f, 1f)] private float _grabSoundVolume = 1f;

    [Tooltip("Throw sound volume multiplier")]
    [SerializeField, Range(0f, 1f)] private float _throwSoundVolume = 1f;

    private TrashData _data;
    private Renderer _renderer;
    private MeshFilter _meshFilter;
    private Rigidbody _rb;
    private MeshCollider _collider;

    public string Name => _data.Name;
    public TrashType Type => _data.Type;
    public uint Score => 1 + _data.MassScore + _data.SizeScore;

    public event Action<Trash, int> OnTrashCollected;

    private void Awake()
    {
        _renderer = GetComponentInChildren<Renderer>();
        _meshFilter = GetComponentInChildren<MeshFilter>();
        _rb = GetComponent<Rigidbody>();
        _collider = GetComponent<MeshCollider>();
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
        _collider.sharedMesh = data.Mesh;
        _collider.material = data.PhysicsMaterial;
        _rb.mass = data.RigidBodyMass;

        //Visual variety data
        transform.localScale = Vector3.one * Random.Range(data.RandomSizeMultiplierRange.x, data.RandomSizeMultiplierRange.y);
        Color modifiedColor = data.RandomColorTintRange.Evaluate(Random.value);
        modifiedColor.a *= Random.Range(data.RandomColorBrightnessMultiplierRange.x, data.RandomColorBrightnessMultiplierRange.y);
        _renderer.material.color = modifiedColor;
    }

    private void PlayFeedbackActions(bool isGrabbed, Vector3 handVelocity)
    {
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

        OnTrashCollected?.Invoke(this, -(int)Score);

        Debug.Log($"Trash {Name} fell to the floor. Score: {-(int)Score}");
    }

    private void OnDisable()
    {
        PlayerManager.Instance.Arm.Wrist.OnTrashGrabbed -= PlayFeedbackActions;
    }
}