
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class ScoreZone : MonoBehaviour
{

    [SerializeField] private bool useTagCheck = true;


    public string AcceptsTag = "Plastic";

    [SerializeField] private bool compareOnItemRoot = true;

    public BinType AcceptsBin = BinType.None;

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

        if (ScoreManager.I == null)
        {
            Debug.LogWarning("[ScoreZone] No ScoreManager.");
            return;
        }

        int id = other.gameObject.GetInstanceID();
        if (ScoreManager.I.HasCounted(id)) return;

        bool correct;

        if (useTagCheck)
        {
          
            GameObject goForTag = compareOnItemRoot ? item.gameObject : other.gameObject;

           
            if (string.IsNullOrWhiteSpace(AcceptsTag))
            {
                Debug.LogWarning($"[ScoreZone] AcceptsTag is empty. '{name}' wont match.");
                return;
            }

            
            correct = goForTag.CompareTag(AcceptsTag);
        }
        else
        {
         
            correct = (item.TargetBin == AcceptsBin);
        }

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
