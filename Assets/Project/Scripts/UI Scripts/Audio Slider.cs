using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Audio;

public class AudioSlider : MonoBehaviour
{
    public string parameterName;        // "MasterVolume", "MusicVolume", "SFXVolume"
    public AudioMixer audioMixer;       // assign MainMixer in prefab
    public Slider slider;               // assign in prefab
    public TextMeshProUGUI valueText;   // assign in prefab

    private void Start()
    {
        // Load saved value or default
        float savedValue = PlayerPrefs.GetFloat(parameterName, 1f);
        slider.value = savedValue;
        UpdateVolume(savedValue);
        slider.onValueChanged.AddListener(UpdateVolume);
    }

    private void UpdateVolume(float value)
    {
        float dB = value <= 0.0001f ? -80f : Mathf.Log10(value) * 20f;
        audioMixer.SetFloat(parameterName, dB);

        // Update visual % text
        valueText.text = Mathf.RoundToInt(value * 100) + "%";

        // Save
        PlayerPrefs.SetFloat(parameterName, value);
    }
}
