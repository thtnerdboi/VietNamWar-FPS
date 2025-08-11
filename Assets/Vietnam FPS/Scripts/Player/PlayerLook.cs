
using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    public Transform cameraPivot;
    public float sensitivity = 2f;
    public float minPitch = -80f;
    public float maxPitch = 80f;

    float yaw;
    float pitch;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        yaw = transform.eulerAngles.y;
        pitch = 0f;
    }

    void LateUpdate()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        yaw += mouseX * sensitivity * 10f * Time.deltaTime;
        pitch -= mouseY * sensitivity * 10f * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        transform.rotation = Quaternion.Euler(0, yaw, 0);
        if (cameraPivot != null)
            cameraPivot.localRotation = Quaternion.Euler(pitch, 0, 0);
    }
}
