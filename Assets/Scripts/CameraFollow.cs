using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;          // The player to follow
    public float smoothSpeed = 5f;    // Follow smoothing
    public Vector3 offset = new Vector3(0, 0, -10f); // Ensure Z is -10

    void LateUpdate()
    {
        if (target == null) return;

        // Keep the offset intact (so camera stays at Z = -10)
        Vector3 desiredPosition = target.position + offset;

        // Smoothly move the camera
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }

    // Call this when assigning a player
    public void SetTarget(GameObject newTarget)
    {
        target = newTarget.transform;
        // Keep current Z offset intact
        offset = new Vector3(offset.x, offset.y, -10f);
    }
}
