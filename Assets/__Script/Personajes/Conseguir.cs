using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Conseguir : MonoBehaviour
{
    //public GameObject jugador;
    public int contador;

    // Start is called before the first frame update
    void Start()
    {
        contador = 0;
    }

    // Update is called once per frame
    void Update()
    {
        contador += 1;
        if (contador > 500)
        {
            //Destroy(gameObject);
        }
    }

    void OnCollisionEnter(Collision other)
    {
        Destroy(gameObject);
        Debug.Log("destrusion");
    }
}
