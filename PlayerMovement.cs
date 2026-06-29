using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    [Header("  ---    Movement   ---")]
    public bool enableRunning = false;
    public float walkingSpeed = 6f;
    public float runningSpeed = 10f;

    [Header("\n  ---    Jumping   ---")]
    public bool enableJump = false;
    public bool enableDoubleJump = false;
    public float jumpForce = 10f;
    public float doubleJumpForce = 15f;
    public float gravityForce = 10f;
    public static bool canDoubleJump = true;

    [Header("\n  ---    Dashing   ---")]
    public bool enableDashing = false;
    public float dashSpeed = 2000;
    private static bool _isDashing;
    public float dashingDistance = 0.1f;
    public static bool canDash = true;

    // Delete [HideInInspector] this if you want to access those all the time (for testing or whatever).
    [Header("\n  ---    isGrounded Check settings   ---")]
    [HideInInspector] public int raycastCount = 18;
    [HideInInspector] public float playerRadius = 0.6f;
    [HideInInspector] public float playerHeight = 1.1f;

    [Header("\n   ---   Audio   ---")]
    public bool enableSounds = false;
    public AudioSource[] footstepsList;
    public float footstepVolumeMin = 0.9f;
    public float footstepVolumeMax = 1.1f;
    public float footstepSpeedMin = 0.9f;
    public float footstepSpeedMax = 1.1f;
    
    [Tooltip("Number of frames with no footstep sound between each footstep. (very framerate dependent)")]
    public int footstepDelay = 150;

    public AudioSource jumpSound;
    public AudioSource dashSound;
    
    [Header("\n  ---    Miscellaneous    ---")]
    public float bobbingSpeed = 2f;
    public bool lockMouse = true;


    /** PRIVATE --- PUBLIC STATIC **/
    private float _currentSpeed;
    private float _footstepDelayer;
    private float _currentFootstepSoundVolumeMin;
    private float _currentFootstepSoundVolumeMax;
    private float _currentFootstepDelay;
    private static CharacterController _cc;
    private Vector3 _gravVector = Vector3.zero;
    private float _dashSpeed;
    public static bool isRunning;
    public static bool isJumping;
    public static float xMovement;
    public static bool isMovingY;
    public static bool isMovingX;
    public static bool isGrounded;

    private void Start() {
        Application.targetFrameRate = 240;
        _cc = GetComponent<CharacterController>();
        _currentSpeed = walkingSpeed;
        _footstepDelayer = footstepDelay;
        canDash = true;
        canDoubleJump = true;
        Bobbing.currentBobbingSpeed = bobbingSpeed;
    }

    private void Update() {
        isGrounded = IsGrounded();
        Movement();
        Jumping();
        if (enableSounds) {
            Sounds();
        }
        if (lockMouse) {
            CursorLocking();
        }
    }

    private void Movement() {
        xMovement = Input.GetAxis("Horizontal");
        isMovingX = Mathf.Abs(Input.GetAxis("Horizontal")) > 0;
        isMovingY = Mathf.Abs(Input.GetAxis("Vertical")) > 0;

        Vector3 moveVector = transform.forward * Input.GetAxis("Vertical") + transform.right * Input.GetAxis("Horizontal");
        Vector3 dashVector = Camera.main.transform.forward * _dashSpeed;

        if (enableRunning) Running();
        _cc.Move((Vector3.ClampMagnitude(moveVector * _currentSpeed, _currentSpeed) + dashVector) * Time.deltaTime);

        if (enableDashing && Input.GetKeyDown(KeyCode.Q) && !_isDashing && canDash) {
            StartCoroutine(Dash());
        }
    }

    private void Running() {
        // If player clicks shift and is moving: run.
        if (Input.GetKey(KeyCode.LeftShift) && (isMovingX | isMovingY)) {
            isRunning = true;

            // Set current movement speed to runningSpeed;
            _currentSpeed = runningSpeed;

            // Change (raise) the footstep volume.
            _currentFootstepSoundVolumeMin = footstepVolumeMin * 2f;
            _currentFootstepSoundVolumeMax = footstepVolumeMax * 2f;

            // Make more (number) footstep sounds per time.
            _currentFootstepDelay = (int)(footstepDelay / 2f);

            // Make the bobbing faster.
            Bobbing.currentBobbingSpeed = bobbingSpeed * 1.5f;
        }
        else {
            isRunning = false;
            _currentSpeed = walkingSpeed;
            _currentFootstepSoundVolumeMin = footstepVolumeMin;
            _currentFootstepSoundVolumeMax = footstepVolumeMax;
            _currentFootstepDelay = footstepDelay;
            Bobbing.currentBobbingSpeed = bobbingSpeed;
        }
    }

    private void Jumping() {
        if (IsGrounded()) {
            canDoubleJump = true;

            if (enableJump && Input.GetKeyDown(KeyCode.Space) && !isJumping && !_isDashing) {
                if (enableSounds) { jumpSound.Play(); }
                isJumping = true;
                _gravVector = Vector3.up * jumpForce;
            }
            else {
                isJumping = false;
            }
        }
        else { // If in the air.
            if (enableDoubleJump && Input.GetKeyDown(KeyCode.Space) && !_isDashing && canDoubleJump) {
                if (enableSounds) { jumpSound.Play(); }
                _gravVector = Vector3.up * doubleJumpForce;
                canDoubleJump = false;
            }
            else if (!_isDashing) {
                _gravVector += Vector3.down * (gravityForce * Time.deltaTime);
            }
            isJumping = true;
        }

        _cc.Move(_gravVector * Time.deltaTime);
    }

    private List<Vector3> GetRayPositions() {
        List<Vector3> positions = new List<Vector3>();

        // Calculate the angle between each raycast point (in radians).
        float angleStep = 2 * Mathf.PI / raycastCount;

        for (int i = 0; i < raycastCount; i++) {
            // Calculate the angle for this point.
            float angle = i * angleStep;

            // Get x and z coordinates based on the angle and radius.
            float x = Mathf.Cos(angle) * playerRadius;
            float z = Mathf.Sin(angle) * playerRadius;

            // Create a vector for the position around the player.
            Vector3 offset = new Vector3(transform.position.x + x, transform.position.y, transform.position.z + z);

            // Add the position relative to the player's base.
            positions.Add(offset);
        }

        return positions;
    }

    private bool IsGrounded() {
        List<Vector3> rayOrigins = GetRayPositions();

        foreach (Vector3 rayOrigin in rayOrigins) {
            if (Physics.Raycast(rayOrigin, Vector3.down, playerHeight, ~LayerMask.GetMask("PlayerSelf"))) {
                return true;
            }
        }

        return false;
    }

    private IEnumerator Dash() {
        if (enableSounds && !dashSound.isPlaying) {
            dashSound.Play();
        }
        _isDashing = true;
        _dashSpeed = dashSpeed;
        yield return new WaitForSeconds(dashingDistance);
        _dashSpeed = 0.0f;
        _isDashing = false;
        canDash = false;
    }

    /** Calls all the methods in this script that take care of audio stuff. */
    private void Sounds() {
        Footsteps();
    }
    
    /** Takes care of making footstep sounds when the player is moving. */
    private void Footsteps() {
        // General conditions for making footsteps' sounds.

        if (!IsGrounded()) {
            return;
        }

        if (_footstepDelayer >= 0) {
            _footstepDelayer -= 1;
            return;
        }

        var footstepPlaying = false;
        foreach (var footstep in footstepsList) {
            footstepPlaying = footstep.isPlaying;
            if (footstepPlaying) {
                break;
            }
        }

        var i = Random.Range(0, footstepsList.Length - 1);
        if (!(isMovingX | isMovingY) | footstepPlaying) {
            return;
        }

        // Making the footstep sound.
        footstepsList[i].pitch = Random.Range(footstepSpeedMin, footstepSpeedMax);
        footstepsList[i].volume = Random.Range(_currentFootstepSoundVolumeMin, _currentFootstepSoundVolumeMax);
        footstepsList[i].Play();

        // Reset footstep delay so the next footstep doesn't play until footstepDelay frames have passed.
        _footstepDelayer = _currentFootstepDelay;
    }

    private static void CursorLocking() {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
