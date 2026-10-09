using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ColocarVegetacion : MonoBehaviour
{
    [System.Serializable]
    public class Paisajes {
        public string nombre;
        [Range(1.0f, 10.0f)]
        public float distancia;
        [Range(0.1f, 0.9f)]
        public float normal;
        [Range(0.0f, 2.0f)]
        public float desfase;
        [Range(0f, 1f)]
        public float densidad;
        [Range(0f, 1f)]
        public float variacion;
        [Range(0.05f, 0.20f)]
        public float clusters;
        [Range(-1.0f, 12.0f)]
        public float altitudMinima;
        [Range(0.5f, 50.0f)]
        public float rangoHabitat;
        public GameObject[] especies;
        [HideInInspector] public int iEspaciado;
        [HideInInspector] public int jEspaciado;
        [HideInInspector] public int iTotal;
        [HideInInspector] public int jTotal;
    }
    [SerializeField] public Paisajes arboles;
    [SerializeField] public Paisajes arbustos;
    [SerializeField] public Paisajes esparto;
    [SerializeField] public Paisajes rocas;
    [HideInInspector] public Mesh terreno;
    [HideInInspector] public float celda;
    [HideInInspector] public int iTamaño;
    [HideInInspector] public int jTamaño;
    // Copia cacheada: Mesh.vertices crea un array nuevo en cada acceso
    private Vector3[] vertices;

    public void Iniciar()
    {   
        vertices = terreno.vertices;
        CrearPaisaje(arboles);
        CrearPaisaje(arbustos);
        CrearPaisaje(esparto);
        CrearPaisaje(rocas);
    }
    private void CrearPaisaje(Paisajes paisajes)
    {   
        GameObject contenedorPaisaje = new GameObject(paisajes.nombre);
        contenedorPaisaje.transform.parent = gameObject.transform;
        if (paisajes.desfase > paisajes.distancia - 0.2f)
            paisajes.desfase = paisajes.distancia - 0.2f;
        if (paisajes.especies == null || paisajes.especies.Length == 0)
            return;
        paisajes.iEspaciado = Mathf.Max(1, (int) Mathf.Floor(paisajes.distancia / celda));
        paisajes.jEspaciado = Mathf.Max(1, (int) Mathf.Floor(paisajes.distancia / celda));
        paisajes.iTotal = (int) Mathf.Floor(iTamaño / paisajes.iEspaciado);
        paisajes.jTotal = (int) Mathf.Floor(jTamaño / paisajes.jEspaciado);

        for (int i = 1; i < paisajes.iTotal; i++)
        {
            for (int j = 1; j < paisajes.jTotal; j++)
            {
                // Las filas de la malla tienen jTamaño vértices
                int punto = jTamaño*i*paisajes.iEspaciado + j*paisajes.jEspaciado;
                Vector3 posicion = vertices[punto];
                bool dentroDelHabitat = posicion.y >= paisajes.altitudMinima
                    && posicion.y <= paisajes.altitudMinima + paisajes.rangoHabitat;
                if (dentroDelHabitat)
                {
                    float probabilidad = 0.5f * Mathf.PerlinNoise(posicion.x*paisajes.clusters, posicion.z*paisajes.clusters);
                    if (probabilidad + 0.5f * paisajes.densidad >= 0.5f)
                    {
                        GameObject especie = paisajes.especies[Random.Range(0, paisajes.especies.Length)];
                        Vector3 normal = CalcularNormal(punto, paisajes);
                        Quaternion rotacion = Quaternion.AngleAxis(Random.Range(0f, 360f), normal);
                        Vector3 insercion = new Vector3(posicion.x + Random.Range(-paisajes.desfase, paisajes.desfase), posicion.y, posicion.z + Random.Range(-paisajes.desfase, paisajes.desfase));
                        GameObject paisajePrefab = Instantiate(especie, insercion, rotacion);
                        paisajePrefab.transform.localScale = GetRandomScale(paisajePrefab.transform.localScale, paisajes.variacion);
                        paisajePrefab.transform.parent = contenedorPaisaje.transform;
                    }
                }
            }
        }
    }
    private Vector3 CalcularNormal(int i, Paisajes paisajes)
    {
        if (vertices.Length > i+jTamaño+1)
        {
            Vector3 a = vertices[i];
            Vector3 b = vertices[i+jTamaño];
            Vector3 c = vertices[i+jTamaño+1];
            float nX = (b.y - a.y) * (c.z - a.z) - (b.z - a.z) * (c.y - a.y);
            float nY = (b.z - a.z) * (c.x - a.x) - (b.x - a.x) * (c.z - a.z) ;
            float nZ = (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x) ;
            Vector3 normal = new Vector3(nX, nY, nZ);
            normal.Normalize();
            return normal*paisajes.normal + Vector3.up*(1-paisajes.normal);
        }
        return Vector3.up;
        
    }
    private Vector3 GetRandomScale(Vector3 localScale, float variacion)
    {
        float widthScaleFactor = Random.Range(1-variacion, 1 + variacion);
        float heightScaleFactor = Random.Range(1-variacion, 1 + variacion);
        //float heightScaleFactor = widthScaleFactor;

        return new Vector3(localScale.x * widthScaleFactor, localScale.y * heightScaleFactor, localScale.z * widthScaleFactor);
    }
    

}
