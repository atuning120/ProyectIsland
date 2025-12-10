using UnityEngine;

public class OceanSoundManager : MonoBehaviour
{
    [Header("Configuración General")]
    [Tooltip("El clip de audio con el sonido de las olas.")]
    public AudioClip oceanClip;
    [Tooltip("El volumen máximo del sonido.")]
    [Range(0f, 1f)]
    public float maxVolume = 1.0f;
    [Tooltip("El AudioSource que reproducirá el sonido.")]
    public AudioSource audioSource;
    [Tooltip("El transform del jugador (se busca automáticamente si está vacío).")]
    public Transform player;

    public enum DetectionMode
    {
        HeightBased,    // Basado en la altura (Y)
        DistanceBased,  // Basado en la distancia al centro (Radio/Bordes)
        Hybrid          // Combina ambos
    }

    public enum IslandShape
    {
        Circular,
        Rectangular
    }

    [Header("Modo de Detección")]
    public DetectionMode mode = DetectionMode.Hybrid;

    [Header("Configuración de Altura (Height Based)")]
    [Tooltip("La altura del nivel del mar (Y).")]
    public float seaLevel = 0f;
    [Tooltip("A qué distancia vertical del mar el sonido desaparece por completo.")]
    public float heightFadeDistance = 30f;

    [Header("Configuración de Distancia (Distance Based)")]
    [Tooltip("Forma de la isla.")]
    public IslandShape islandShape = IslandShape.Rectangular;
    
    [Tooltip("El centro de la isla.")]
    public Vector3 islandCenter = Vector3.zero;
    
    [Tooltip("El radio de la isla (si es Circular).")]
    public float islandRadius = 100f;

    [Tooltip("El tamaño de la isla en X y Z (si es Rectangular).")]
    public Vector2 islandSize = new Vector2(200f, 200f);

    [Tooltip("Qué tan adentro de la isla se deja de escuchar el mar.")]
    public float inlandFadeDistance = 50f;

    private void Start()
    {
        // Configurar AudioSource si no está asignado
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        // Configurar propiedades del AudioSource
        audioSource.clip = oceanClip;
        audioSource.loop = true;
        audioSource.playOnAwake = true;
        audioSource.spatialBlend = 0f; // 2D Sound (ambiente)
        
        // Iniciar reproducción si hay clip
        if (oceanClip != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }

        // Buscar jugador si no está asignado
        if (player == null)
        {
            FindPlayer();
        }
    }

    private void FindPlayer()
    {
        // Intentar encontrar el XR Origin o Main Camera
        GameObject xrOrigin = GameObject.Find("XR Origin");
        if (xrOrigin != null)
        {
            player = xrOrigin.transform;
            return;
        }
        
        if (Camera.main != null)
        {
            player = Camera.main.transform;
        }
    }

    private void Update()
    {
        if (player == null) return;

        float targetVolume = 0f;

        switch (mode)
        {
            case DetectionMode.HeightBased:
                targetVolume = CalculateHeightVolume();
                break;
            case DetectionMode.DistanceBased:
                targetVolume = CalculateDistanceVolume();
                break;
            case DetectionMode.Hybrid:
                // Usamos el mayor de los dos
                float hVol = CalculateHeightVolume();
                float dVol = CalculateDistanceVolume();
                targetVolume = Mathf.Max(hVol, dVol);
                break;
        }

        // Aplicar volumen suavemente
        audioSource.volume = Mathf.Lerp(audioSource.volume, targetVolume * maxVolume, Time.deltaTime * 2f);
    }

    private float CalculateHeightVolume()
    {
        // Distancia vertical al mar
        float distanceY = Mathf.Abs(player.position.y - seaLevel);
        
        // Si estamos bajo el agua o en el nivel 0, volumen 1.
        // A medida que subimos, el volumen baja.
        float volume = 1f - Mathf.Clamp01(distanceY / heightFadeDistance);
        return volume;
    }

    private float CalculateDistanceVolume()
    {
        if (islandShape == IslandShape.Circular)
        {
            // Distancia horizontal al centro
            float distanceToCenter = Vector3.Distance(new Vector3(player.position.x, 0, player.position.z), new Vector3(islandCenter.x, 0, islandCenter.z));
            
            // Si estamos más allá del radio (en el mar), volumen 1.
            if (distanceToCenter >= islandRadius) return 1f;

            // Si estamos dentro, calculamos qué tan cerca estamos del borde
            float distanceToEdge = islandRadius - distanceToCenter;
            
            // Si estamos muy adentro (distanceToEdge > inlandFadeDistance), volumen 0.
            return 1f - Mathf.Clamp01(distanceToEdge / inlandFadeDistance);
        }
        else // Rectangular
        {
            // Calcular posición relativa al centro
            Vector3 offset = player.position - islandCenter;
            float absX = Mathf.Abs(offset.x);
            float absZ = Mathf.Abs(offset.z);
            float halfSizeX = islandSize.x / 2f;
            float halfSizeZ = islandSize.y / 2f;

            // Si estamos fuera de los límites en X o Z, estamos en el mar (o más allá del borde)
            if (absX >= halfSizeX || absZ >= halfSizeZ)
            {
                return 1f;
            }

            // Estamos dentro de la isla. Calcular distancia al borde más cercano.
            float distToEdgeX = halfSizeX - absX;
            float distToEdgeZ = halfSizeZ - absZ;
            
            // La distancia al mar es la menor distancia a cualquiera de los 4 bordes
            float distToNearestEdge = Mathf.Min(distToEdgeX, distToEdgeZ);

            // Calcular volumen basado en qué tan adentro estamos
            return 1f - Mathf.Clamp01(distToNearestEdge / inlandFadeDistance);
        }
    }

    // Dibujar Gizmos para visualizar el área en el editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Vector3 center = new Vector3(islandCenter.x, seaLevel, islandCenter.z);
        
        if (islandShape == IslandShape.Circular)
        {
            // Dibujar borde de la isla
            Gizmos.DrawWireSphere(center, islandRadius);
            
            // Dibujar zona de fade (donde se deja de escuchar)
            Gizmos.color = new Color(0, 0, 1, 0.3f);
            Gizmos.DrawWireSphere(center, Mathf.Max(0, islandRadius - inlandFadeDistance));
        }
        else
        {
            // Dibujar borde de la isla
            Vector3 size = new Vector3(islandSize.x, 1, islandSize.y);
            Gizmos.DrawWireCube(center, size);

            // Dibujar zona de fade
            Gizmos.color = new Color(0, 0, 1, 0.3f);
            Vector3 fadeSize = new Vector3(Mathf.Max(0, islandSize.x - inlandFadeDistance * 2), 1, Mathf.Max(0, islandSize.y - inlandFadeDistance * 2));
            Gizmos.DrawWireCube(center, fadeSize);
        }
    }
}
