
using UnityEngine;

public class RainFollowCamera : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Vector3 offset = new Vector3(0f, 5f, 5f); // in front of camera

    private void LateUpdate()
    {
        if (cameraTransform == null) return;

        transform.position = cameraTransform.position + offset;
    }
}