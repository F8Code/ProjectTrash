using UnityEngine;

[CreateAssetMenu(fileName = "TrashData", menuName = "Scriptable Objects/TrashData")]
public class TrashData : ScriptableObject
{
    [Header("Core data")]
    public string Name;
    public TrashType Type;
    public Mesh Mesh;
    public Material Material;
    //Potentially add a 2d sprite for UI

    [Header("Physics data")]
    public PhysicsMaterial PhysicsMaterial;
    public float RigidBodyMass;

    [Header("Feedback data")]
    public float HandSpeedMultiplier;
    public AudioClip PickupSound;
    public AudioClip ThrowSound;

    [Header("Visual variety data")]
    public Vector2 RandomSizeMultiplierRange;
    public Gradient RandomColorTintRange;
    public Vector2 RandomColorBrightnessMultiplierRange;

    [Header("Scoring data")]
    public uint MassValue;
    public uint SizeValue;
    public uint Score;
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