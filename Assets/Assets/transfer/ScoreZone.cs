using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class ScoreZone : MonoBehaviour
{
    [Header("This Zone Accepts")]
    public BinType Accepts = BinType.None;

    [Header("Options")]
    [SerializeField] private bool consumeOnDeposit = true;

    private void Reset()
    {
        var col = GetComponent<BoxCollider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        var item = other.GetComponent<ItemStats>();
        if (item == null) return;

        if (ScoreManager.I == null) { Debug.LogWarning("[ScoreZone] ScoreManager yok."); return; }

        int id = other.gameObject.GetInstanceID();
        if (ScoreManager.I.HasCounted(id)) return;

        bool correct = (item.TargetBin == Accepts);

        if (correct)
        {
            ScoreManager.I.MarkCounted(id);
            ScoreManager.I.RegisterCorrectDeposit(item.BaseTotal());

            if (consumeOnDeposit) Destroy(other.gameObject);
       
        }
        else
        {
            ScoreManager.I.RegisterWrongDeposit();
        }
    }
}
