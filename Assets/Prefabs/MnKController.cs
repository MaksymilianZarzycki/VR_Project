using UnityEngine;
using UnityEngine.InputSystem;

public class MnKController : MonoBehaviour
{
    public InputActionProperty mouseX;
    public InputActionProperty mouseY;
    [SerializeField] public Transform camera_t;
    [SerializeField] public float mouseSensitivity = 2f;

    private float cam_rot_v = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        float inputX = mouseX.action.ReadValue<float>() * mouseSensitivity;
        float inputY = mouseY.action.ReadValue<float>() * mouseSensitivity;

        cam_rot_v -= inputY;
        cam_rot_v = Mathf.Clamp(cam_rot_v, -90f, 90f);
        camera_t.localEulerAngles = Vector3.right * cam_rot_v;
        transform.localEulerAngles += Vector3.up * inputX;
    }
}
