using UnityEngine;

/// <summary>
/// Crea un límite invisible que impide al jugador salir del área del mapa
/// </summary>
public class MapBoundary : MonoBehaviour
{
    [Header("Configuración de Límites")]
    [Tooltip("Centro del área jugable")]
    public Vector3 centerPoint = Vector3.zero;
    
    [Tooltip("Tamaño del área jugable (ancho en X, alto en Y, largo en Z)")]
    public Vector3 boundarySize = new Vector3(100f, 50f, 100f);
    
    [Header("Configuración del Jugador")]
    [Tooltip("Transform del jugador (XR Rig o cámara principal)")]
    public Transform player;
    
    [Tooltip("Suavidad al empujar al jugador hacia adentro")]
    [Range(0.1f, 1f)]
    public float pushBackSmooth = 0.5f;
    
    [Header("Efectos Visuales")]
    [Tooltip("Mostrar el límite en el editor")]
    public bool showGizmos = true;
    
    [Tooltip("Color del límite en el editor")]
    public Color gizmoColor = new Color(1f, 0f, 0f, 0.3f);

    private void Start()
    {
        // Si no se asignó el jugador, buscar automáticamente
        if (player == null)
        {
            // Buscar XR Rig o Main Camera
            GameObject xrRig = GameObject.Find("XR Origin") ?? GameObject.Find("XR Rig");
            if (xrRig != null)
            {
                player = xrRig.transform;
            }
            else
            {
                Camera mainCam = Camera.main;
                if (mainCam != null)
                {
                    player = mainCam.transform;
                }
            }
            
            if (player == null)
            {
                Debug.LogWarning("MapBoundary: No se encontró el jugador. Asigna manualmente el Transform del jugador.");
            }
        }
    }

    private void LateUpdate()
    {
        if (player == null) return;

        Vector3 playerPos = player.position;
        Vector3 clampedPos = playerPos;
        bool needsCorrection = false;

        // Calcular los límites
        float minX = centerPoint.x - boundarySize.x / 2f;
        float maxX = centerPoint.x + boundarySize.x / 2f;
        float minY = centerPoint.y - boundarySize.y / 2f;
        float maxY = centerPoint.y + boundarySize.y / 2f;
        float minZ = centerPoint.z - boundarySize.z / 2f;
        float maxZ = centerPoint.z + boundarySize.z / 2f;

        // Verificar límites en X
        if (playerPos.x < minX)
        {
            clampedPos.x = minX;
            needsCorrection = true;
        }
        else if (playerPos.x > maxX)
        {
            clampedPos.x = maxX;
            needsCorrection = true;
        }

        // Verificar límites en Y (altura)
        if (playerPos.y < minY)
        {
            clampedPos.y = minY;
            needsCorrection = true;
        }
        else if (playerPos.y > maxY)
        {
            clampedPos.y = maxY;
            needsCorrection = true;
        }

        // Verificar límites en Z
        if (playerPos.z < minZ)
        {
            clampedPos.z = minZ;
            needsCorrection = true;
        }
        else if (playerPos.z > maxZ)
        {
            clampedPos.z = maxZ;
            needsCorrection = true;
        }

        // Si el jugador está fuera de los límites, empujarlo suavemente de vuelta
        if (needsCorrection)
        {
            player.position = Vector3.Lerp(playerPos, clampedPos, pushBackSmooth);
        }
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        Gizmos.color = gizmoColor;
        Gizmos.DrawWireCube(centerPoint, boundarySize);
        
        // Dibujar las caras del cubo con transparencia
        Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.1f);
        Gizmos.DrawCube(centerPoint, boundarySize);
    }
}
