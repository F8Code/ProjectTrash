using UnityEngine;

public class Trash : MonoBehaviour
{
    TrashData _data;

    public void Initialize(TrashData data)
    {
        _data = data;
        ApplyData();
    }
    
    private void ApplyData()
    {
        /*// Ustaw model i materiały
        if (_data.modelPrefab != null)
        {
            // Możesz wstawić model jako dziecko
            var model = Instantiate(_data.modelPrefab, transform);
        }

        var renderer = GetComponentInChildren<Renderer>();
        if (renderer && _data.material != null)
            renderer.material = _data.material;

        var collider = GetComponent<Collider>();
        if (collider && _data.physicsMaterial != null)
            collider.material = _data.physicsMaterial;

        // Dodatkowe właściwości typu specjalnego
        _data.OnCollected(); // przykład użycia logiki polimorficznej*/
    }
}
