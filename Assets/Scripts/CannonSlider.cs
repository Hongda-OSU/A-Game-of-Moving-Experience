using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CannonSlider : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private Transform cannon;
    [SerializeField] private float xLimit = 45f;
    [SerializeField] private float dir = -1;

    void Start()
    {
        slider.onValueChanged.AddListener(delegate
        {
            RotateCannon();
        });
    }

    private void RotateCannon()
    {
        cannon.localEulerAngles = new Vector3(90, 0, dir * xLimit * slider.value);
    }
}
