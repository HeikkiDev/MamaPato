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

    private float separacionBackup;

    public Duck PatitoInfo { get; set; }

    // Patitos vivos en escena; evita GameObject.FindGameObjectsWithTag en cada FixedUpdate
    public static readonly List<GestorPatito> Activos = new();

    // En Awake (no Start): InicializarPatito se llama justo después de Instantiate, antes de Start
    private void Awake()
    {
        separacionBackup = separacion;
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
        if (jugador == null)
        {
            accionMovimiento = Vector2.zero;
            GestionarAccionMovimiento();
            if (!DucksInGameManager.Instance.MatandoPatitos)
                EncontrarUltimoPatitoDeLaFila();
            return;
        }

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
        
        if (dx*dx + dy*dy > radio*radio )
        {
            dx = 0;
            dy = 0;
        }
        else
        {
            mx = x - tx;
            my = y - ty;
        }
        patitoRigidbody.velocity = new Vector3(-dx*velocidad, 0, -dy*velocidad);
        giro = Mathf.Asin(my/(Mathf.Sqrt(mx*mx+my*my)+0.0000001f));
        if (mx > 0) {giro = Mathf.PI - giro;};
        gameObject.transform.localRotation = Quaternion.Euler(0,90+180*giro/Mathf.PI,0);
        accionMovimiento = new Vector2(0.25f*dx*velocidad,0.25f*dy*velocidad);
        GestionarAccionMovimiento();
    }

    void EncontrarUltimoPatitoDeLaFila()
    {
        bool isOrphan = PatitoInfo != null && PatitoInfo.IsInOrphanQueue;
        bool isOrphanHead = PatitoInfo != null && PatitoInfo.IsHeadOfOrphanQueue;

        var taggedObjects = DucksInGameManager.Instance.MainQueue.DucksList;

        foreach (Duck obj in taggedObjects)
        {
            float distancia = Vector3.Distance(obj.GameObject.transform.position, gameObject.transform.position);

            if (distancia < percepcion)
            {
                if (isOrphan && isOrphanHead)
                    DucksInGameManager.Instance.RemoveOrphanQueueAndAddToMain(PatitoInfo.OrphanQueueUniqueId);
                else if (!isOrphan)
                    DucksInGameManager.Instance.AddChildToMainQueue(gameObject);

                //Debug.Log("PATITO SOLO ENCONTRADO!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                break;
            }
        }
    }
}