using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Setting")]
    public Vector3 offset = new Vector3(10, 10, -10);
    public float smoothSpeed = 5f;

    void LateUpdate()
    {
        if(target == null)
        {
            if(PlayerController.Instance != null)
            {
                target = PlayerController.Instance.transform;
            }
            return;
        }
        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothPosition;
    }
}
