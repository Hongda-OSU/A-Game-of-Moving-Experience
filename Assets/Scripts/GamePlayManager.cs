using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GamePlayManager : MonoBehaviour
{
    [SerializeField] private GameObject fpsPlayerController;
    [SerializeField] private GameObject freeLookPlayerController;
    [SerializeField] private GameObject topDownPlayerController;

    [SerializeField] private GameObject fpsCubePlayerController;
    [SerializeField] private GameObject freeLookCubePlayerController;
    [SerializeField] private GameObject topDownCubePlayerController;

    private GameObject activedController;
    private GameObject previousActiveController;

    [SerializeField] private Camera cannonCamera;
    [SerializeField] private Slider cannonSlider;
    [SerializeField] private float playerHealth;
    private float tempHealth;

    [SerializeField] private GameObject surface;
    [SerializeField] private GameObject cannon;
    [SerializeField] private Transform cube;
    [SerializeField] private Button jumpButton;
    [SerializeField] private Button speedUpButton;
    [SerializeField] private Button shootButton;
    [SerializeField] private Slider speedSlider;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button quitButton;

    [SerializeField] private Camera gameOverCamera;
    [SerializeField] private Camera topDownCamera1;
    [SerializeField] private Camera topDownCamera2;
    [SerializeField] private Camera topDownCamera3;
    [SerializeField] private Slider topDownCameraSlider;
    private Vector3 cam1Pos;
    private Vector3 cam2Pos;
    private Vector3 cam3Pos;
    private Camera currentCamera;

    [SerializeField] private GameObject heathText;
    [SerializeField] private GameObject timeText;
    [SerializeField] private GameObject platformInfo;
    [SerializeField] private GameObject deadInfo;
    [SerializeField] private GameObject winText;
    [SerializeField] private GameObject lossText;

    [SerializeField] private Button fpsCamSwitch;
    [SerializeField] private Button freeLookCamSwitch;
    [SerializeField] private Button topDownCamSwitch;

    [SerializeField] private Material brighterColor;
    [SerializeField] private GameObject platform1;
    [SerializeField] private GameObject platform2;
    [SerializeField] private GameObject platform3;

    [SerializeField] private GameObject light;

    private TMPro.TextMeshProUGUI HealthText, TimeText, InfoText;

    private enum ControllerType { FPS, FREELOOK, TOPDOWN, FPSCUEB, FREELOOKCUBE, TOPDOWNCUBE }
    private enum PlatformType {ONE, TWO, THREE}

    // help to determine current controller
    private ControllerType currenControllerType;

    // help to determine current platform
    private PlatformType currentPlatform = PlatformType.ONE;
    
    // timer that record how long player reach the goal
    private float timer;

    // position where player restart from the begining
    private Vector3 startPlatformPos = new Vector3(0, 0.5f, -20f);
    private Vector3 secondPlatformPos = new Vector3(9f, 5.5f, 110f);
    private Vector3 thirdPlatformPos;

    // hold the current active controller position
    private Vector3 controllerPosition;
    private Vector3 previousControllerPosition;

    // check if any bottom is pressed
    public bool buttonPressed;

    private static GamePlayManager instance;
    public static GamePlayManager Instance { get { return instance; } }

    // singleton
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        instance = this;
    }

    // add button methods
    private void Start()
    {
        tempHealth = playerHealth;
        thirdPlatformPos = cube.position;
        HealthText = heathText.GetComponent<TMPro.TextMeshProUGUI>();
        TimeText = timeText.GetComponent<TMPro.TextMeshProUGUI>();
        InfoText = platformInfo.GetComponent<TMPro.TextMeshProUGUI>();
        HealthText.SetText("HEALTH: " + tempHealth.ToString());

        cam1Pos = topDownCamera1.transform.position;
        cam2Pos = topDownCamera2.transform.position;
        cam3Pos = topDownCamera3.transform.position;

        jumpButton.onClick.AddListener(() =>
        {
            PlayerJump();
        });
        speedSlider.onValueChanged.AddListener(delegate
        {
            PlayerSpeedChange();
        });
        shootButton.onClick.AddListener(() =>
        {
            PlayerShoot();
        });
        restartButton.onClick.AddListener(() =>
        {
            RestartGame();
        });
        fpsCamSwitch.onClick.AddListener(() =>
        {
            SwitchToFPSPlayer();
        });
        freeLookCamSwitch.onClick.AddListener(() =>
        {
            SwitchToFreeLookPlayer();
        });
        topDownCamSwitch.onClick.AddListener(() =>
        {
            SwitchToTopDownPlayer();
        });
    }

    private void Update()
    {
        FindCurrentActiveController();
        SetPlaformInfo();
        timer += Time.deltaTime;
        TimeText.SetText("TIME: " + timer.ToString());
        if (activedController == topDownPlayerController)
            UpdateCamera();
    }

    // Player switching methods
    private void SwitchToFPSPlayer()
    {
        if (activedController == fpsPlayerController) return;
        if (activedController == topDownPlayerController) topDownCameraSlider.gameObject.SetActive(false);
        if (activedController == freeLookCubePlayerController || activedController == topDownCubePlayerController)
        {
            SwitchToCubeFPSPlayer();
            return;
        }
        
        previousActiveController = activedController;
        previousControllerPosition = controllerPosition;
        activedController = fpsPlayerController;
        previousActiveController.SetActive(false);
        activedController.SetActive(true);
        activedController.transform.position = previousControllerPosition;
    }

    private void SwitchToCubeFPSPlayer()
    {
        topDownCamera3.enabled = false;
        topDownCameraSlider.gameObject.SetActive(false);
        previousActiveController = activedController;
        previousControllerPosition = controllerPosition;
        activedController = fpsCubePlayerController;
        previousActiveController.SetActive(false);
        activedController.SetActive(true);
        activedController.transform.position = previousControllerPosition;
    }

    private void SwitchToFreeLookPlayer()
    {
        if (activedController == freeLookPlayerController) return;
        if (activedController == topDownPlayerController) topDownCameraSlider.gameObject.SetActive(false);
        if (activedController == fpsCubePlayerController || activedController == topDownCubePlayerController)
        {
            SwitchToCubeFreeLookPlayer();
            return;
        }

        previousActiveController = activedController;
        previousControllerPosition = controllerPosition;
        activedController = freeLookPlayerController;
        previousActiveController.SetActive(false);
        activedController.SetActive(true);
        activedController.transform.position = previousControllerPosition;
    }

    private void SwitchToCubeFreeLookPlayer()
    {
        topDownCamera3.enabled = false;
        topDownCameraSlider.gameObject.SetActive(false);
        previousActiveController = activedController;
        previousControllerPosition = controllerPosition;
        activedController = freeLookCubePlayerController;
        previousActiveController.SetActive(false);
        activedController.SetActive(true);
        activedController.transform.position = previousControllerPosition;
    }

    private void SwitchToTopDownPlayer()
    {
        if (activedController == topDownPlayerController) return;
        if (activedController == fpsCubePlayerController || activedController == freeLookCubePlayerController)
        {
            SwitchToCubeTopDownPlayer();
            return;
        }

        topDownCameraSlider.gameObject.SetActive(true);
        previousActiveController = activedController;
        previousControllerPosition = controllerPosition;
        activedController = topDownPlayerController;
        previousActiveController.SetActive(false);
        activedController.SetActive(true);
        activedController.transform.position = previousControllerPosition;
    }

    private void SwitchToCubeTopDownPlayer()
    {
        topDownCamera3.enabled = true;
        topDownCameraSlider.gameObject.SetActive(true);
        previousActiveController = activedController;
        previousControllerPosition = controllerPosition;
        activedController = topDownCubePlayerController;
        previousActiveController.SetActive(false);
        activedController.SetActive(true);
        activedController.transform.position = previousControllerPosition;
    }

    // Camera update method for top-down player
    private void UpdateCamera()
    {
        switch (currentPlatform)
        {
            case PlatformType.ONE:
                topDownCamera1.enabled = true;
                topDownCamera2.enabled = false;
                topDownCamera3.enabled = false;
                currentCamera = topDownCamera1;
                break;
            case PlatformType.TWO:
                topDownCamera1.enabled = false;
                topDownCamera2.enabled = true;
                topDownCamera3.enabled = false;
                currentCamera = topDownCamera2;
                break;
            case PlatformType.THREE:
                topDownCamera1.enabled = false;
                topDownCamera2.enabled = false;
                topDownCamera3.enabled = true;
                currentCamera = topDownCamera3;
                break;
        }
    }

    private void SetPlaformInfo()
    {
        InfoText.SetText("PLATFORM: " + currentPlatform.ToString());
    }

    // handle player jump
    private void PlayerJump()
    {
        switch (currenControllerType)
        {
            case ControllerType.FPS:
                fpsPlayerController.GetComponent<FPSPlayerController>().Jump();
                break;
            case ControllerType.FREELOOK:
                freeLookPlayerController.GetComponent<FreeLookPlayerController>().Jump();
                break;
            case ControllerType.TOPDOWN:
                topDownPlayerController.GetComponent<TopDownPlayerController>().Jump();
                break;
            case ControllerType.FPSCUEB:
                fpsCubePlayerController.GetComponent<FPSPlayerController>().Jump();
                break;
            case ControllerType.FREELOOKCUBE:
                freeLookCubePlayerController.GetComponent<FreeLookPlayerController>().Jump();
                break;
            case ControllerType.TOPDOWNCUBE:
                topDownCubePlayerController.GetComponent<TopDownPlayerController>().Jump();
                break;
        }
    }

    // handle player speed change
    private void PlayerSpeedChange()
    {
        switch (currenControllerType)
        {
            case ControllerType.FPS:
                fpsPlayerController.GetComponent<FPSPlayerController>().SpeedChange(speedSlider.value);
                break;
            case ControllerType.FREELOOK:
                freeLookPlayerController.GetComponent<FreeLookPlayerController>().SpeedChange(speedSlider.value);
                break;
            case ControllerType.TOPDOWN:
                topDownPlayerController.GetComponent<TopDownPlayerController>().SpeedChange(speedSlider.value);
                break;
            case ControllerType.FPSCUEB:
                fpsCubePlayerController.GetComponent<FPSPlayerController>().SpeedChange(speedSlider.value);
                break;
            case ControllerType.FREELOOKCUBE:
                freeLookCubePlayerController.GetComponent<FreeLookPlayerController>().SpeedChange(speedSlider.value);
                break;
            case ControllerType.TOPDOWNCUBE:
                topDownCubePlayerController.GetComponent<TopDownPlayerController>().SpeedChange(speedSlider.value);
                break;
        }
    }

    // handle shoot player
    public void PlayerShoot()
    {
        switch (currenControllerType)
        {
            case ControllerType.FPS:
                fpsPlayerController.GetComponent<FPSPlayerController>().Shoot();
                break;
            case ControllerType.FREELOOK:
                freeLookPlayerController.GetComponent<FreeLookPlayerController>().Shoot();
                break;
            case ControllerType.TOPDOWN:
                topDownPlayerController.GetComponent<TopDownPlayerController>().Shoot();
                break;
        }
    }

    // handle player health change
    public void HealthHandler()
    {
        tempHealth -= 1;
        if (tempHealth <= 0)
        {
            HandleDeath();
        }
        else
        {
            StartCoroutine(VisualDeath());
            HealthText.SetText("HEALTH: " + tempHealth.ToString());
        }
    }

    // provide visual to player fall
    IEnumerator VisualDeath()
    {
        deadInfo.SetActive(true);
        activedController.SetActive(false);
        gameOverCamera.enabled = true;
        yield return new WaitForSeconds(2f);
        activedController.SetActive(true);
        gameOverCamera.enabled = false;
        switch (currentPlatform)
        {
            case PlatformType.ONE:
                activedController.transform.position = startPlatformPos;
                break;
            case PlatformType.TWO:
                activedController.transform.position = secondPlatformPos;
                break;
            case PlatformType.THREE:
                activedController.transform.position = thirdPlatformPos;
                break;
        }
        deadInfo.SetActive(false);
        cannon.SetActive(true);
        surface.SetActive(true);
    }

    // handle player death
    private void HandleDeath()
    {
        activedController.gameObject.SetActive(false);
        JumpButton.gameObject.SetActive(false);
        SpeedSlider.gameObject.SetActive(false);
        timeText.SetActive(false);
        platformInfo.SetActive(false);

        lossText.SetActive(true);
        gameOverCamera.enabled = true;
        restartButton.gameObject.SetActive(true);
        quitButton.gameObject.SetActive(true);
    }

    // handle player win
    public void HandleWin()
    {
        activedController.gameObject.SetActive(false);
        JumpButton.gameObject.SetActive(false);
        SpeedSlider.gameObject.SetActive(false);
        timeText.SetActive(false);
        platformInfo.SetActive(false);

        winText.SetActive(true);
        gameOverCamera.enabled = true;
        restartButton.gameObject.SetActive(true);
        quitButton.gameObject.SetActive(true);
    }

    private void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    //public void PlayerSpeedUp()
    //{
    //    switch (currenControllerType)
    //    {
    //        case ControllerType.FPS:
    //            fpsPlayerController.GetComponent<FPSPlayerController>().SpeedUp();
    //            break;
    //        case ControllerType.FREELOOK:
    //            freeLookPlayerController.GetComponent<FreeLookPlayerController>().SpeedUp();
    //            break;
    //        case ControllerType.TOPDOWN:
    //            topDownPlayerController.GetComponent<TopDownPlayerController>().SpeedUp();
    //            break;
    //    }
    //}

    //public void PlayerSpeedDown()
    //{
    //    switch (currenControllerType)
    //    {
    //        case ControllerType.FPS:
    //            fpsPlayerController.GetComponent<FPSPlayerController>().SpeedDown();
    //            break;
    //        case ControllerType.FREELOOK:
    //            freeLookPlayerController.GetComponent<FreeLookPlayerController>().SpeedDown();
    //            break;
    //        case ControllerType.TOPDOWN:
    //            topDownPlayerController.GetComponent<TopDownPlayerController>().SpeedDown();
    //            break;
    //    }
    //}

    // help to find current active player
    private void FindCurrentActiveController()
    {
        if (fpsPlayerController.activeSelf)
        {
            activedController = fpsPlayerController;
            currenControllerType = ControllerType.FPS;
        }
        else if (freeLookPlayerController.activeSelf)
        {
            activedController = freeLookPlayerController;
            currenControllerType = ControllerType.FREELOOK;
        }
        else if (topDownPlayerController.activeSelf)
        {
            activedController = topDownPlayerController;
            currenControllerType = ControllerType.TOPDOWN;
        }
        else if (fpsCubePlayerController.activeSelf)
        {
            activedController = fpsCubePlayerController;
            currenControllerType = ControllerType.FPSCUEB;
        }
        else if (freeLookCubePlayerController.activeSelf)
        {
            activedController = freeLookCubePlayerController;
            currenControllerType = ControllerType.FREELOOKCUBE;
        }
        else if (topDownCubePlayerController.activeSelf)
        {
            activedController = topDownCubePlayerController;
            currenControllerType = ControllerType.TOPDOWNCUBE;
        }

        controllerPosition = activedController.transform.position;
    }

    // player transformation in platform 3
    public void CreateFreeLookPlayer(Vector3 position, Quaternion rotation)
    {
        freeLookCubePlayerController.SetActive(true);
        //Instantiate(freeLookCubePlayerController, position, rotation);
        //Deactive();
    }

    public void CreateFPSPlayer(Vector3 position, Quaternion rotation)
    {
        fpsCubePlayerController.SetActive(true);
        //Instantiate(fpsCubePlayerController, position, rotation);
        //Deactive();
    }

    public void CreateTopDownPlayer()
    {
        topDownCubePlayerController.SetActive(true);
        //Instantiate(topDownCubePlayerController, cube.position, Quaternion.identity);
        //Deactive();
    }

    private void Deactive()
    {
        JumpButton.gameObject.SetActive(false);
        SpeedSlider.gameObject.SetActive(false);
        light.SetActive(false);
        fpsCamSwitch.gameObject.SetActive(false);
        freeLookCamSwitch.gameObject.SetActive(false);
        topDownCamSwitch.gameObject.SetActive(false);
    }

    // platform setter
    public void SetPlatformOne()
    {
        currentPlatform = PlatformType.ONE;
        platform1.GetComponent<Renderer>().material = brighterColor;
    }

    public void SetPlatformTwo()
    {
        currentPlatform = PlatformType.TWO;
        platform2.GetComponent<Renderer>().material = brighterColor;
    }

    public void SetPlatformThree()
    {
        currentPlatform = PlatformType.THREE;
        platform3.GetComponent<Renderer>().material = brighterColor;
    }

    public Vector3 GetCameraTrans()
    {
        Vector3 pos = Vector3.zero;
        if (currentCamera == topDownCamera1)
        {
            pos =  cam1Pos;
        }
        else if (currentCamera == topDownCamera2)
        {
            pos = cam2Pos;
        }
        else if (currentCamera == topDownCamera2)
        {
            pos = cam3Pos;
        }
        return pos;
    }

    // Getter methods
    public GameObject Surface => surface;
    public GameObject Cannon => cannon;
    //public GameObject FreeLookCubePlayer => freeLookCubePlayerController;
    //public GameObject FPSCubePlayer => fpsCubePlayerController;
    public Camera CannonCamera => cannonCamera;
    public Slider CannonSlider => cannonSlider;
    public Button JumpButton => jumpButton;
    //public Button SpeedUpButton => speedUpButton;
    public Button ShootButton => shootButton;
    public Slider SpeedSlider => speedSlider;
    public GameObject ActiveController => activedController;
    public Camera CurrentCamera => currentCamera;
}
