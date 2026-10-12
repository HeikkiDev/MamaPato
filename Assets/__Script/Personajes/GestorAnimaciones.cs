using UnityEngine;

public class GestorAnimaciones : MonoBehaviour
{
    Animator animador;
    int horizontal;
    int vertical;
    public float transicion = 0.1f ;
    [Header("Caída y aterrizaje")]
    [Tooltip("Segundos en el aire antes de reproducir la caída (evita parpadeos al pasar por baches)")]
    public float retrasoCaida = 0.1f;
    bool reproduciendoCaida;
    float tiempoEnElAire;
    private void Awake()
    {
        animador = GetComponent<Animator>();
        horizontal = Animator.StringToHash("Horizontal");
        vertical = Animator.StringToHash("Vertical");
    }
    
    public void IniciarAnimacion(string animacion, bool interactuando)
    {
        animador.SetBool("interactuando", interactuando);
        animador.CrossFade(animacion, 0.1f);
    }

    // Lanza Fall_anim al despegar y Land_anim al tocar suelo, una sola vez cada una.
    // No marcan "interactuando": el pato sigue pudiendo moverse en el aire.
    public void ActualizarEstadoAire(bool enElAire, float deltaTime)
    {
        if (enElAire)
        {
            tiempoEnElAire += deltaTime;
            if (!reproduciendoCaida && tiempoEnElAire >= retrasoCaida)
            {
                reproduciendoCaida = true;
                IniciarAnimacion("Fall_anim", false);
            }
            return;
        }

        tiempoEnElAire = 0;
        if (reproduciendoCaida)
        {
            reproduciendoCaida = false;
            IniciarAnimacion("Land_anim", false);
        }
    }

    public void ActualizarValoresAnimacion(float movimientoHorizontal, float movimientoVertical)
    {
        //meter quizá snap https://www.youtube.com/watch?v=YiNCqmAF3Lc&list=PLD_vBJjpCwJsqpD8QRPNPMfVUpPFLVGg4&index=4
        animador.SetFloat(horizontal, movimientoHorizontal, transicion, Time.deltaTime);
        animador.SetFloat(vertical, movimientoVertical, transicion, Time.deltaTime);
    }
}
