using UnityEngine;
using ithappy.Animals_FREE;
using System.Collections;

[RequireComponent(typeof(CreatureMover))]
public class WildAnimalAI : MonoBehaviour
{
    [Header("Comportamiento del Animal")]
    [SerializeField] private float wanderRadius = 10f; // Radio de patrullaje
    [SerializeField] private float wanderDistance = 5f; // Distancia mínima para cambiar dirección
    [SerializeField] private float idleTime = 3f; // Tiempo de descanso
    [SerializeField] private float walkTime = 5f; // Tiempo caminando
    
    [Header("Velocidades")]
    [SerializeField] private float walkChance = 0.7f; // Probabilidad de caminar vs correr
    [SerializeField] private float changeDirectionTime = 3f; // Cada cuanto cambia de dirección
    
    [Header("Detección de Obstáculos")]
    [SerializeField] private float rayDistance = 3f; // Distancia del raycast
    [SerializeField] private LayerMask obstacleLayer = -1; // Capas de obstáculos
    
    [Header("Zona de Movimiento")]
    [SerializeField] private bool useMovementBounds = false; // Si limitar el área de movimiento
    [SerializeField] private Vector3 boundsCenter; // Centro del área
    [SerializeField] private Vector3 boundsSize = new Vector3(20f, 10f, 20f); // Tamaño del área
    
    [Header("Sistema de Sonidos")]
    [SerializeField] private AudioSource audioSource; // Componente de audio
    [SerializeField] private AudioClip[] footstepSounds; // Sonidos de pisadas al caminar
    [SerializeField] private AudioClip[] animalCallSounds; // Sonidos característicos del animal (rugido, mugido, etc.)
    [SerializeField] private float footstepInterval = 0.5f; // Tiempo entre pisadas
    [SerializeField] private float animalCallFrequency = 0.05f; // Probabilidad de hacer el ruido característico
    [SerializeField] private Vector2 volumeRange = new Vector2(0.4f, 0.7f); // Rango de volumen
    [SerializeField] private Vector2 pitchRange = new Vector2(0.9f, 1.1f); // Rango de pitch
    [SerializeField] private float minSoundDistance = 1f; // Distancia mínima para volumen máximo
    [SerializeField] private float maxSoundDistance = 5f; // Distancia máxima donde se escucha
    
    private CreatureMover creatureMover;
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private Vector3 currentDirection;
    
    private AnimalState currentState;
    private float stateTimer;
    private float directionTimer;
    private float footstepTimer;
    private float animalCallTimer;
    
    private enum AnimalState
    {
        Idle,      // Parado/descansando
        Walking,   // Caminando
        Running    // Corriendo (ocasionalmente)
    }
    
    private void Start()
    {
        creatureMover = GetComponent<CreatureMover>();
        startPosition = transform.position;
        
        // Si no se configuró el centro de los límites, usar la posición inicial
        if (boundsCenter == Vector3.zero)
            boundsCenter = startPosition;
        
        // Configurar AudioSource si no se asignó
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        // Configurar propiedades 3D del AudioSource
        audioSource.spatialBlend = 1f; // Sonido 3D
        audioSource.rolloffMode = AudioRolloffMode.Linear; // Atenuación lineal para control preciso de distancia
        audioSource.minDistance = minSoundDistance;
        audioSource.maxDistance = maxSoundDistance;
        
        // Inicializar timers de sonido
        footstepTimer = footstepInterval;
        animalCallTimer = 5f; // Cambiar a 5 segundos para testing
        
        // Comenzar en estado idle
        ChangeState(AnimalState.Idle);
        
        // Establecer primera dirección aleatoria
        ChooseNewDirection();
    }
    
    private void Update()
    {
        // Actualizar timers
        stateTimer -= Time.deltaTime;
        directionTimer -= Time.deltaTime;
        footstepTimer -= Time.deltaTime;
        animalCallTimer -= Time.deltaTime;
        
        // Cambiar dirección periódicamente
        if (directionTimer <= 0f)
        {
            ChooseNewDirection();
            directionTimer = changeDirectionTime + Random.Range(-1f, 1f);
        }
        
        // Cambiar estado cuando el timer expire
        if (stateTimer <= 0f)
        {
            ChooseNewState();
        }
        
        // Reproducir sonidos según el comportamiento
        HandleSounds();
        
        // Ejecutar comportamiento del estado actual
        ExecuteCurrentState();
        
        // Detectar obstáculos y ajustar dirección
        CheckForObstacles();
        
        // Verificar límites del área
        if (useMovementBounds)
        {
            CheckMovementBounds();
        }
    }
    
    private void ChooseNewState()
    {
        // Probabilidades de cada estado
        float randomValue = Random.Range(0f, 1f);
        
        switch (currentState)
        {
            case AnimalState.Idle:
                // Después de descansar, es más probable que camine
                if (randomValue < 0.8f)
                    ChangeState(AnimalState.Walking);
                else
                    ChangeState(AnimalState.Running);
                break;
                
            case AnimalState.Walking:
                // Después de caminar, puede descansar o seguir moviéndose
                if (randomValue < 0.4f)
                    ChangeState(AnimalState.Idle);
                else if (randomValue < 0.8f)
                    ChangeState(AnimalState.Walking);
                else
                    ChangeState(AnimalState.Running);
                break;
                
            case AnimalState.Running:
                // Después de correr, es probable que descanse o camine
                if (randomValue < 0.6f)
                    ChangeState(AnimalState.Idle);
                else
                    ChangeState(AnimalState.Walking);
                break;
        }
    }
    
    private void ChangeState(AnimalState newState)
    {
        currentState = newState;
        
        switch (newState)
        {
            case AnimalState.Idle:
                stateTimer = idleTime + Random.Range(-1f, 2f);
                break;
            case AnimalState.Walking:
                stateTimer = walkTime + Random.Range(-2f, 3f);
                break;
            case AnimalState.Running:
                stateTimer = Random.Range(2f, 4f); // Correr por menos tiempo
                break;
        }
    }
    
    private void ExecuteCurrentState()
    {
        Vector2 inputAxis = Vector2.zero;
        bool isRunning = false;
        Vector3 lookTarget = transform.position + currentDirection;
        
        switch (currentState)
        {
            case AnimalState.Idle:
                // No moverse, solo mirar alrededor ocasionalmente
                inputAxis = Vector2.zero;
                if (Random.Range(0f, 1f) < 0.01f) // 1% de chance cada frame
                {
                    ChooseNewDirection();
                }
                break;
                
            case AnimalState.Walking:
                // Caminar en la dirección actual
                inputAxis = new Vector2(currentDirection.x, currentDirection.z).normalized;
                isRunning = false;
                break;
                
            case AnimalState.Running:
                // Correr en la dirección actual
                inputAxis = new Vector2(currentDirection.x, currentDirection.z).normalized;
                isRunning = true;
                break;
        }
        
        // Enviar input al CreatureMover
        if (creatureMover != null)
        {
            creatureMover.SetInput(inputAxis, lookTarget, isRunning, false);
        }
    }
    
    private void ChooseNewDirection()
    {
        // Elegir dirección aleatoria
        float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        currentDirection = new Vector3(Mathf.Sin(randomAngle), 0f, Mathf.Cos(randomAngle));
        
        // Si estamos usando límites, intentar dirigirse hacia el centro si estamos muy lejos
        if (useMovementBounds)
        {
            Vector3 toBounds = boundsCenter - transform.position;
            if (toBounds.magnitude > boundsSize.magnitude * 0.4f)
            {
                // Mezclar dirección aleatoria con dirección hacia el centro
                currentDirection = Vector3.Lerp(currentDirection, toBounds.normalized, 0.6f).normalized;
            }
        }
        else
        {
            // Si no usamos límites, intentar mantenerse cerca de la posición inicial
            Vector3 toStart = startPosition - transform.position;
            if (toStart.magnitude > wanderRadius)
            {
                currentDirection = Vector3.Lerp(currentDirection, toStart.normalized, 0.5f).normalized;
            }
        }
    }
    
    private void CheckForObstacles()
    {
        // Raycast hacia adelante para detectar obstáculos
        Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;
        
        if (Physics.Raycast(rayOrigin, currentDirection, out RaycastHit hit, rayDistance, obstacleLayer))
        {
            // Si hay un obstáculo, cambiar dirección
            Vector3 reflectedDirection = Vector3.Reflect(currentDirection, hit.normal);
            currentDirection = reflectedDirection.normalized;
            
            // Reiniciar timer de dirección
            directionTimer = changeDirectionTime;
        }
        
        // Debug: dibujar el ray en Scene view
        Debug.DrawRay(rayOrigin, currentDirection * rayDistance, Color.red);
    }
    
    private void CheckMovementBounds()
    {
        Vector3 position = transform.position;
        Vector3 toBounds = boundsCenter - position;
        
        // Verificar si está fuera de los límites
        if (Mathf.Abs(toBounds.x) > boundsSize.x * 0.5f ||
            Mathf.Abs(toBounds.z) > boundsSize.z * 0.5f)
        {
            // Dirigirse hacia el centro
            currentDirection = toBounds.normalized;
        }
    }
    
    private void HandleSounds()
    {
        // Sonidos de pisadas solo cuando se está moviendo
        if ((currentState == AnimalState.Walking || currentState == AnimalState.Running) && footstepTimer <= 0f)
        {
            PlayFootstepSound();
            
            // Reiniciar timer de pisadas (más rápido si está corriendo)
            float interval = currentState == AnimalState.Running ? footstepInterval * 0.6f : footstepInterval;
            footstepTimer = interval + Random.Range(-0.1f, 0.1f);
        }
        
        // Sonidos característicos del animal ocasionalmente
        if (animalCallTimer <= 0f)
        {
            PlayAnimalCall(); // Quitar la probabilidad para garantizar que suene
            
            // Reiniciar timer a 5 segundos exactos para testing
            animalCallTimer = 5f;
        }
    }
    
    private void PlayFootstepSound()
    {
        if (audioSource == null || footstepSounds == null || footstepSounds.Length == 0) 
            return;
        
        AudioClip clipToPlay = footstepSounds[Random.Range(0, footstepSounds.Length)];
        
        if (clipToPlay != null)
        {
            PlaySoundWithVariation(clipToPlay, 0.6f); // Pisadas más suaves
        }
    }
    
    private void PlayAnimalCall()
    {
        if (audioSource == null || animalCallSounds == null || animalCallSounds.Length == 0) 
            return;
        
        AudioClip clipToPlay = animalCallSounds[Random.Range(0, animalCallSounds.Length)];
        
        if (clipToPlay != null)
        {
            PlaySoundWithVariation(clipToPlay, 1f); // Sonido característico a volumen normal
        }
    }
    
    private void PlaySoundWithVariation(AudioClip clip, float volumeMultiplier = 1f)
    {
        if (audioSource.isPlaying && Random.Range(0f, 1f) < 0.7f)
            return; // No interrumpir sonidos existentes muy seguido
        
        // Aplicar variaciones aleatorias
        audioSource.volume = Random.Range(volumeRange.x, volumeRange.y) * volumeMultiplier;
        audioSource.pitch = Random.Range(pitchRange.x, pitchRange.y);
        
        // Reproducir el sonido
        audioSource.PlayOneShot(clip);
    }
    
    // Método para reproducir sonido específico (útil para eventos especiales)
    public void PlaySpecificSound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            PlaySoundWithVariation(clip);
        }
    }
    
    // Método para cambiar la frecuencia de sonidos en tiempo real
    public void SetAnimalCallFrequency(float newFrequency)
    {
        animalCallFrequency = Mathf.Clamp01(newFrequency);
    }
    
    public void SetFootstepInterval(float interval)
    {
        footstepInterval = Mathf.Max(interval, 0.1f);
    }
    
    // Para mostrar los límites en el editor
    private void OnDrawGizmosSelected()
    {
        // Dibujar radio de patrullaje
        Gizmos.color = Color.yellow;
        Vector3 center = Application.isPlaying ? startPosition : transform.position;
        Gizmos.DrawWireSphere(center, wanderRadius);
        
        // Dibujar límites del área si están habilitados
        if (useMovementBounds)
        {
            Gizmos.color = Color.green;
            Vector3 boundsPos = boundsCenter;
            if (boundsCenter == Vector3.zero)
                boundsPos = transform.position;
            Gizmos.DrawWireCube(boundsPos, boundsSize);
        }
        
        // Dibujar dirección actual
        if (Application.isPlaying)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, currentDirection * 2f);
        }
    }
    
    // Métodos públicos para controlar el comportamiento
    public void SetWanderRadius(float radius)
    {
        wanderRadius = radius;
    }
    
    public void SetMovementBounds(Vector3 center, Vector3 size)
    {
        boundsCenter = center;
        boundsSize = size;
        useMovementBounds = true;
    }
    
    public void ForceNewDirection()
    {
        ChooseNewDirection();
        directionTimer = changeDirectionTime;
    }
    
    // Métodos públicos para controlar sonidos
    public void SetAudioSource(AudioSource newAudioSource)
    {
        audioSource = newAudioSource;
    }
    
    public void SetSoundArrays(AudioClip[] footsteps, AudioClip[] animalCalls)
    {
        footstepSounds = footsteps;
        animalCallSounds = animalCalls;
    }
    
    public void MuteAnimal(bool mute)
    {
        if (audioSource != null)
        {
            audioSource.mute = mute;
        }
    }
    
    public void SetVolumeRange(float minVolume, float maxVolume)
    {
        volumeRange = new Vector2(minVolume, maxVolume);
    }
}