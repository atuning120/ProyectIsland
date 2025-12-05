using UnityEngine;
using UnityEngine.InputSystem;

public class TutorialControlador : MonoBehaviour
{
    public GameObject[] tutorialSlides;  // ← Tus imágenes del tutorial
    public GameObject menuPrincipal;     // ← Tu menú principal

    private int index = 0;
    private bool lockInput = false;

    void OnEnable()
    {
        ResetTutorial();
    }

    void Update()
    {
        if (lockInput) return;

        bool pressed = false;

        // Teclado
        if (Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame)
            pressed = true;

        // Gamepad / Quest
        var pad = Gamepad.current;
        if (!pressed && pad != null)
        {
            if (pad.buttonSouth.wasPressedThisFrame ||
                pad.rightTrigger.wasPressedThisFrame)
                pressed = true;
        }

        if (pressed)
            NextSlide();
    }

    public void ResetTutorial()
    {
        index = 0;
        ShowSlide(0);
        lockInput = false;
    }

    void NextSlide()
    {
        lockInput = true;

        index++;

        // Si terminó el tutorial
        if (index >= tutorialSlides.Length)
        {
            gameObject.SetActive(false);   // Cierra tutorial
            menuPrincipal.SetActive(true); // Abre menú principal
            return;
        }

        ShowSlide(index);

        // Previene doble entrada accidental
        Invoke(nameof(Unlock), 0.05f);
    }

    void Unlock() => lockInput = false;

    void ShowSlide(int i)
    {
        for (int j = 0; j < tutorialSlides.Length; j++)
            tutorialSlides[j].SetActive(j == i);
    }
}
