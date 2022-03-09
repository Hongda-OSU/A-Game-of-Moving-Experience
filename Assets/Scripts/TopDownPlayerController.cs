using System.Collections;
using UnityEngine;

public class TopDownPlayerController : MonoBehaviour
{
    private Rigidbody rb;
    [SerializeField] private float PlayerSpeed;
    [SerializeField] private float JumpHeight;
    [SerializeField] private float ShootHeight;
    private bool isGyro;
    private bool isGrounded;
    private float horizontalMovement;
    private float verticalMovement;
    private Vector3 movementDirection;
    private float tmpSpeed;

    void Awake()
    {
        rb = this.gameObject.GetComponent<Rigidbody>();
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

    void Update()
    {
        // Check falling
        CheckPlayerDeath();
    }

    void FixedUpdate()
    {
        // using Gyroscope if devise supported
        if (isGyro)
        {
            horizontalMovement = Input.gyro.gravity.x;
            verticalMovement = Input.gyro.gravity.y;
        }
        // using Accelerometer as alternative
        else
        {
            horizontalMovement = Input.acceleration.x;
            verticalMovement = Input.acceleration.y;
        }
        // (1) first Move() that handle player movement
        movementDirection = new Vector3(horizontalMovement, 0.0f, verticalMovement);
        rb.AddForce(movementDirection * tmpSpeed, ForceMode.Force);
    }

    public void Jump()
    {
        // Handle Jumping
        if (isGrounded)
        {
            rb.AddForce(new Vector3(0, JumpHeight, 0), ForceMode.Impulse);
            isGrounded = false;
            //if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            //{
            //}
        }
    }

    public void SpeedChange(float value)
    {
        tmpSpeed = PlayerSpeed * (1 + value);
    }

    public void Shoot()
    {
        ShootHandler();
        StartCoroutine(TempSpeedUp());
        rb.AddForce(GamePlayManager.Instance.Cannon.transform.forward + new Vector3(0, ShootHeight, 0), ForceMode.Impulse);
        isGrounded = false;
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

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = true;

        if (collision.gameObject.layer == 6)
        {
            GamePlayManager.Instance.SetPlatformOne();
        }
        else if (collision.gameObject.layer == 7)
        {
            GamePlayManager.Instance.SetPlatformTwo();
        }
        else if (collision.gameObject.layer == 8)
        {
            GamePlayManager.Instance.SetPlatformThree();
        }

        if (collision.gameObject.CompareTag("Surface"))
        {
            this.gameObject.SetActive(false);
            GamePlayManager.Instance.JumpButton.gameObject.SetActive(false);
            GamePlayManager.Instance.SpeedSlider.gameObject.SetActive(false);
            GamePlayManager.Instance.CannonCamera.enabled = true;
            GamePlayManager.Instance.CannonSlider.gameObject.SetActive(true);
            GamePlayManager.Instance.ShootButton.gameObject.SetActive(true);
        }

        if (collision.gameObject.CompareTag("Hitter"))
        {
            GamePlayManager.Instance.CreateTopDownPlayer();
            this.gameObject.SetActive(false);
            collision.gameObject.SetActive(false);
        }

        if (collision.gameObject.CompareTag("Goal") && this.gameObject.name.Contains("Cube"))
        {
            GamePlayManager.Instance.HandleWin();
        }
    }

    private void CheckPlayerDeath()
    {
        if (transform.position.y <= -5f)
            GamePlayManager.Instance.HealthHandler();
    }
}
