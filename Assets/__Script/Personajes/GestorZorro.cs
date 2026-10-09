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
    [Range(0.5f, 10.0f)]
    public float radioIntimidacion;
    [Tooltip("Distancia a la que mama pato intimida al zorro si lo mira de frente")]
    [Range(0.5f, 10.0f)]
    public float distanciaIntimidacionFrontal = 4f;
    [Tooltip("Ángulo total (grados) del cono de visión de mama pato que intimida")]
    [Range(10f, 180f)]
    public float anguloIntimidacion = 90f;
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
    public GameObject patitoMasCercano;

    public float changeDirectionInterval = 1f;

    public bool IsKilling { get; set; }
    public bool IsStunt { get; set; }

    private Coroutine killCoroutine;

    void Start()
    {
        StartCoroutine(CambiarDireccionZorro());
    }

    public void InicializarZorro()
    {
        gestorAnimaciones = GetComponent<GestorAnimaciones>();
        zorroRigidbody = GetComponent<Rigidbody>();
        Physics.IgnoreCollision(jugador.GetComponent<Collider>(), GetComponent<Collider>());
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
            return;

        if (IsStunt)
        {
            HuirDeMamaPato();
            return;
        }

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
            MoverZorroAleatoriamente();
            return;
        }

        zorroRigidbody.velocity = new Vector3(dx*velocidad, zorroRigidbody.velocity.y, dy*velocidad);
        giro = Mathf.Asin(my/(Mathf.Sqrt(mx*mx+my*my)+0.0000001f));
        if (mx > 0) {giro = Mathf.PI - giro;};
        gameObject.transform.localRotation = Quaternion.Euler(0, 90 + 180*giro/Mathf.PI, 0);
        accionMovimiento = new Vector2(0.25f*dx*velocidad,0.25f*dy*velocidad);
        GestionarAccionMovimiento();
    }

    // Muy cerca de mama pato, o dentro de su campo de visión frontal
    bool EstaIntimidado()
    {
        Vector3 haciaZorro = transform.position - jugador.transform.position;
        haciaZorro.y = 0;
        float distancia = haciaZorro.magnitude;
        if (distancia < radioIntimidacion)
            return true;

        Vector3 frenteMama = jugador.transform.forward;
        frenteMama.y = 0;
        return distancia < distanciaIntimidacionFrontal
            && Vector3.Angle(frenteMama, haciaZorro) < anguloIntimidacion * 0.5f;
    }

    // Corre en dirección contraria a mama pato mientras dura el aturdimiento
    void HuirDeMamaPato()
    {
        Vector3 huida = transform.position - jugador.transform.position;
        huida.y = 0;
        huida = huida.sqrMagnitude > 0.0001f ? huida.normalized : transform.forward;

        transform.rotation = Quaternion.LookRotation(huida);
        Vector3 velocidadHuir = huida * velocidadHuida;
        velocidadHuir.y = zorroRigidbody.velocity.y;
        zorroRigidbody.velocity = velocidadHuir;

        accionMovimiento = new Vector2(huida.x, huida.z);
        gestorAnimaciones.ActualizarValoresAnimacion(0, 1); // animación de correr
    }


    void MoverZorroAleatoriamente()
    {
        dy = Mathf.Cos(Mathf.PI * gameObject.transform.localRotation.eulerAngles.y / 180);
        dx = Mathf.Sin(Mathf.PI * gameObject.transform.localRotation.eulerAngles.y / 180);
        zorroRigidbody.velocity = new Vector3(dx * 2, zorroRigidbody.velocity.y, dy * 2);
        accionMovimiento = new Vector2(dx, dy);
        GestionarAccionMovimiento();
    }

    IEnumerator CambiarDireccionZorro()
    {
        while (true)
        {
            // Gira el rumbo actual (ángulos Euler, no componentes del quaternion)
            Vector3 euler = gameObject.transform.localEulerAngles;
            euler.y += Random.Range(-30f, 30f);
            gameObject.transform.localRotation = Quaternion.Euler(euler);

            // Wait for a random amount of time
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
