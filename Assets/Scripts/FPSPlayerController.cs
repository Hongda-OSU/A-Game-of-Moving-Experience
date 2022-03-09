using UnityEngine;
using System.Collections;

public class FPSPlayerController : MonoBehaviour
{
    private float initialYAngle;
    private float appliedGyroYAngle;
    private float calibrationYAngle;
    private Transform rawGyroRotation;
    private float tempSmoothing;
    [SerializeField] private float smoothing = 0.1f;

    private CharacterController characterController;
    [SerializeField] private float PlayerSpeed;
    [SerializeField] private float JumpHeight;
    [SerializeField] private float ShootHeight;
    [SerializeField] private float Gravity;
    [SerializeField] private float ForceMagnitude;
    private bool isGyro;
    private float horizontalMovement;
    private float verticalMovement;
    private Vector3 movementDirection;
    private Vector3 playerVelocity;
    private float tmpSpeed;

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        if (SystemInfo.supportsGyroscope)
        {
            Input.gyro.enabled = true;
            isGyro = true;
            Debug.Log("Using Gyroscope");
        }
        // Preventing mobile devices going in to sleep mode 
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Screen.autorotateToLandscapeRight = true;
        tmpSpeed = PlayerSpeed;
    }

    private IEnumerator Start()
    {
        Application.targetFrameRate = 60;
        initialYAngle = transform.eulerAngles.y;

        rawGyroRotation = new GameObject("GyroRaw").transform;
        rawGyroRotation.position = transform.position;
        rawGyroRotation.rotation = transform.rotation;

        // Wait until gyro is active, then calibrate to reset starting rotation.
        yield return new WaitForSeconds(1);

        StartCoroutine(CalibrateYAngle());
    }

    private void Update()
    {
        // using Gyroscope if devise supported
        if (isGyro)
        {
            horizontalMovement = Input.gyro.gravity.x;
            verticalMovement = Input.gyro.gravity.z;
        }
        // using Accelerometer as alternative
        else
        {
            horizontalMovement = Input.acceleration.x;
            verticalMovement = Input.acceleration.z;
        }

        Vector3 move = new Vector3(horizontalMovement * tmpSpeed * Time.deltaTime, 0,
            -verticalMovement * tmpSpeed * Time.deltaTime);
        movementDirection = transform.TransformDirection(move);
        characterController.Move(movementDirection);
        // (2) second Move() that apply gravity to player
        playerVelocity.y -= Gravity * Time.deltaTime;
        // player grounded => stick with ground
        if (characterController.isGrounded && playerVelocity.y < 0f)
            playerVelocity.y = -2f;
        characterController.Move(playerVelocity * Time.deltaTime);

        ApplyGyroRotation();
        ApplyCalibration();

        transform.rotation = Quaternion.Slerp(transform.rotation, rawGyroRotation.rotation, smoothing);
        CheckPlayerDeath();
    }

    public void Jump()
    {
        if (characterController.isGrounded)
        {
            playerVelocity.y = Mathf.Sqrt(JumpHeight * Gravity * 2f);
        }
    }

    public void SpeedChange(float value)
    {
        tmpSpeed = PlayerSpeed * (1 + value);
    }

    public void SpeedUp()
    {
        tmpSpeed = PlayerSpeed * 3f;
    }

    public void SpeedDown()
    {
        tmpSpeed = PlayerSpeed;
    }

    public void Shoot()
    {
        ShootHandler();
        StartCoroutine(TempSpeedUp());
        playerVelocity.y = Mathf.Sqrt(ShootHeight * Gravity * 4f);
    }

    IEnumerator TempSpeedUp()
    {
        tmpSpeed = PlayerSpeed * 10f;
        yield return new WaitForSeconds(4f);
        tmpSpeed = PlayerSpeed;
    }

    private void ShootHandler()
    {
        GamePlayManager.Instance.CannonCamera.enabled = false;
        GamePlayManager.Instance.CannonSlider.gameObject.SetActive(false);
        GamePlayManager.Instance.ShootButton.gameObject.SetActive(false);
        GamePlayManager.Instance.Surface.SetActive(false);
        GamePlayManager.Instance.Cannon.SetActive(false);

        this.gameObject.SetActive(true);
        GamePlayManager.Instance.JumpButton.gameObject.SetActive(true);
        GamePlayManager.Instance.SpeedSlider.gameObject.SetActive(true);
        GamePlayManager.Instance.SpeedSlider.value = 0;
    }


    private IEnumerator CalibrateYAngle()
    {
        tempSmoothing = smoothing;
        smoothing = 1;
        // Offsets the y angle in case it wasn't 0 at edit time.
        calibrationYAngle = appliedGyroYAngle - initialYAngle; 
        yield return null;
        smoothing = tempSmoothing;
    }

    private void ApplyGyroRotation()
    {
        rawGyroRotation.rotation = Input.gyro.attitude;
        // Swap "handedness" of quaternion from gyro.
        rawGyroRotation.Rotate(0f, 0f, 180f, Space.Self); 
        // Rotate to make sense as a camera pointing out the back of your device.
        rawGyroRotation.Rotate(90f, 180f, 0f, Space.World); 
        // Save the angle around y axis for use in calibration.
        appliedGyroYAngle = rawGyroRotation.eulerAngles.y; 
    }

    private void ApplyCalibration()
    {
        // Rotates y angle back however much it deviated when calibrationYAngle was saved.
        rawGyroRotation.Rotate(0f, -calibrationYAngle, 0f, Space.World); 
    }

    public void SetEnabled(bool value)
    {
        enabled = true;
        StartCoroutine(CalibrateYAngle());
    }

    private void CheckPlayerDeath()
    {
        if (transform.position.y <= -5f)
            GamePlayManager.Instance.HealthHandler();
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody rb = hit.collider.attachedRigidbody;
        if (rb != null)
        {
            if (hit.collider.CompareTag("Weighter") && SeeSawHandler.Instance.IsHit == false)
            {
                Vector3 forceDir = hit.transform.position - transform.position;
                forceDir.y = 0;
                forceDir.Normalize();
                rb.AddForceAtPosition(forceDir * ForceMagnitude * 0.8f, transform.position, ForceMode.Force);
            }
            else if (hit.collider.CompareTag("SeeSaw") && SeeSawHandler.Instance.IsHit == false)
            {
                Vector3 forceDir = Vector3.down;
                rb.AddForceAtPosition(forceDir * ForceMagnitude * 5f, transform.position, ForceMode.Force);
            }
        }

        if (hit.collider.gameObject.layer == 6)
        {
            GamePlayManager.Instance.SetPlatformOne();
        }
        else if (hit.collider.gameObject.layer == 7)
        {
            GamePlayManager.Instance.SetPlatformTwo();
        }
        else if (hit.collider.gameObject.layer == 8)
        {
            GamePlayManager.Instance.SetPlatformThree();
        }

        if (hit.collider.CompareTag("Surface"))
        {
            this.gameObject.SetActive(false);
            GamePlayManager.Instance.JumpButton.gameObject.SetActive(false);
            GamePlayManager.Instance.SpeedSlider.gameObject.SetActive(false);
            GamePlayManager.Instance.CannonCamera.enabled = true;
            GamePlayManager.Instance.CannonSlider.gameObject.SetActive(true);
            GamePlayManager.Instance.ShootButton.gameObject.SetActive(true);
        }

        if (hit.collider.CompareTag("Hitter"))
        {
            GamePlayManager.Instance.CreateFPSPlayer(transform.position, transform.rotation);
            this.gameObject.SetActive(false);
            hit.gameObject.SetActive(false);
        }

        if (hit.collider.CompareTag("Goal") && this.gameObject.name.Contains("Cube"))
        {
            GamePlayManager.Instance.HandleWin();
        }
    }
}