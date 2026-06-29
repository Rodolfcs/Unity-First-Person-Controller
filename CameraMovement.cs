using UnityEngine;

/*
Attaches to the Camera GameObject. 

Allows the user to rotate the camera vertically.
*/
public class CameraMovement : MonoBehaviour
{
    [Tooltip("The sensitivity of the mouse controlls.")]
    public float mouseSensitivity = 1;

    [Tooltip("The minimum vertical look angle (when the player is looking at the ground).")]
    public float minVerticalLookAngle = -85.0f;
    [Tooltip("The maximum vertical look angle (when the player is looking at the sky).")]
    public float maxVerticalLookAngle = 85.0f;
    
    private float _rotationX = 0;

    public GameObject playerGameObject;
    private Camera _thisCamera;

    private void Start() {
        // Get this camera.
        _thisCamera = GetComponent<Camera>();
    }

    // Update is called once per frame
    private void Update() { 
        playerGameObject.transform.Rotate(0, Input.GetAxis("Mouse X") * mouseSensitivity, 0);
        _rotationX -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        _rotationX = Mathf.Clamp(_rotationX, minVerticalLookAngle, maxVerticalLookAngle);
        
        float rotationY = transform.localEulerAngles.y;
        
        transform.localEulerAngles = new Vector3(_rotationX, rotationY, transform.localEulerAngles.z);
    }
}
