using UnityEngine;
using UnityEngine.UI;

public class ControlVolumenSlider : MonoBehaviour
{
    public Slider sliderVolumen; // arrastra tu slider aquí en el Inspector

    void Start()
    {
        // Inicializa el slider con el valor actual del volumen
        if (sliderVolumen != null)
        {
            sliderVolumen.value = AudioListener.volume;
            sliderVolumen.onValueChanged.AddListener(AjustarVolumen);
        }
    }

    // Función que se llama cada vez que el slider cambia
    public void AjustarVolumen(float valor)
    {
        AudioListener.volume = valor;
        Debug.Log("Volumen ajustado a: " + valor);
    }
}
