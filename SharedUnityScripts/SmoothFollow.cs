using UnityEngine;

public class SmoothFollow : MonoBehaviour
{
    public Transform target;
    [Tooltip("The speed multiplied by deltaTime for movement")]
    public float smoothSpeed = 12.5f;
    [Tooltip("Offset from target")]
    public Vector3 offset;
    Vector3 velocity = Vector3.zero;
    private Vector3 pos, fw; //Position, forward

    private void Start()
    {
        pos = target.transform.InverseTransformPoint(transform.position);
        fw = target.transform.InverseTransformDirection(transform.forward);
    }
    private void LateUpdate()
    {

        Vector3 desiredPosition = target.transform.TransformPoint(pos + offset);
        Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothSpeed *Time.deltaTime);

        var newfw = target.transform.TransformDirection(fw);
        var newrot = Quaternion.LookRotation(newfw);

        transform.position = smoothedPosition;
        transform.rotation = newrot;

    }

}
