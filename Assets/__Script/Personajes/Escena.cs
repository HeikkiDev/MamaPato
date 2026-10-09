using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Escena : MonoBehaviour
{
    [Header("OBJETOS")]
    [Tooltip("Asigna el objeto eventos aquí :)")]
    public GameObject monedaPrefab;
    public TMP_Text UI_contador;
    public int GLOBAL_nivel;
    public int GLOBAL_contador;
    public int GLOBAL_tiempo;
    
    public bool SWITCH_ejecutar;
    public bool SWITCH_completo;
    public float sizeX, sizeZ;
    private GameObject contenedor;
    // Start is called before the first frame update
    void Start()
    {
        GLOBAL_contador = 0;
        GLOBAL_nivel = 1;
        GLOBAL_tiempo = 600000;
    }

    // Update is called once per frame
    void Update()
    {
        GLOBAL_tiempo -= 1;
        UI_contador.text = GLOBAL_nivel.ToString() +"__"+ GLOBAL_contador.ToString();
        if (SWITCH_ejecutar)
        {
            SetupScene();
            SWITCH_ejecutar = false;
        }
        if (SWITCH_completo)
        {
            reconstruir();
            SWITCH_completo = false;
        }
    }
    public void SetupScene()
    {
        if (contenedor != null)
        {
            Destroy(contenedor);
        }
        contenedor = new GameObject("Contenedor");
        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeZ; j++)
            {
                GameObject go = Instantiate(monedaPrefab, new Vector3(i*10, 1, j*10), Quaternion.identity);
                go.transform.parent = contenedor.transform;
                go.name = string.Format("Ground: ({0},{1})", i, j);
            }
        }
    }
    public void reconstruir ()
    {
        if (contenedor != null)
        {
            Destroy(contenedor);
        }
        GLOBAL_nivel += 1;
        GLOBAL_contador = 0;
        contenedor = new GameObject("Contenedor");
        for (int i = 0; i < GLOBAL_nivel; i++)
        {
            GameObject go = Instantiate(monedaPrefab, new Vector3(Random.Range(-5,5), 1, Random.Range(-5,5)), Quaternion.identity);
            go.transform.parent = contenedor.transform;
            go.name = string.Format("Moneda_{0}", i);
        }
    }
}
