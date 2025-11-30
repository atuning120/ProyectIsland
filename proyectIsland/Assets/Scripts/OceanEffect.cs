using UnityEngine;

/// <summary>
/// Crea un efecto de océano infinito alrededor del mapa usando un plano simple
/// </summary>
public class OceanEffect : MonoBehaviour
{
    [Header("Configuración del Océano")]
    [Tooltip("Altura del agua")]
    public float waterLevel = 0f;
    
    [Tooltip("Tamaño del plano de agua")]
    public float oceanSize = 500f;
    
    [Tooltip("Color del agua")]
    public Color waterColor = new Color(0.1f, 0.3f, 0.5f, 0.8f);
    
    [Header("Animación")]
    [Tooltip("Activar animación de ondas")]
    public bool animateWaves = true;
    
    [Tooltip("Velocidad de las ondas")]
    public float waveSpeed = 0.5f;
    
    [Tooltip("Altura de las ondas")]
    public float waveHeight = 0.2f;

    private Material oceanMaterial;
    private Renderer oceanRenderer;

    private void Start()
    {
        CreateOcean();
    }

    private void CreateOcean()
    {
        // Crear un plano para el océano
        GameObject oceanPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        oceanPlane.name = "Ocean";
        oceanPlane.transform.parent = transform;
        oceanPlane.transform.localPosition = new Vector3(0, waterLevel, 0);
        oceanPlane.transform.localScale = new Vector3(oceanSize / 10f, 1, oceanSize / 10f);

        // Configurar el material
        oceanRenderer = oceanPlane.GetComponent<Renderer>();
        oceanMaterial = new Material(Shader.Find("Standard"));
        oceanMaterial.color = waterColor;
        oceanMaterial.SetFloat("_Glossiness", 0.8f);
        oceanMaterial.SetFloat("_Metallic", 0.5f);
        
        // Hacer semi-transparente
        oceanMaterial.SetFloat("_Mode", 3);
        oceanMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        oceanMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        oceanMaterial.SetInt("_ZWrite", 0);
        oceanMaterial.DisableKeyword("_ALPHATEST_ON");
        oceanMaterial.EnableKeyword("_ALPHABLEND_ON");
        oceanMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        oceanMaterial.renderQueue = 3000;
        
        oceanRenderer.material = oceanMaterial;
        
        // Remover el collider para que no interfiera
        Destroy(oceanPlane.GetComponent<Collider>());
    }

    private void Update()
    {
        if (animateWaves && oceanMaterial != null)
        {
            // Animar el offset de textura para simular movimiento
            float offset = Time.time * waveSpeed * 0.1f;
            oceanMaterial.mainTextureOffset = new Vector2(offset, offset);
        }
    }
}
