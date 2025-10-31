using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandMouseFollower : MonoBehaviour
{
    [Header("Settings")]
    public bool useLerp = false;
    public float followSpeed = 50f;
    public float heightOffset = 0.1f;
    public Vector3 positionOffset = Vector3.zero;

    [Header("Raycast Settings")]
    public LayerMask surfaceMask = -1;
    public float maxRayDistance = 100f;

    [Header("Fallback")]
    public bool useFallbackPlane = true;
    public float fallbackPlaneY = 0f;

    [Header("BOUNDS Restriction - ΝΕΟ!")]
    public bool useBounds = true; // Ενεργοποίησε/απενεργοποίησε bounds
    public Bounds playArea = new Bounds(Vector3.zero, new Vector3(10f, 0f, 10f)); // Κέντρο (0,0,0), Μέγεθος (10x0x10) - ρύθμισε εδώ

    private Camera cam;

    void Start()
    {
        cam = Camera.main;
        if (cam == null) Debug.LogError("No Main Camera!");

        if (surfaceMask == 0) surfaceMask = -1;

        // Debug: Εκτύπωσε bounds για check
        Debug.Log("Hand Bounds: Min=" + playArea.min + " Max=" + playArea.max);
    }

    void Update()
    {
        Vector3 targetPos = GetMouseHitPosition();
        targetPos += positionOffset;

        // **ΚΛΕΙΔΙ: Clamp σε bounds ΠΡΙΝ set position**
        if (useBounds)
        {
            targetPos = ClampToBounds(targetPos);
        }

        if (useLerp)
        {
            transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);
        }
        else
        {
            transform.position = targetPos;
        }
    }

    // **ΝΕΑ FUNCTION: Περιορισμός σε Bounds**
    private Vector3 ClampToBounds(Vector3 pos)
    {
        pos.x = Mathf.Clamp(pos.x, playArea.min.x, playArea.max.x);
        pos.y = Mathf.Clamp(pos.y, playArea.min.y, playArea.max.y);
        pos.z = Mathf.Clamp(pos.z, playArea.min.z, playArea.max.z);
        return pos;
    }

    private Vector3 GetMouseHitPosition()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxRayDistance, surfaceMask))
        {
            return hit.point + Vector3.up * heightOffset;
        }

        if (useFallbackPlane)
        {
            return GetMouseWorldPosPlane(fallbackPlaneY);
        }

        return transform.position;
    }

    private Vector3 GetMouseWorldPosPlane(float planeY)
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, planeY);
        float distance;
        if (plane.Raycast(ray, out distance))
        {
            return ray.GetPoint(distance);
        }
        return transform.position;
    }

    // **Gizmos: ΒΛΕΠΕΙΣ τα bounds στο Scene View (πράσινο box)**
    void OnDrawGizmosSelected()
    {
        if (useBounds)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(playArea.center, playArea.size);
        }
    }
}