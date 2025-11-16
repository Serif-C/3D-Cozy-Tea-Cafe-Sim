using UnityEngine;

public class BillBoard : MonoBehaviour
{
    // Might also have to billboard the customer/player sprites to achieve 2.5D effect

    private Transform cam;

    private void Start()
    {
        cam = Camera.main.transform;
    }

    private void LateUpdate()
    {
        if (cam == null) { return; }

        // Option A: Look directly at camera
        Vector3 direction = transform.position - cam.position;
        direction.y = 0f; // keep it upright if you want
        transform.rotation = Quaternion.LookRotation(direction);
    }
}
