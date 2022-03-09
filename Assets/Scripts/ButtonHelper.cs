using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHelper : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    // check if the button is pressed, used for any type of button
    public void OnPointerDown(PointerEventData eventData)
    {
        GamePlayManager.Instance.buttonPressed = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        GamePlayManager.Instance.buttonPressed = false;
    }
}
