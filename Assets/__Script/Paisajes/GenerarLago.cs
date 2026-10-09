using UnityEngine;

public class GenerarLago : MonoBehaviour
{   
    [System.Serializable]
    public class Ajustes {
        public float celda = 1;
        [Range(5,200)]
        public int iTamaño = 100;
        [Range(5,200)]
        public int jTamaño = 100;
    }
    
    public Mesh mesh;
    Vector3[] vertices;
    Vector2[] uvs;
    int[] triangulos;
    int n = 0; 
    [SerializeField] public Ajustes ajustes = default;
    
    MeshFilter filtro;
    void Awake()
    {
        mesh = new Mesh();
        filtro = GetComponent<MeshFilter>();
        if (filtro == null)
            filtro = gameObject.AddComponent<MeshFilter>();
        CrearMalla();
        MapearMalla();
        ActualizarMalla();
        filtro.mesh = mesh;

        var meshCollider = GetComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;
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
