using UnityEngine;

/// <summary>
/// Crea un efecto de océano infinito usando un cilindro que sigue al jugador
/// Perfecto para dar la ilusión de horizonte con agua
/// </summary>
public class SkyboxOcean : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Transform del jugador para seguirlo")]
    public Transform player;
    
    [Tooltip("Altura del agua")]
    public float waterLevel = 0f;
    
    [Tooltip("Radio del cilindro de agua")]
    public float radius = 200f;
    
    [Tooltip("Altura del cilindro")]
    public float height = 100f;
    
    [Header("Apariencia")]
    [Tooltip("Color del agua en el horizonte")]
    public Color oceanColor = new Color(0.2f, 0.4f, 0.6f, 1f);
    
    [Tooltip("Color del agua cerca (gradiente)")]
    public Color nearWaterColor = new Color(0.1f, 0.5f, 0.7f, 0.9f);
    
    [Header("Animación")]
    [Tooltip("Velocidad de rotación del agua")]
    public float rotationSpeed = 1f;

    private GameObject oceanCylinder;
    private Material oceanMaterial;

    private void Start()
    {
        FindPlayer();
        CreateOceanCylinder();
    }

    private void FindPlayer()
    {
        if (player == null)
        {
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
        }
    }

    private void CreateOceanCylinder()
    {
        // Crear cilindro
        oceanCylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        oceanCylinder.name = "SkyboxOcean";
        oceanCylinder.transform.parent = transform;
        oceanCylinder.transform.localPosition = new Vector3(0, waterLevel, 0);
        oceanCylinder.transform.localScale = new Vector3(radius * 2f, height / 2f, radius * 2f);
        
        // Remover collider
        Destroy(oceanCylinder.GetComponent<Collider>());
        
        // Crear material
        oceanMaterial = new Material(Shader.Find("Standard"));
        oceanMaterial.color = oceanColor;
        oceanMaterial.SetFloat("_Glossiness", 0.7f);
        oceanMaterial.SetFloat("_Metallic", 0.3f);
        
        // Renderizar desde dentro (invertir normales conceptualmente)
        oceanCylinder.GetComponent<Renderer>().material = oceanMaterial;
        
        // Hacer que se renderice detrás de todo
        oceanMaterial.renderQueue = 2000;
    }

    private void LateUpdate()
    {
        if (player != null && oceanCylinder != null)
        {
            // Seguir al jugador solo en X y Z
            Vector3 targetPos = new Vector3(player.position.x, waterLevel, player.position.z);
            oceanCylinder.transform.position = targetPos;
            
            // Rotar lentamente para efecto de movimiento
            oceanCylinder.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
    }
}
