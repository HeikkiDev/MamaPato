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

    [Header("Salto (barra espaciadora)")]
    public float alturaSalto = 1.2f;
    [Tooltip("Tras saltar, durante este tiempo el movimiento no se pega a la pendiente (si no, anularía el impulso)")]
    public float tiempoDespegue = 0.2f;
    // Los patitos que siguen a mamá pato comparan este contador para saber que ha saltado
    public int SaltosRealizados { get; private set; }
    float finDespegue;

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
        if (Time.time >= finDespegue && Physics.Raycast(origenRayo, Vector3.down, out RaycastHit suelo, AlturaRayoPendiente + distanciaPegarSuelo, terreno))
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
        bool saltoPedido = gestorAcciones.ConsumirSalto();
        if (gestorPato.interactuando)
            return;

        GestionarMovimiento();
        GestionarRotacion();
        if (saltoPedido)
            GestionarSalto();
    }

    private void GestionarSalto()
    {
        Vector3 origenRayo = transform.position + Vector3.up * AlturaRayoPendiente;
        if (Time.time < finDespegue || !Physics.Raycast(origenRayo, Vector3.down, AlturaRayoPendiente + distanciaPegarSuelo, terreno))
            return;

        Vector3 velocidad = patoRigidbody.velocity;
        velocidad.y = VelocidadSalto(alturaSalto);
        patoRigidbody.velocity = velocidad;
        finDespegue = Time.time + tiempoDespegue;
        SaltosRealizados++;
    }

    public static float VelocidadSalto(float altura) => Mathf.Sqrt(2f * -Physics.gravity.y * altura);
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
