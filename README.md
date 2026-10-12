<div align="center">

# 🦆 Mamá Pato

**Una mamá. Un montón de patitos. Y unos zorros con otras intenciones.**

Explora, reúne a tu familia y no dejes a nadie atrás.

![Unity](https://img.shields.io/badge/Unity-6000.6.5f1-222222?logo=unity&logoColor=white)
![C#](https://img.shields.io/badge/Lenguaje-C%23-512BD4)
![URP](https://img.shields.io/badge/Render-URP-38761D)

[El juego](#-el-juego) · [Controles](#-controles) · [Cómo jugar](#-abrir-el-proyecto)

<img src="gameplay.png" alt="Mamá Pato guiando a sus patitos por el paisaje del juego" width="960">

</div>

## 🌿 El juego

**Mamá Pato** es un juego 3D desarrollado en Unity en el que guías a una mamá pato por un entorno natural generado proceduralmente. Los patitos cercanos se incorporan a tu fila y siguen tus pasos… ¡incluidos los saltos!

Pero el paseo no está libre de peligros: los zorros acechan a los más pequeños. Acércate a ellos para espantarlos y vuelve a reunir a los patitos que se hayan quedado separados.

> El reto: mantener a la familia unida mientras exploras.

## ✨ Qué encontrarás

- **Una fila con vida propia.** Los patitos siguen al que tienen delante y saltan en cadena.
- **Rescate y reencuentros.** Encuentra patitos sueltos y recupera los grupos separados tras un ataque.
- **Zorros al acecho.** Pasean, persiguen a los patitos y huyen cuando mamá pato los intimida.
- **Paisajes procedurales.** Terreno con ruido Perlin, lagos y vegetación generada al iniciar la escena.
- **Cámara de seguimiento.** Una vista elevada para acompañar a tu familia durante el recorrido.

## 🎮 Controles

| Tecla | Acción |
| :---: | --- |
| **W A S D** | Mover a mamá pato |
| **Espacio** | Saltar |
| **Esc** | Pausar / reanudar |

**Para espantar a un zorro**, acércate hasta tocarlo con mamá pato: no hace falta una tecla de ataque.

## 🚀 Abrir el proyecto

### Requisitos

- [Unity Hub](https://unity.com/download).
- **Unity Editor 6000.6.5f1 (Unity 6.6)**, la versión utilizada por el proyecto.
- Una copia local de este repositorio.

### Primer paseo

1. Clona el repositorio o descárgalo desde **Code → Download ZIP** y descomprímelo.
2. En Unity Hub, selecciona **Add / Añadir** y elige la carpeta que contiene `Assets`, `Packages` y `ProjectSettings`.
3. Abre el proyecto con **Unity 6000.6.5f1 (Unity 6.6)** y espera a que termine la importación de los assets y paquetes.
4. Abre la escena [`Assets/Scenes/SampleScene.unity`](Assets/Scenes/SampleScene.unity).
5. Pulsa **Play** y empieza a reunir a tus patitos.

Las dependencias están declaradas en [`Packages/manifest.json`](Packages/manifest.json); Unity las resuelve al abrir el proyecto.

## 🛠️ Bajo las plumas

| Parte | Tecnología |
| --- | --- |
| Motor | Unity 6.6 |
| Lógica | C# |
| Renderizado | Universal Render Pipeline (URP) |
| Entrada | Unity Input System |
| Movimiento | Rigidbody y comprobaciones de terreno |
| Entorno | Mallas procedurales, ruido Perlin y vegetación |

```text
Assets/
├── Scenes/           # Escena del juego
├── __Script/
│   ├── Personajes/   # Mamá pato, patitos, zorros y gestión de filas
│   ├── Paisajes/     # Terreno, lagos y vegetación
│   └── Otros/        # Interfaz y música
├── __Animaciones/    # Animaciones de los personajes
└── __Materiales/     # Materiales y shaders
Packages/             # Dependencias de Unity
ProjectSettings/      # Configuración del proyecto
```

---

<div align="center">

**Que nadie se quede atrás. 🐥**

</div>
