using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject panelStartScreen;
    public GameObject panelContract;

    void Start()
    {
        // Se till att rätt paneler är aktiva i början
        panelStartScreen.SetActive(true);
        panelContract.SetActive(false);
    }

    public void OnPlayPressed()
    {
        panelStartScreen.SetActive(false);
        panelContract.SetActive(true);
    }

    public void OnContractDone()
    {
        // När 
        // kontraktet är signerat eller animationen är klar
        SceneManager.LoadScene("GameScene");
    }
}
