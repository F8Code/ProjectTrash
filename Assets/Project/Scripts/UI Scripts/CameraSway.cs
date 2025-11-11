using UnityEngine;

// This script makes the menu camera move gently left and right
// to create a calm, dynamic background effect in the main menu.
public class MenuCameraSway : MonoBehaviour
{
    // How fast the camera moves back and forth
    public float speed = 1f;

    // How far the camera moves from the starting point
    public float amount = 2f;

    // The camera’s original position when the scene starts
    private Vector3 startPos;

    void Start()
    {
        // Save the starting position so we know where to move from
        startPos = transform.position;
    }

    void Update()
    {
        // Create a smooth back-and-forth value using a sine wave
        float offset = Mathf.Sin(Time.time * speed) * amount;

        // Apply that offset to the X position to move left and right
        transform.position = startPos + new Vector3(offset, 0f, 0f);
    }
}
