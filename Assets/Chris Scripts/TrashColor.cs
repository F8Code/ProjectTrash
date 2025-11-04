using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashColor : MonoBehaviour
{
    public string color; // Βάλε εδώ "blue", "gray", "teal", "orange" ή "magenta" στο Inspector

    void Start()
    {
        if (string.IsNullOrEmpty(color))
        {
            Debug.LogWarning("Color not set on " + gameObject.name);
        }
    }
}