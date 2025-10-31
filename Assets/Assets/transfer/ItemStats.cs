using UnityEngine;

public enum BinType { None = 0, Organic = 1, Plastic = 2, Paper = 3, Metal = 4, Glass = 5 }

public class ItemStats : MonoBehaviour
{
    [Header("Scoring")]
    public int Weight = 0;
    public int Size = 0;

    [Header("Target")]
    public BinType TargetBin = BinType.None;

    public int BaseTotal() => 1 + Weight + Size;
}
