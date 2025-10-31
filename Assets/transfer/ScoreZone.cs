
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class ScoreZone : MonoBehaviour
{
    [Header("Checking Mode")]
    [Tooltip("Açıksa, tag ile doğrular. Kapalıysa enum ile doğrular.")]
    [SerializeField] private bool useTagCheck = true;

    [Header("Tag Mode")]
    [Tooltip("Karşılaştırılacak tag (Tag Manager'da oluşturulmuş olmalı).")]
    public string AcceptsTag = "Plastic";

    [Tooltip("Tag'i item'ın root GameObject'inden mi oku? (Önerilir)")]
    [SerializeField] private bool compareOnItemRoot = true;

    [Header("Enum Mode")]
    public BinType AcceptsBin = BinType.None;

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

        if (ScoreManager.I == null)
        {
            Debug.LogWarning("[ScoreZone] ScoreManager yok.");
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
                Debug.LogWarning($"[ScoreZone] AcceptsTag boş. '{name}' doğru eşleşmeyecek.");
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
