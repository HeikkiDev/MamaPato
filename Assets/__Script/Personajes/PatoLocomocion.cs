using UnityEngine;

public class PatoLocomocion : MonoBehaviour
{
    GestorPato gestorPato;
    GestorAcciones gestorAcciones;
    GestorAnimaciones gestorAnimaciones;
    Vector3 direccionMovimiento;
    //Transform cameraObject;
    public Transform cameraObject;
    Rigidbody patoRigidbody;
    [Header("Vuelo")]
    public float desfaseRaycast;
    public float tiempoVuelo;
    public float impulso;
    public float salto;
    public float gravedad;
    public LayerMask terreno;
    [Tooltip("Distancia máxima del SphereCast de suelo (8 = valor efectivo que se usaba antes)")]
    public float distanciaSuelo = 8f;
    [Tooltip("Por debajo de esta distancia al suelo, el movimiento sigue la pendiente del terreno")]
    public float distanciaPegarSuelo = 0.3f;
    const float AlturaRayoPendiente = 0.5f;

    public float velocidadMovimiento = 5;
    public float velocidadRotacion = 15;
    [Header("Semáforos de movimiento")]
    public bool enTierra;
    public void Awake()
    {
        gestorPato = GetComponent<GestorPato>();
        gestorAcciones = GetComponent<GestorAcciones>();
        gestorAnimaciones = GetComponent<GestorAnimaciones>();
        patoRigidbody = GetComponent<Rigidbody>();
    }
    #region Gestores
    private void GestionarMovimiento()
    {
        direccionMovimiento = cameraObject.forward * gestorAcciones.accionVertical;
        direccionMovimiento = direccionMovimiento + cameraObject.right * gestorAcciones.accionHorizontal;
        direccionMovimiento.y = 0;
        direccionMovimiento.Normalize();

        Vector3 velocidad;
        Vector3 origenRayo = transform.position + Vector3.up * AlturaRayoPendiente;
        if (Physics.Raycast(origenRayo, Vector3.down, out RaycastHit suelo, AlturaRayoPendiente + distanciaPegarSuelo, terreno))
        {
            // Avanza paralela a la pendiente: sube y baja colinas a velocidad constante
            velocidad = Vector3.ProjectOnPlane(direccionMovimiento, suelo.normal).normalized * velocidadMovimiento;
        }
        else
        {
            // En el aire conserva la velocidad vertical para que actúe la gravedad
            velocidad = direccionMovimiento * velocidadMovimiento;
            velocidad.y = patoRigidbody.velocity.y;
        }
        patoRigidbody.velocity = velocidad;
    }
    
    private void GestionarRotacion()
    {
        Vector3 direccionObjetivo = Vector3.zero;
        direccionObjetivo = cameraObject.forward * gestorAcciones.accionVertical; 
        direccionObjetivo = direccionObjetivo + cameraObject.right * gestorAcciones.accionHorizontal;
        direccionObjetivo.Normalize();
        direccionObjetivo.y = 0;
        if (direccionObjetivo == Vector3.zero)
            direccionObjetivo = transform.forward;
        Quaternion rotacionObjetivo = Quaternion.LookRotation(direccionObjetivo);
        Quaternion patoRotacion = Quaternion.Slerp(transform.rotation, rotacionObjetivo, velocidadRotacion * Time.deltaTime);
        transform.rotation = patoRotacion;
    }
    #endregion
    public void GestionarLocomocion()
    {   
        GestionarVuelo();
        if (gestorPato.interactuando)
            return;

        GestionarMovimiento();
        GestionarRotacion();
    }
    private void GestionarVuelo()
    {
        RaycastHit hit;
        Vector3 origenRaycast = transform.position;
        origenRaycast.y += desfaseRaycast;
        if (!enTierra)
        {
            if (!gestorPato.interactuando)
            {
                gestorAnimaciones.IniciarAnimacion("Fall_anim", true);
            }
            tiempoVuelo += Time.deltaTime;
            patoRigidbody.AddForce(transform.forward*impulso);
            patoRigidbody.AddForce(-Vector3.up * gravedad * tiempoVuelo + Vector3.up*salto);
        }
        if (Physics.SphereCast(origenRaycast, 0.1f, -Vector3.up, out hit, distanciaSuelo, terreno))
        {
            if (!enTierra)// && !gestorPato.interactuando)
            {
                gestorAnimaciones.IniciarAnimacion("Land_anim", true);
            }
            tiempoVuelo = 0;
            enTierra = true;
        }
        else
        {
            enTierra = false;
        }
    }
}
