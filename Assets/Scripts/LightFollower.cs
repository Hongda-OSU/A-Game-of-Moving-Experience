using UnityEngine;

public class LightFollower : MonoBehaviour
{
    [SerializeField] private GameObject tmpObject;
    private Vector3 offset;

    void Start()
    {
        offset = transform.position - tmpObject.transform.position;
    }

    void LateUpdate()
    {
        transform.position = GamePlayManager.Instance.ActiveController.transform.position + offset;
    }
}
