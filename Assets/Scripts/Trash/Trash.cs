using UnityEngine;

public class Trash : MonoBehaviour
{
    TrashData _data;
    Renderer _renderer;
    MeshFilter _meshFilter;
    Rigidbody _rb;
    MeshCollider _collider;

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
        
    }
    
    void OnDisable()
    {
        PlayerManager.Instance.Arm.Wrist.OnTrashGrabbed -= PlayFeedbackActions;
    }
}
