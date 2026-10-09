using UnityEngine;

public class GestorAnimaciones : MonoBehaviour
{
    Animator animador;
    int horizontal;
    int vertical;
    public float transicion = 0.1f ;
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

    public void ActualizarValoresAnimacion(float movimientoHorizontal, float movimientoVertical)
    {
        //meter quizá snap https://www.youtube.com/watch?v=YiNCqmAF3Lc&list=PLD_vBJjpCwJsqpD8QRPNPMfVUpPFLVGg4&index=4
        animador.SetFloat(horizontal, movimientoHorizontal, transicion, Time.deltaTime);
        animador.SetFloat(vertical, movimientoVertical, transicion, Time.deltaTime);
    }
}
