using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DraggableObject : MonoBehaviour
{
    private bool isDragging = false;
    private Vector3 offset;
    private Rigidbody rb;

    // Για Throw: Track ταχύτητα mouse
    private Vector3 lastMousePos;
    private Vector3 mouseVelocity;
    public float throwMultiplier = 5f; // Ρύθμισε για δύναμη throw

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Υπολόγισε mouse velocity
        Vector3 currentMousePos = GetMouseWorldPos();
        mouseVelocity = (currentMousePos - lastMousePos) / Time.deltaTime;
        lastMousePos = currentMousePos;

        // Κλικ down
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform == transform)
                {
                    isDragging = true;
                    rb.isKinematic = true;

                    Vector3 mouseWorldPos = GetMouseWorldPos();
                    offset = transform.position - mouseWorldPos;
                }
            }
        }

        // Κλικ up (Release & Throw)
        if (Input.GetMouseButtonUp(0))
        {
            if (isDragging)
            {
                isDragging = false;
                rb.isKinematic = false;

                Vector3 throwVelocity = mouseVelocity * throwMultiplier;
                rb.linearVelocity = throwVelocity;
            }
        }

        // Drag
        if (isDragging)
        {
            Vector3 mouseWorldPos = GetMouseWorldPos();
            transform.position = mouseWorldPos + offset;
        }
    }

    private Vector3 GetMouseWorldPos()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, 0);
        float distance;
        if (plane.Raycast(ray, out distance))
        {
            return ray.GetPoint(distance);
        }
        return Vector3.zero;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Έλεγχος αν collision με Bin (tag "Bin")
        if (collision.gameObject.CompareTag("Bin"))
        {
            // Πάρε colors
            TrashColor myColor = GetComponent<TrashColor>();
            TrashColor binColor = collision.gameObject.GetComponent<TrashColor>();

            if (myColor != null && binColor != null && myColor.color == binColor.color)
            {
                // Ίδιο χρώμα: Destroy + point
                Debug.Log("+1 point! (Color match: " + myColor.color + ")");
                Destroy(gameObject);
            }
            else
            {
                Debug.Log("No match or missing color component!");
            }
        }
    }
}