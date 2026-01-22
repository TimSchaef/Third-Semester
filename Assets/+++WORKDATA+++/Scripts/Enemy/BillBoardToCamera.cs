using UnityEngine;

public class BillboardToCamera : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private bool lockX = false;
    [SerializeField] private bool lockY = false;
    [SerializeField] private bool lockZ = false;

    private void Awake()
    {
        if (!targetCamera) targetCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (!targetCamera) return;

        Vector3 forward = targetCamera.transform.forward;
        Quaternion look = Quaternion.LookRotation(forward, Vector3.up);

        Vector3 e = look.eulerAngles;
        Vector3 current = transform.rotation.eulerAngles;

        if (lockX) e.x = current.x;
        if (lockY) e.y = current.y;
        if (lockZ) e.z = current.z;

        transform.rotation = Quaternion.Euler(e);
    }
}