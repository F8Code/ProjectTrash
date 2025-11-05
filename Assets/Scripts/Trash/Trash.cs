using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class Trash : MonoBehaviour
{
    [Header("Audio settings")]
    [Tooltip("Minimum angle of the thrash throw trajectory for the throw sound to play")]
    [SerializeField, Range(0, 80)] uint _minimumGroundAngleToPlayThrowSound = 45;
    [Tooltip("Grab sound volume multiplier")]
    [SerializeField, Range(0f, 2f)] float _grabSoundVolume = 1f;
    [Tooltip("Throw sound volume multiplier")]
    [SerializeField, Range(0f, 2f)] float _throwSoundVolume = 1f;

    TrashData _data;
    Renderer _renderer;
    MeshFilter _meshFilter;
    Rigidbody _rb;
    MeshCollider _collider;

    public string Name => _data.Name;
    public TrashType Type => _data.Type;
    public uint Score => 1 + _data.MassScore + _data.SizeScore;

    public event Action<Trash, int> OnTrashCollected;

    void Awake()
    {
        _renderer = GetComponentInChildren<Renderer>();
        _meshFilter = GetComponentInChildren<MeshFilter>();
        _rb = GetComponent<Rigidbody>();
        _collider = GetComponent<MeshCollider>();
    }

    void OnEnable()
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

    void PlayFeedbackActions(bool isGrabbed)
    {
        PlayerManager.Instance.Arm.SetArmSpeedDebuf(isGrabbed ? _data.HandSpeedMultiplier : 1f);
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != GameConstants.Layer.TrashDespawnPlane)
            return;

        OnTrashCollected?.Invoke(this, -(int)Score);

        Debug.Log($"Trash {Name} fell to the floor. Score: {-(int)Score}");
    }
    
    void OnDisable()
    {
        PlayerManager.Instance.Arm.Wrist.OnTrashGrabbed -= PlayFeedbackActions;
    }
}
