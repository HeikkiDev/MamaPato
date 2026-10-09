using UnityEngine;

public class GestorPato : MonoBehaviour
{
    Animator animador;
    GestorAcciones gestorAcciones;
    PatoLocomocion patoLocomocion;
    public bool interactuando;
    // Start is called before the first frame update
    private void Awake()
    {
        animador = GetComponent<Animator>();
        gestorAcciones = GetComponent<GestorAcciones>();
        patoLocomocion = GetComponent<PatoLocomocion>();
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
