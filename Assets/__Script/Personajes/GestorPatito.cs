using System.Collections.Generic;
using UnityEngine;
using static DucksInGameManager;

public class GestorPatito : MonoBehaviour
{   
    [Header("SCRIPT DE FAUSTO")]
    public GameObject jugador;
    [Range(0.2f, 2.0f)]
    public float separacion;
    [Range(2.5f, 10.0f)]
    public float radio;
    [Range(0.05f, 0.3f)]
    public float acel;
    public float x, y, tx, ty, dx, dy, mx, my;
    public float giro = 0;
    [Range(0.25f, 5.0f)]
    public float velocidad = 1;
    [Range(2.0f, 10.0f)]
    public float percepcion;

    GestorAnimaciones gestorAnimaciones;
    public Vector2 accionMovimiento;
    public float cantidadMovimiento;
    public float accionVertical;
    public float accionHorizontal;
    Rigidbody patitoRigidbody;

    [Tooltip("Segundos sin acercarse a su guía (estando lejos) antes de reaparecer justo detrás de él")]
    public float segundosAtascado = 2f;
    private float mejorDistancia = float.MaxValue;
    private float tiempoSinAcercarse;

    [Header("Salto")]
    [Tooltip("Segundos que tarda en imitar el salto del pato al que sigue (efecto cadena a lo largo de la fila)")]
    public float retrasoSalto = 0.12f;
    public float alturaSalto = 1f;
    [Tooltip("Tras saltar, durante este tiempo el movimiento no se pega a la pendiente (si no, anularía el impulso)")]
    public float tiempoDespegue = 0.2f;
    // El patito de detrás compara este contador para saber que este ha saltado
    public int SaltosRealizados { get; private set; }
    private GameObject guiaObservado;
    private PatoLocomocion guiaPato;
    private GestorPatito guiaPatito;
    private int saltosGuiaVistos;
    private float saltoPendienteEn = -1;
    private float finDespegue;
    private const float MaxEsperaSalto = 0.5f;

    private float separacionBackup;
    private int capaTerreno;
    private const float AlturaRayoPendiente = 0.3f;
    private const float DistanciaPegarSuelo = 0.2f;

    public Duck PatitoInfo { get; set; }

    // Patitos vivos en escena; evita GameObject.FindGameObjectsWithTag en cada FixedUpdate
    public static readonly List<GestorPatito> Activos = new();

    // En Awake (no Start): InicializarPatito se llama justo después de Instantiate, antes de Start
    private void Awake()
    {
        separacionBackup = separacion;
        capaTerreno = LayerMask.GetMask("Terrain");

        // Los patitos solo chocan con el terreno: así no se atascan en arbustos, flores, rocas u otros animales
        GetComponent<Collider>().excludeLayers = ~capaTerreno;
    }

    private void OnEnable() => Activos.Add(this);
    private void OnDisable() => Activos.Remove(this);

    public void InicializarPatito()
    {
        gestorAnimaciones = GetComponent<GestorAnimaciones>();
        patitoRigidbody = GetComponent<Rigidbody>();
        if (jugador != null)
        {
            foreach (var patito in DucksInGameManager.Instance.MainQueue.DucksList)
            {
                Physics.IgnoreCollision(patito.GameObject.GetComponent<Collider>(), GetComponent<Collider>());
            }
        }

        gestorAnimaciones.ActualizarValoresAnimacion(0, 0);

        if (PatitoInfo?.IsHeadOfMainQueue == true)
            separacion = separacionBackup + 0.2f;
        else
            separacion = separacionBackup;
    }

    private void GestionarAccionMovimiento()
    {
        accionVertical = accionMovimiento.y;
        accionHorizontal = accionMovimiento.x;
        cantidadMovimiento = Mathf.Clamp01(Mathf.Abs(accionVertical)+Mathf.Abs(accionHorizontal));
        gestorAnimaciones.ActualizarValoresAnimacion(0, cantidadMovimiento);
    }

    private void FixedUpdate()
    {
        bool esHuerfano = PatitoInfo != null && PatitoInfo.IsInOrphanQueue;

        if (jugador == null)
        {
            accionMovimiento = Vector2.zero;
            GestionarAccionMovimiento();
            BuscarFilaPrincipal();
            return;
        }

        // Cualquier patito de una fila huérfana (no solo el primero) puede devolverla a la fila principal
        if (esHuerfano)
            BuscarFilaPrincipal();

        tx = jugador.transform.position[0];
        ty = jugador.transform.position[2];
        x = gameObject.transform.position[0];
        y = gameObject.transform.position[2];
        dx = 0;
        dy = 0;

        if (x > tx + separacion || x < tx - separacion )
            dx = x - tx;
        if (y > ty + separacion || y < ty - separacion )
            dy = y - ty;
        
        float distancia = Mathf.Sqrt(dx*dx + dy*dy);
        if (distancia > radio)
        {
            if (distancia < mejorDistancia - 0.05f)
            {
                mejorDistancia = distancia;
                tiempoSinAcercarse = 0;
            }
            else if ((tiempoSinAcercarse += Time.fixedDeltaTime) > segundosAtascado)
            {
                ReaparecerDetrasDelGuia();
                return;
            }

            // Se ha quedado atrás: sigue a velocidad máxima en lugar de pararse para siempre
            dx *= radio / distancia;
            dy *= radio / distancia;
        }
        else
        {
            mejorDistancia = float.MaxValue;
            tiempoSinAcercarse = 0;
        }
        mx = x - tx;
        my = y - ty;
        patitoRigidbody.velocity = SeguirPendiente(new Vector3(-dx*velocidad, 0, -dy*velocidad));
        giro = Mathf.Asin(my/(Mathf.Sqrt(mx*mx+my*my)+0.0000001f));
        if (mx > 0) {giro = Mathf.PI - giro;};
        gameObject.transform.localRotation = Quaternion.Euler(0,90+180*giro/Mathf.PI,0);
        accionMovimiento = new Vector2(0.25f*dx*velocidad,0.25f*dy*velocidad);
        GestionarAccionMovimiento();
        ImitarSaltoDelGuia();
    }

    int SaltosDelGuia()
    {
        if (guiaPato != null)
            return guiaPato.SaltosRealizados;
        if (guiaPatito != null)
            return guiaPatito.SaltosRealizados;
        return saltosGuiaVistos;
    }

    void ImitarSaltoDelGuia()
    {
        if (jugador != guiaObservado)
        {
            // Nuevo guía: se toma su contador actual para no repetir saltos antiguos
            guiaObservado = jugador;
            guiaPato = jugador.GetComponent<PatoLocomocion>();
            guiaPatito = jugador.GetComponent<GestorPatito>();
            saltosGuiaVistos = SaltosDelGuia();
            saltoPendienteEn = -1;
            return;
        }

        int saltosGuia = SaltosDelGuia();
        if (saltosGuia != saltosGuiaVistos)
        {
            saltosGuiaVistos = saltosGuia;
            saltoPendienteEn = Time.time + retrasoSalto;
        }

        if (saltoPendienteEn < 0 || Time.time < saltoPendienteEn)
            return;

        if (EnSuelo())
        {
            Vector3 velocidadActual = patitoRigidbody.velocity;
            velocidadActual.y = PatoLocomocion.VelocidadSalto(alturaSalto);
            patitoRigidbody.velocity = velocidadActual;
            finDespegue = Time.time + tiempoDespegue;
            SaltosRealizados++;
            saltoPendienteEn = -1;
        }
        else if (Time.time > saltoPendienteEn + MaxEsperaSalto)
        {
            saltoPendienteEn = -1;
        }
    }

    bool EnSuelo()
    {
        Vector3 origen = transform.position + Vector3.up * AlturaRayoPendiente;
        return Time.time >= finDespegue && Physics.Raycast(origen, Vector3.down, AlturaRayoPendiente + DistanciaPegarSuelo, capaTerreno);
    }

    // Avanza paralelo al terreno para poder subir y bajar pendientes; en el aire conserva la gravedad
    Vector3 SeguirPendiente(Vector3 velocidadHorizontal)
    {
        Vector3 origen = transform.position + Vector3.up * AlturaRayoPendiente;
        if (Time.time >= finDespegue && Physics.Raycast(origen, Vector3.down, out RaycastHit suelo, AlturaRayoPendiente + DistanciaPegarSuelo, capaTerreno))
            return Vector3.ProjectOnPlane(velocidadHorizontal, suelo.normal).normalized * velocidadHorizontal.magnitude;

        velocidadHorizontal.y = patitoRigidbody.velocity.y;
        return velocidadHorizontal;
    }

    void ReaparecerDetrasDelGuia()
    {
        Transform guia = jugador.transform;
        Vector3 destino = guia.position - guia.forward * (separacion + 0.2f);
        if (Physics.Raycast(destino + Vector3.up * 5f, Vector3.down, out RaycastHit suelo, 20f, capaTerreno))
            destino.y = suelo.point.y;
        else
            destino.y = guia.position.y;

        patitoRigidbody.position = destino;
        transform.position = destino;
        patitoRigidbody.velocity = Vector3.zero;
        mejorDistancia = float.MaxValue;
        tiempoSinAcercarse = 0;
    }

    void BuscarFilaPrincipal()
    {
        bool isOrphan = PatitoInfo != null && PatitoInfo.IsInOrphanQueue;

        // Tras separarse por un ataque, la fila huérfana espera unos segundos antes de poder volver
        if (isOrphan && Time.time - PatitoInfo.OrphanedAt < DucksInGameManager.SegundosEsperaHuerfanos)
            return;

        var taggedObjects = DucksInGameManager.Instance.MainQueue.DucksList;

        foreach (Duck obj in taggedObjects)
        {
            float distancia = Vector3.Distance(obj.GameObject.transform.position, gameObject.transform.position);

            if (distancia < percepcion)
            {
                if (isOrphan)
                    DucksInGameManager.Instance.RemoveOrphanQueueAndAddToMain(PatitoInfo.OrphanQueueUniqueId);
                else
                    DucksInGameManager.Instance.AddChildToMainQueue(gameObject);

                //Debug.Log("PATITO SOLO ENCONTRADO!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                break;
            }
        }
    }
}