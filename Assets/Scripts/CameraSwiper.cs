using UnityEngine;

public class CameraSwiper : MonoBehaviour
{
    private Touch initTouch = new Touch();
    private float rotX;
    private float rotY;
    private Vector3 originRot;
    [SerializeField] private float rotSpeed;
    [SerializeField] private float dir;
    void Start()
    {
        originRot = transform.eulerAngles;
        rotX = originRot.x;
        rotY = originRot.y;
    }

    void FixedUpdate()
    {
        if (GamePlayManager.Instance.buttonPressed) return;
        foreach (Touch touch in Input.touches)
        {
            if (touch.phase == TouchPhase.Began)
            {
                initTouch = touch;
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                //swiping
                float deltaX = initTouch.position.x - touch.position.x;
                float deltaY = initTouch.position.y - touch.position.y;
                rotX -= deltaY * Time.deltaTime * rotSpeed * dir;
                rotY += deltaX * Time.deltaTime * rotSpeed * dir;
                Mathf.Clamp(rotX, -80, 80);
                transform.eulerAngles = new Vector3(rotX, rotY, 0f);
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                initTouch = new Touch();
            }
        }
    }
}
