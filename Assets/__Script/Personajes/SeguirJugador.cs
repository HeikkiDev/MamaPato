using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeguirJugador : MonoBehaviour
{
    [Header("SCRIPT DE FAUSTO")]
   
    public GameObject jugador;
    [Range(0.2f, 2.0f)]
    public float tol;
     [Header("VARIABLE DE HOLA")]
     [Range(0.05f, 0.3f)]
    public float acel;
     [Header("VARIABLE DE HOLA")]
     [Range(1.0f, 15.0f)]
    public float zoom;
    public float x, y, tx, ty;

    // LateUpdate: la cámara se mueve después de que el jugador se haya movido este frame
    void LateUpdate()
    {
        if (jugador == null)
            return;

        tx = jugador.transform.position[0];
        ty = jugador.transform.position[2];
        x = tx - zoom;
        y = ty - zoom;
        gameObject.transform.position = new Vector3 (x, jugador.transform.position[1] + zoom, y);
    }
}
