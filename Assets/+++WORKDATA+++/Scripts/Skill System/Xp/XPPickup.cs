using UnityEngine;

public class XPPickup : MonoBehaviour
{
    public int xpAmount = 10;

    [Header("Pickup")]
    public string playerTag = "Player";
    public bool destroyOnPickup = true;

    [Header("Billboard")]
    public bool faceCamera = true;
    public bool lockYAxis = false; 
    public bool flipForward180 = false; 

    private Transform cam;

    private void LateUpdate()
    {
        if (!faceCamera) return;

        if (cam == null)
        {
            var c = Camera.main;
            if (c == null) return;             
            cam = c.transform;
        }

        if (lockYAxis)
        {
           
            Vector3 dir = cam.position - transform.position;
            dir.y = 0f;

           
            if (dir.sqrMagnitude < 0.0001f)
                dir = Vector3.ProjectOnPlane(cam.forward, Vector3.up);

            transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
        }
        else
        {
            
            transform.rotation = Quaternion.LookRotation(-cam.forward, cam.up);
        }

        if (flipForward180)
            transform.Rotate(0f, 180f, 0f, Space.Self);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        var giver = other.GetComponent<XPGiver>();
        if (giver == null) return;

        giver.GrantXP(xpAmount);
        if (destroyOnPickup) Destroy(gameObject);
    }
}



