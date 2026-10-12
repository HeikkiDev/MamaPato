using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GestorAcciones : MonoBehaviour
{
    PatoControles controles;
    public GestorAnimaciones gestorAnimaciones;
    public Vector2 accionMovimiento;
    public float cantidadMovimiento;
    public float accionVertical;
    public float accionHorizontal;
    public bool saltoPedido;

    private bool gamePaused;

    private void Awake()
    {
        gestorAnimaciones = GetComponent<GestorAnimaciones>();
    }

    private void Update()
    {
        // This demonstrates how to pause the game!
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            gamePaused = !gamePaused;
            Time.timeScale = gamePaused ? 0f : 1f; // Animators MUST use Update Mode "Unscaled Time"
        }
    }

    private void OnEnable()
    {
        if (controles == null)
        {
            controles = new PatoControles();
            controles.PatoMovimiento.Movimiento.performed += OnMovimiento;
            controles.PatoMovimiento.Salto.performed += OnSalto;
        }
        controles.Enable();
    }
    private void OnDisable()
    {
        controles.Disable();
    }
    private void OnDestroy()
    {
        if (controles == null)
            return;
        controles.PatoMovimiento.Movimiento.performed -= OnMovimiento;
        controles.PatoMovimiento.Salto.performed -= OnSalto;
        controles.Dispose();
    }
    private void OnMovimiento(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        accionMovimiento = context.ReadValue<Vector2>();
    }
    // Se pide en Update (input) y se consume en FixedUpdate (física)
    private void OnSalto(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        saltoPedido = true;
    }
    public bool ConsumirSalto()
    {
        bool pedido = saltoPedido;
        saltoPedido = false;
        return pedido;
    }
    private void GestionarAccionMovimiento()
    {
        accionVertical = accionMovimiento.y;
        accionHorizontal = accionMovimiento.x;
        cantidadMovimiento = Mathf.Clamp01(Mathf.Abs(accionVertical)+Mathf.Abs(accionHorizontal));
        gestorAnimaciones.ActualizarValoresAnimacion(0, cantidadMovimiento);
    }
    public void GestionarAcciones()
    {
        GestionarAccionMovimiento();
        //GestorPausa();
    }
}
