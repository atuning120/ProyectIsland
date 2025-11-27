using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class VRMenuButtonNavigator : MonoBehaviour
{
    public Transform buttonContainer;    
    private Button[] buttons;
    private int currentIndex = 0;

    void Start()
    {
        buttons = buttonContainer.GetComponentsInChildren<Button>(true);

        if (buttons.Length == 0)
        {
            Debug.LogError("No se encontraron botones dentro del contenedor");
            return;
        }

        EventSystem.current.firstSelectedGameObject = buttons[0].gameObject;
        ForceSelect(buttons[0].gameObject);
    }

    void Update()
    {
        // ------- DEVICE SIMULATOR (teclado) ---------
        if (Keyboard.current != null)
        {
            if (Keyboard.current.sKey.wasPressedThisFrame) Move(1);  // bajar
            if (Keyboard.current.wKey.wasPressedThisFrame) Move(-1); // subir

            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                ExecuteEvents.Execute(
                    buttons[currentIndex].gameObject,
                    new BaseEventData(EventSystem.current),
                    ExecuteEvents.submitHandler
                );
            }
        }

        // ------- META QUEST REAL (mando) ---------
        var pad = Gamepad.current;
        if (pad != null)
        {
            if (pad.buttonSouth.wasPressedThisFrame) Move(1);  // A
            if (pad.buttonNorth.wasPressedThisFrame) Move(-1); // Y

            if (pad.rightTrigger.wasPressedThisFrame)
            {
                ExecuteEvents.Execute(
                    buttons[currentIndex].gameObject,
                    new BaseEventData(EventSystem.current),
                    ExecuteEvents.submitHandler
                );
            }
        }

        // Reiniciar selección si se pierde
        if (EventSystem.current.currentSelectedGameObject == null)
            ForceSelect(buttons[currentIndex].gameObject);
    }

    void Move(int direction)
    {
        currentIndex += direction;

        if (currentIndex < 0) currentIndex = buttons.Length - 1;
        if (currentIndex >= buttons.Length) currentIndex = 0;

        ForceSelect(buttons[currentIndex].gameObject);
    }

    void ForceSelect(GameObject obj)
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(obj);
    }
}
