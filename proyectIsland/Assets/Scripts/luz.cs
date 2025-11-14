using UnityEngine;

public class LightDayNightController : MonoBehaviour
{
    public Light targetLight;        // La luz a controlar
    public Color nightColor = Color.blue; // Color para la noche
    public float transitionSpeed = 1f;   // Velocidad del cambio

    private Color dayColor;          // Se guardará el color original del componente
    private bool isDay = true;    // Estado actual
    private float timer=0f;

    void Start()
    {
        if (targetLight == null)
            targetLight = GetComponent<Light>();

        // 🔹 Guarda el color original del componente Light (día)
        dayColor = targetLight.color;
    }

    void Update()
    {
        timer += Time.deltaTime;
        // 🔹 Interpola suavemente entre los colores
        if (isDay && timer>=10f) 
            targetLight.color = Color.Lerp(targetLight.color, dayColor, Time.deltaTime * transitionSpeed);
        else
            targetLight.color = Color.Lerp(targetLight.color, nightColor, Time.deltaTime * transitionSpeed);
    }

    // 🔹 Método público para cambiar entre día y noche desde otros scripts o eventos
    public void SetDay(bool value)
    {
        isDay = value;
    }

    // 🔹 Alternar manualmente (por ejemplo, con tecla)
    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 150, 30), "Cambiar Día/Noche"))
        {
            isDay = !isDay;
        }
    }
}
