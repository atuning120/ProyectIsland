using UnityEngine;

public class BackgroundMusicManager : MonoBehaviour
{
    [Header("Configuración de Música")]
    [Tooltip("El clip de música de fondo.")]
    public AudioClip musicClip;
    
    [Tooltip("Volumen de la música (0 a 1).")]
    [Range(0f, 1f)]
    public float volume = 0.5f;

    private AudioSource audioSource;

    private void Start()
    {
        // Configurar o añadir AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Configuración CRÍTICA para que la música "siga" al jugador:
        // Spatial Blend en 0 hace que el sonido sea 2D.
        // Esto significa que no tiene posición en el mundo, se escucha "en tu cabeza"
        // o "en el ambiente", sin importar dónde estés.
        audioSource.spatialBlend = 0f; 
        
        audioSource.clip = musicClip;
        audioSource.volume = volume;
        audioSource.loop = true; // Repetir infinitamente
        audioSource.playOnAwake = true;

        // Iniciar reproducción
        if (musicClip != null)
        {
            audioSource.Play();
        }
    }

    // Método público para cambiar el volumen dinámicamente (útil para menús de opciones)
    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        if (audioSource != null)
        {
            audioSource.volume = volume;
        }
    }
}
