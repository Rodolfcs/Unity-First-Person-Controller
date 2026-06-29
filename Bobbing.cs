using UnityEngine;

public class Bobbing : MonoBehaviour {
    [Header("Camera side tilt effect")]
    public float maxCameraTiltAngle = 3f;
    public float cameraTiltSpeed = 10f;

    [Header("Bobbing settings")]
    [Tooltip("The cameras relative y-position to the player capsule. After placing the camera around the peak of the capsule, with a 1x1x1 capsule, this should be around 1.")]
    public float cameraBodyOffset = 1f;

    public float xBobbingDistanceMultiplier = 0.5f;
    public float yBobbingDistanceMultiplier = 1f;
    public static float currentBobbingSpeed;
    private float _x;
    private float _y;

    private void Start() {
        // Avoid the camera snapping at the start.
        transform.localPosition = new Vector3(transform.localPosition.x, cameraBodyOffset, transform.localPosition.z);
    }

    private void Update() {
    // If you only want one and not the other, comment out the other. 
        DoCamTilt();
        DoBobbing();
    }

    private void DoCamTilt() {
        //Debug.Log(transform.localRotation.eulerAngles.ToString());
        var currentRotation = transform.localRotation.eulerAngles;
        currentRotation.z = (currentRotation.z > 180) ? currentRotation.z - 360 : currentRotation.z;

        // If player is moving right.
        if (PlayerMovement.xMovement > 0) {
            // If we haven't reached the max tilt angle.
            if (currentRotation.z < -maxCameraTiltAngle) {
                return;
            }
            currentRotation += new Vector3(0, 0, -cameraTiltSpeed * Time.deltaTime);
            transform.localRotation = Quaternion.Euler(currentRotation);
        }
        // If player is moving left.
        else if (PlayerMovement.xMovement < 0) {
            // If we haven't reached the max tilt angle.
            if (currentRotation.z >= maxCameraTiltAngle) {
                return;
            }
            currentRotation += new Vector3(0, 0, cameraTiltSpeed * Time.deltaTime);
            transform.localRotation = Quaternion.Euler(currentRotation);
        }
        // If player is not moving (return to original rotation).
        else {
            // If the camera is currently tilted right.
            if (currentRotation.z < -0.1f) {
                currentRotation += new Vector3(0, 0, cameraTiltSpeed * Time.deltaTime);
                transform.localRotation = Quaternion.Euler(currentRotation);
            }
            // If the camera is currently tilted left.
            else if (currentRotation.z > 0.1f) {
                currentRotation += new Vector3(0, 0, -cameraTiltSpeed * Time.deltaTime);
                transform.localRotation = Quaternion.Euler(currentRotation);
            }
        }
    }

    private void DoBobbing() {
        // If the player isn't moving or on the ground, don't bob the camera.
        if (!PlayerMovement.isMovingY | !PlayerMovement.isGrounded) {
            return;
        }

        // Reset x and y values so they don't go on forever (reset so it's a perfect cycle).
        if (_x >= 2 * Mathf.PI) {
            _x = 0;
        }
        if (_y >= 2 * Mathf.PI) {
            _y = 0;
        }

        // Set the camera position at the place of the (infinity sign) movement.
        transform.localPosition =
            new Vector3(Mathf.Sin(_x += Time.deltaTime * currentBobbingSpeed) * xBobbingDistanceMultiplier,
                cameraBodyOffset + Mathf.Sin(2 * (_y += Time.deltaTime * currentBobbingSpeed)) * yBobbingDistanceMultiplier,
                0);
    }
}
