using UnityEngine;
using UnityEngine.UI;

public class ControlMusica : MonoBehaviour
{
    public AudioSource musica; // arrastra tu AudioSource de música
    public Slider sliderVolumen; // arrastra tu Slider

    void Start()
    {
        if (musica != null && sliderVolumen != null)
        {
            // Inicializa el slider con el valor actual del AudioSource
            sliderVolumen.value = musica.volume;

            // Conecta el slider a la función
            sliderVolumen.onValueChanged.AddListener(AjustarVolumenMusica);
        }
    }

    // Función que se llama al mover el slider
    public void AjustarVolumenMusica(float valor)
    {
        if (musica != null)
        {
            musica.volume = valor;
            Debug.Log("Volumen de música ajustado a: " + valor);
        }
    }
}
