# Sistema de Límites de Mapa y Océano

Este sistema optimiza tu mapa creando límites invisibles y un océano falso para mejorar el rendimiento del APK.

## Componentes

### 1. **MapBoundary.cs**
Crea límites invisibles que impiden al jugador salir del área jugable.

**Configuración:**
1. Crea un GameObject vacío llamado "MapBoundary"
2. Agrega el componente `MapBoundary`
3. Configura:
   - **Center Point**: Centro de tu mapa (ej: 0, 0, 0)
   - **Boundary Size**: Tamaño del área jugable (ej: 100, 50, 100)
   - **Player**: Arrastra tu XR Rig o déjalo vacío para auto-detectar
   - **Push Back Smooth**: 0.5 (qué tan suave empuja al jugador)

### 2. **OceanEffect.cs**
Crea un plano de agua simple alrededor del mapa.

**Configuración:**
1. Crea un GameObject vacío llamado "Ocean"
2. Agrega el componente `OceanEffect`
3. Configura:
   - **Water Level**: Altura del agua (ej: -5)
   - **Ocean Size**: Tamaño del océano (ej: 500)
   - **Water Color**: Color azul del agua
   - **Animate Waves**: true para movimiento

### 3. **SkyboxOcean.cs**
Crea un océano infinito en el horizonte que sigue al jugador.

**Configuración:**
1. Crea un GameObject vacío llamado "SkyboxOcean"
2. Agrega el componente `SkyboxOcean`
3. Configura:
   - **Water Level**: Misma altura que OceanEffect
   - **Radius**: 200 (distancia del horizonte)
   - **Ocean Color**: Color del horizonte
   - **Rotation Speed**: 1 (movimiento lento)

## Setup Rápido

### Paso 1: Límites Invisibles
```
1. GameObject → Create Empty → "MapBoundary"
2. Add Component → MapBoundary
3. Configurar tamaño según tu terrain
```

### Paso 2: Océano Base
```
1. GameObject → Create Empty → "Ocean"
2. Add Component → OceanEffect
3. Ajustar altura y color
```

### Paso 3: Océano en Horizonte
```
1. GameObject → Create Empty → "SkyboxOcean"  
2. Add Component → SkyboxOcean
3. Vincular con el player (opcional)
```

## Optimización

Para mejor rendimiento:
- Usa **Ocean Size** más pequeño (solo visible cerca)
- Desactiva **Animate Waves** si no es necesario
- Ajusta **Boundary Size** al mínimo necesario
- El **SkyboxOcean** es ligero y perfecto para el horizonte

## Tips

1. **Límites**: Haz el área un poco más grande que tu terrain visible
2. **Agua**: Coloca el nivel del agua justo debajo del terrain
3. **Colores**: Usa azul oscuro para profundidad (#1A3D5C)
4. **Testing**: Activa los Gizmos para ver los límites en el editor

## Resultado

✅ Área de juego limitada (optimización)  
✅ Océano infinito falso (apariencia)  
✅ Jugador no puede salirse  
✅ APK más ligero y optimizado
