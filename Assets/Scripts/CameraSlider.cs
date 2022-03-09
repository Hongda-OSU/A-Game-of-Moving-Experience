using UnityEngine;
using UnityEngine.UI;

public class CameraSlider : MonoBehaviour
{
    [SerializeField] private Slider slider;

    void Start()
    {
        slider.onValueChanged.AddListener(delegate
        {
            CameraValueChange();
        });
    }

    private void CameraValueChange()
    {
        GamePlayManager.Instance.CurrentCamera.transform.position = new Vector3(
            GamePlayManager.Instance.GetCameraTrans().x,
            GamePlayManager.Instance.GetCameraTrans().y + slider.value * 50,
            GamePlayManager.Instance.GetCameraTrans().z);
    }
}
