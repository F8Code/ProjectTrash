using UnityEngine;

[CreateAssetMenu(fileName = "TrashData", menuName = "Scriptable Objects/TrashData")]
public class TrashData : ScriptableObject
{
    [Header("Core data")]
    [Tooltip("Name of the trash, used mainly in UI and debugging")]
    public string Name;

    [Tooltip("Type of trash used for matching with trash cans")]
    public TrashType Type;

    [Tooltip("Mesh representing the trash visually")]
    public Mesh Mesh;

    [Tooltip("Material applied to the trash mesh")]
    public Material Material;

    [Header("Physics data")]
    [Tooltip("Prefab with a uniform scale 1 containing only colliders for this trash type")]
    public GameObject ColliderPrefab;

    [Tooltip("Physics material assigned to the collider")]
    public PhysicsMaterial PhysicsMaterial;

    [Tooltip("Mass of the trash in Rigidbody")]
    [Range(0.1f, 20f)]
    public float RigidBodyMass = 1f;

    [Header("Feedback data")]
    [Tooltip("Multiplier for hand movement speed during interaction")]
    [Range(0.1f, 1f)]
    public float HandSpeedMultiplier = 1f;

    [Header("Audio Settings")]
    [Tooltip("Sound played when picking up the trash")]
    public AudioClip PickupSound;
    [Tooltip("Sound played when throwing the trash")]
    public AudioClip ThrowSound;

    [Header("Visual variety data")]
    [Tooltip("Range of random size variation for the trash")]
    public Vector2 RandomSizeMultiplierRange = new Vector2(0.9f, 1.1f);

    [Tooltip("Gradient used for random color tinting of the trash")]
    public Gradient RandomColorTintRange;

    [Tooltip("Range of random brightness variation for the trash")]
    public Vector2 RandomColorBrightnessMultiplierRange = new Vector2(0.8f, 1.2f);

    [Header("Scoring data")]
    [Tooltip("Points awarded based on the mass of the trash")]
    [Range(0, 4)]
    public uint MassScore;

    [Tooltip("Points awarded based on the size of the trash")]
    [Range(0, 4)]
    public uint SizeScore;
}

public enum TrashType
{
    Plastic,
    Paper,
    Metal,
    Glass,
    Hazard,
    Mixed
}