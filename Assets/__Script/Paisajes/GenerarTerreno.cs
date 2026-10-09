using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ExtensionMethods {
 
    public static float Distorsion (this float valor, float limite) {
        if (valor >= limite)
            return 1;
        return 1 - (valor/limite-1)*(valor/limite-1);
    }
   
}
public class GenerarTerreno : MonoBehaviour
{   
    [System.Serializable]
    public class Ajustes {
        public float celda = 1;
        [Range(0.1f,0.45f)]
        public float lim0 = 0.4f;
        [Range(0.55f,0.90f)]
        public float lim1 = 0.6f;
        [Range(3.0f,5.0f)]
        public float escarpado0 = 3;
        [Range(0.015f,0.035f)]
        public float perlin0 = 0.025f;
        [Range(2.0f,4.0f)]
        public float escarpado1 = 3;
        [Range(0.04f,0.09f)]
        public float perlin1 = 0.05f;
        [Range(0.5f,2.0f)]
        public float escarpado2 = 2;
        [Range(0.1f,0.2f)]
        public float perlin2 = 0.15f;
        [Range(0.0f,0.25f)]
        public float escarpado3 = 0.2f;
        [Range(5,200)]
        public int iTamaño = 100;
        [Range(5,200)]
        public int jTamaño = 100;
        [Range(1,10)]
        public int lagoTamaño = 5;
        [Range(0.04f,0.15f)]
        public float lagoComplejidad = 0.02f;
        [Range(1.00f,5.0f)]
        public float lagoProfundidad = 5;
        [Range(1.00f,50.0f)]
        public float lagoPlaya = 5;
    }
    public Material material;
    private float desfase0;
    private float desfase1;
    private float desfase2;
    public Mesh mesh;
    Vector3[] vertices;
    Vector2[] uvs;
    int[] triangulos;
    int n = 0; 
    [SerializeField] public Ajustes ajustes = default;
    
    MeshFilter filtro;
    MeshRenderer renderizador;
    void Awake()
    {
        //mesh.name = "terrenillo";
        desfase0 = Random.Range(0f,300f);
        desfase1 = Random.Range(0f,300f);
        desfase2 = Random.Range(0f,300f);
        mesh = new Mesh();
        gameObject.AddComponent<MeshFilter>();
        gameObject.AddComponent<MeshRenderer>();
        filtro = GetComponent<MeshFilter>();
        renderizador = GetComponent<MeshRenderer>();
        CrearMalla();
        MapearMalla();
        ModificarMalla();
        CrearLagos();
        RefinarMalla();
        ActualizarMalla();
        renderizador.material= material;
        filtro.mesh = mesh;
        
        var meshCollider = GetComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;

        ProceduralGrassRenderer grass = GetComponent<ProceduralGrassRenderer>();
        grass.sourceMesh = mesh;
        grass.Iniciar();
        
        ColocarVegetacion vegetacion = GetComponent<ColocarVegetacion>();
        vegetacion.terreno = mesh;
        vegetacion.iTamaño = ajustes.iTamaño;
        vegetacion.jTamaño = ajustes.jTamaño;
        vegetacion.celda = ajustes.celda;
        vegetacion.Iniciar();
        
    }
    void CrearMalla()
    {   
        vertices = new Vector3[ajustes.iTamaño*ajustes.jTamaño];
        // Una celda (2 triángulos) por cada vértice que no está en la primera fila/columna
        triangulos = new int[6*(ajustes.iTamaño-1)*(ajustes.jTamaño-1)];

        for (int j = 0; j < ajustes.jTamaño; j++)
        {
            vertices[j] = new Vector3(j*ajustes.celda, 0, 0);
        }
        for (int i = 1; i < ajustes.iTamaño; i++)
        {
            vertices[ajustes.jTamaño*i] = new Vector3(0, 0, i*ajustes.celda);
            for (int j = 1; j < ajustes.jTamaño; j++)
            {
                //definir coordenadas del vértice
                vertices[ajustes.jTamaño*i+j] = new Vector3(j*ajustes.celda, 0, i*ajustes.celda);
                //definir los dos triángulos asociados al vértice
                triangulos[n+0] = ajustes.jTamaño*i+j;
                triangulos[n+1] = ajustes.jTamaño*i+j - ajustes.jTamaño;
                triangulos[n+2] = ajustes.jTamaño*i+j - ajustes.jTamaño - 1;
                triangulos[n+3] = ajustes.jTamaño*i+j;
                triangulos[n+4] = ajustes.jTamaño*i+j - ajustes.jTamaño - 1;
                triangulos[n+5] = ajustes.jTamaño*i+j           - 1;
                n += 6;
            }
        }
        
    }
    void ModificarMalla()
    {   
        for (int i = 0; i < ajustes.iTamaño*ajustes.jTamaño; i++)
        {   
            float altura = 1;
            altura += ajustes.escarpado2*Mathf.PerlinNoise(vertices[i].x*ajustes.perlin2, vertices[i].z*ajustes.perlin2 + desfase2);
            altura *= ajustes.escarpado0*(1-(Mathf.PerlinNoise(vertices[i].x*ajustes.perlin0, vertices[i].z*ajustes.perlin0 + desfase0)).Distorsion(ajustes.lim1)).Distorsion(ajustes.lim0);
            altura += ajustes.escarpado1*(0.5f - Mathf.PerlinNoise(vertices[i].x*ajustes.perlin1, vertices[i].z*ajustes.perlin1 + desfase1));
            vertices[i].y = altura;
        }
    }
    void CrearLagos()
    {
        for (int i = 0; i < ajustes.iTamaño*ajustes.jTamaño; i++)
        {   
            float altura = vertices[i].y;
            float silueta = Mathf.Sqrt(Mathf.PerlinNoise(vertices[i].x*ajustes.perlin0, vertices[i].z*ajustes.perlin0 + desfase0));
            float balsa = ajustes.lagoProfundidad*(Mathf.PerlinNoise(vertices[i].x*ajustes.lagoComplejidad , vertices[i].z*ajustes.lagoComplejidad)).Distorsion(ajustes.lim1);
            
            if (silueta < 0.0f)
                altura -= balsa;
            vertices[i].y = altura;
        }
    }
    void RefinarMalla()
    {
        for (int i = 0; i < ajustes.iTamaño*ajustes.jTamaño; i++)
        {  
            float altura = vertices[i].y;
            if (altura > 0.15f && altura < 2.00f)
                altura *= 0.5f;
            if (altura < -0.2f )
                altura *= ajustes.lagoProfundidad;
            altura -= Random.Range(0, Mathf.Abs(altura)*ajustes.escarpado3);
            vertices[i].y = altura;
        }
    }
    void MapearMalla()
    {
        uvs = new Vector2[vertices.Length];
        for (int i = 0; i < uvs.Length; i++)
        {
            uvs[i] = new Vector2(vertices[i].x, vertices[i].z);
        }
    }
    void ActualizarMalla()
    {   
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangulos;
        mesh.RecalculateNormals();
    }
}
