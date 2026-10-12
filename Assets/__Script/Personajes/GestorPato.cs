using UnityEngine;

public class GestorPato : MonoBehaviour
{
    Animator animador;
    GestorAcciones gestorAcciones;
    PatoLocomocion patoLocomocion;
    GestorAnimaciones gestorAnimaciones;
    public bool interactuando;
    // Start is called before the first frame update
    private void Awake()
    {
        animador = GetComponent<Animator>();
        gestorAcciones = GetComponent<GestorAcciones>();
        patoLocomocion = GetComponent<PatoLocomocion>();
        gestorAnimaciones = GetComponent<GestorAnimaciones>();
    }

    // Aleteo al chocar con un zorro para espantarlo; no bloquea el movimiento
    public void Espantar()
    {
        gestorAnimaciones.IniciarAnimacion("Espantar_anim", false);
    }
    private void Update()
    {
        gestorAcciones.GestionarAcciones();
    }
    private void FixedUpdate()
    {
        patoLocomocion.GestionarLocomocion();
    }
    private void LateUpdate()
    {
        interactuando = animador.GetBool("interactuando");
    }
}
