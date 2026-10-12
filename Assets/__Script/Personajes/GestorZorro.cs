using System.Collections;
using UnityEngine;

public class GestorZorro : MonoBehaviour
{
    [Header("SCRIPT DE FAUSTO")]
    public GameObject jugador;
    [Range(2.0f, 10.0f)]
    public float percepcion;
    [Range(2.0f, 10.0f)]
    public float percepcionMatar;
    [Range(0.2f, 2.0f)]
    public float tol;
    [Tooltip("Velocidad a la que huye el zorro intimidado")]
    public float velocidadHuida = 3f;
    [Range(0.05f, 0.3f)]
    public float acel;
    public float x, y, tx, ty, jx, jy, dx, dy, mx, my;
    public float giro = 0;
    [Range(0.25f, 5.0f)]
    public float velocidad = 1;

    GestorAnimaciones gestorAnimaciones;
    public Vector2 accionMovimiento;
    public float cantidadMovimiento;
    public float accionVertical;
    public float accionHorizontal;
    Rigidbody zorroRigidbody;
    Collider zorroCollider;
    Collider mamaCollider;
    public GameObject patitoMasCercano;

    public float changeDirectionInterval = 1f;

    [Header("Animación")]
    [Tooltip("Velocidad al pasear; se corresponde con la animación de caminar")]
    public float velocidadPaseo = 2f;
    [Tooltip("A esta velocidad (o más) se reproduce la animación de correr")]
    public float velocidadCarrera = 3f;
    [Tooltip("Rapidez con la que gira hacia su nuevo rumbo (sin saltos bruscos)")]
    public float velocidadGiro = 8f;
    [Tooltip("Probabilidad, en cada cambio de rumbo, de pararse un rato a olfatear")]
    [Range(0f, 1f)]
    public float probabilidadPausa = 0.2f;
    public Vector2 duracionPausa = new Vector2(1f, 2.5f);

    private float rumboPaseo;
    private bool enPausa;
    private bool paseando;

    public bool IsKilling { get; set; }
    public bool IsStunt { get; set; }

    private Coroutine killCoroutine;

    void Start()
    {
        rumboPaseo = transform.eulerAngles.y;
        StartCoroutine(CambiarDireccionZorro());
    }

    public void InicializarZorro()
    {
        gestorAnimaciones = GetComponent<GestorAnimaciones>();
        zorroRigidbody = GetComponent<Rigidbody>();
        zorroCollider = GetComponent<Collider>();
        mamaCollider = jugador.GetComponent<Collider>();
        Physics.IgnoreCollision(mamaCollider, zorroCollider);
    }

    // Blend tree: 0 = esperando, 0.5 = caminar, 1 = correr. Se calcula con la velocidad real
    // para que las patas no "patinen" (antes dependía de la distancia al objetivo).
    private void GestionarAccionMovimiento()
    {
        accionVertical = accionMovimiento.y;
        accionHorizontal = accionMovimiento.x;
        float rapidez = accionMovimiento.magnitude;
        if (rapidez <= velocidadPaseo)
            cantidadMovimiento = 0.5f * rapidez / velocidadPaseo;
        else
            cantidadMovimiento = 0.5f + 0.5f * Mathf.Clamp01((rapidez - velocidadPaseo) / (velocidadCarrera - velocidadPaseo));
        gestorAnimaciones.ActualizarValoresAnimacion(0, cantidadMovimiento);
    }

    private void Mover(Vector3 velocidadHorizontal)
    {
        velocidadHorizontal.y = zorroRigidbody.linearVelocity.y;
        zorroRigidbody.linearVelocity = velocidadHorizontal;
        accionMovimiento = new Vector2(velocidadHorizontal.x, velocidadHorizontal.z);
        GestionarAccionMovimiento();
    }

    private void GirarHacia(Vector3 direccion, float multiplicadorGiro = 1f)
    {
        direccion.y = 0;
        if (direccion.sqrMagnitude < 0.0001f)
            return;
        Quaternion objetivo = Quaternion.LookRotation(direccion);
        transform.rotation = Quaternion.Slerp(transform.rotation, objetivo, velocidadGiro * multiplicadorGiro * Time.deltaTime);
    }
    
    private void FixedUpdate()
    {
        if (jugador == null)
            return;

        if (IsStunt)
        {
            paseando = false;
            HuirDeMamaPato();
            return;
        }
        bool estabaPaseando = paseando;
        paseando = false;

        EncontrarPatitoMasCercano();
        MatarPatitoMasCercano();

        x = gameObject.transform.position[0];
        y = gameObject.transform.position[2];
        if (EstaIntimidado())
        {
            // Intimidado por mama pato: abandona el ataque y huye
            patitoMasCercano = null;
            if (killCoroutine != null)
            {
                StopCoroutine(killCoroutine);
                killCoroutine = null;
            }
            IsKilling = false;
            IsStunt = true;
            gestorAnimaciones.IniciarAnimacion("Empty", false); // corta la animación de ataque
            if (jugador.TryGetComponent(out GestorPato gestorPato))
                gestorPato.Espantar();
            StartCoroutine(ResetStuntInSeconds(4));
            HuirDeMamaPato();
            return;
        }
        else if (patitoMasCercano != null)
        {
            // Patito cercano encontrado
            tx = patitoMasCercano.transform.position[0];
            ty = patitoMasCercano.transform.position[2];
            dx = -x + tx;
            dy = -y + ty;
            mx = x - tx;
            my = y - ty;
        }
        else
        {
            // Patito cercano NO encontrado
            if (!estabaPaseando)
                rumboPaseo = transform.eulerAngles.y; // sigue paseando desde donde mira ahora
            paseando = true;
            MoverZorroAleatoriamente();
            return;
        }

        GirarHacia(new Vector3(dx, 0, dy));
        Mover(new Vector3(dx*velocidad, 0, dy*velocidad));
    }

    // Mama pato choca con el zorro (las colisiones físicas entre ambos están desactivadas, así que se comprueba el solapamiento)
    bool EstaIntimidado()
    {
        return Physics.ComputePenetration(
            zorroCollider, zorroCollider.transform.position, zorroCollider.transform.rotation,
            mamaCollider, mamaCollider.transform.position, mamaCollider.transform.rotation,
            out _, out _);
    }

    // Corre en dirección contraria a mama pato mientras dura el aturdimiento
    void HuirDeMamaPato()
    {
        Vector3 huida = transform.position - jugador.transform.position;
        huida.y = 0;
        huida = huida.sqrMagnitude > 0.0001f ? huida.normalized : transform.forward;

        // Al asustarse se da la vuelta rápido, sin teletransportar la rotación
        GirarHacia(huida, 3f);
        Mover(huida * velocidadHuida);
    }


    void MoverZorroAleatoriamente()
    {
        if (enPausa)
        {
            Mover(Vector3.zero); // animación de esperar
            return;
        }

        GirarHacia(Quaternion.Euler(0, rumboPaseo, 0) * Vector3.forward);
        Mover(transform.forward * velocidadPaseo);
    }

    IEnumerator CambiarDireccionZorro()
    {
        while (true)
        {
            if (paseando && Random.value < probabilidadPausa)
            {
                enPausa = true;
                yield return new WaitForSeconds(Random.Range(duracionPausa.x, duracionPausa.y));
                enPausa = false;
            }
            else
            {
                // Nuevo rumbo; el giro se hace suavemente en MoverZorroAleatoriamente
                rumboPaseo += Random.Range(-30f, 30f);
            }

            yield return new WaitForSeconds(changeDirectionInterval);
        }
    }

    void EncontrarPatitoMasCercano()
    {
        patitoMasCercano = null;

        float distanciaMasCercana = percepcion;

        foreach (GestorPatito patito in GestorPatito.Activos)
        {
            float distancia = Vector3.Distance(patito.transform.position, gameObject.transform.position); 

            if (distancia < distanciaMasCercana) // Check if this object is closer than the previous closest object
            {
                distanciaMasCercana = distancia; // Update closest distance
                patitoMasCercano = patito.gameObject; // Set this object as the closest one
            }
        }
    }

    void MatarPatitoMasCercano()
    {
        if(IsKilling)
            return;

        if (patitoMasCercano == null)
            return;

        float distancia = Vector3.Distance(patitoMasCercano.transform.position, gameObject.transform.position);

        if (distancia < percepcionMatar)
        {
            IsKilling = true;

            gestorAnimaciones.IniciarAnimacion("Attack_anim", true);

            killCoroutine = StartCoroutine(KillPatitoInSeconds(patitoMasCercano, 3));
        }
    }

    // Mata al patito que se eligió al empezar el ataque, no al que esté más cerca al terminar
    IEnumerator KillPatitoInSeconds(GameObject objetivo, int secs)
    {
        yield return new WaitForSeconds(secs);

        if (objetivo != null
            && Vector3.Distance(objetivo.transform.position, gameObject.transform.position) < percepcion)
        {
            var gestorPatito = objetivo.GetComponent<GestorPatito>();

            if (gestorPatito.PatitoInfo?.IsInMainQueue == true)
            {
                Debug.Log("PATITO REMOVED FROM MAIN !!!");
                DucksInGameManager.Instance.RemoveChildFromMainQueue(gestorPatito.PatitoInfo);
            }
            else if(gestorPatito.PatitoInfo?.IsInOrphanQueue == true)
            {
                Debug.Log("PATITO REMOVED FROM ORPHAN !!!");
                DucksInGameManager.Instance.RemoveChildFromOrphanQueue(gestorPatito.PatitoInfo);
            }

            gestorPatito.jugador = null;
            gestorPatito.PatitoInfo = null;

            Destroy(objetivo);

            // Reset animation
            accionMovimiento = Vector2.zero;
            GestionarAccionMovimiento();

            yield return new WaitForSeconds(4);
        }

        IsKilling = false;
        killCoroutine = null;
    }

    IEnumerator ResetStuntInSeconds(int secs)
    {
        yield return new WaitForSeconds(secs);

        IsStunt = false;
    }
}
