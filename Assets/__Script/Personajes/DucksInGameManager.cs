using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DucksInGameManager : MonoBehaviour
{
    public class Duck
    {
        public Guid UniqueId { get; } = Guid.NewGuid();
        public GameObject GameObject { get; set; }

        public bool IsMommy { get; set; }
        public bool IsInMainQueue { get; set; }
        public bool IsInOrphanQueue { get; set; }

        public bool IsHeadOfMainQueue { get; set; }
        public bool IsHeadOfOrphanQueue { get; set; }

        public Guid OrphanQueueUniqueId { get; set; } // Id de la orphan queue, para que sea más eficiente acceder a ella en el array de orphan queues

        public Duck DuckAheadOfYou { get; set; } // el pato delante de ti en la cola, al que tienes que seguir!
    }

    public class DucksQueue
    {
        public Guid UniqueId { get; } = Guid.NewGuid();

        public List<Duck> DucksList { get; set; } = new();

        public bool IsMainQueue { get; set; }
        public bool IsOrphanQueue { get; set; }
    }

    public DucksQueue MainQueue { get; set; }
    public List<DucksQueue> OrphanQueues { get; set; }

    public GameObject mommyDuckPrefab;
    public GameObject childDuckPrefab;
    public GameObject enemyPrefab;
    public GameObject mainCamera;

    [Tooltip("Máximo de patitos vivos a la vez; al alcanzarlo se pausa el spawn")]
    public int maxPatitos = 40;
    [Tooltip("Máximo de zorros vivos a la vez; al alcanzarlo se pausa el spawn")]
    public int maxZorros = 8;

    private const float SpawnIntervalInSeconds = 5f;
    private const float EnemySpawnIntervalInSeconds = 20f;
    private const float SpawnRaycastHeight = 100f;
    private const float SpawnRaycastDistance = 300f;

    // Terreno y agua están en la capa "Terrain"; así el spawn no cae encima de árboles o patos
    private int terrainMask;
    private readonly List<GameObject> enemies = new();

    public static DucksInGameManager Instance { get; private set; }
    public bool MatandoPatitos { get; set; }

    private void Awake()
    {
        Instance = this;
        terrainMask = LayerMask.GetMask("Terrain");
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    void Start()
    {
        MainQueue = new DucksQueue { IsMainQueue = true };
        OrphanQueues = new List<DucksQueue>();

        GenerateMommyDuck();
        GenerateInitialDuckQueue();

        StartChildrenSpawnCoroutine();
        StartEnemiesSpawnCoroutine();
    }

    private Vector3 RandomGroundPosition()
    {
        var position = new Vector3(UnityEngine.Random.Range(40, 60), SpawnRaycastHeight, UnityEngine.Random.Range(40, 60));

        if (Physics.Raycast(position, Vector3.down, out RaycastHit hit, SpawnRaycastDistance, terrainMask))
            position.y = hit.point.y;

        return position;
    }

    private void GenerateMommyDuck()
    {
        GameObject mommyDuck = Instantiate(mommyDuckPrefab, RandomGroundPosition(), Quaternion.identity);

        var cameraSeguir = mainCamera.GetComponent<SeguirJugador>();
        cameraSeguir.jugador = mommyDuck;

        var patoLocomocion = mommyDuck.GetComponent<PatoLocomocion>();
        patoLocomocion.cameraObject = mainCamera.transform;

        var mommy = new Duck
        {
            GameObject = mommyDuck,
            IsMommy = true,
            IsInMainQueue = true
        };

        MainQueue.DucksList.Add(mommy);
    }

    private void GenerateInitialDuckQueue()
    {
        int numberOfChildren = UnityEngine.Random.Range(3, 6);

        for (int i = 0; i < numberOfChildren; i++)
        {
            var position = MainQueue.DucksList.Last().GameObject.transform.localPosition;
            GameObject childDuckGO = Instantiate(childDuckPrefab, new Vector3(position.x, position.y, position.z - 0.5f), Quaternion.identity);

            AddChildToMainQueue(new Duck { GameObject = childDuckGO });
        }
    }

    private void GenerateChildDuckInRandomPosition()
    {
        GameObject childDuckGO = Instantiate(childDuckPrefab, RandomGroundPosition(), Quaternion.identity);

        var gestorPatito = childDuckGO.GetComponent<GestorPatito>();
        gestorPatito.jugador = null; // aun no tiene patito al que seguir
        gestorPatito.InicializarPatito();
    }

    private void GenerateEnemyInRandomPosition()
    {
        GameObject enemy = Instantiate(enemyPrefab, RandomGroundPosition(), Quaternion.identity);
        enemies.Add(enemy);

        var gestorZorro = enemy.GetComponent<GestorZorro>();
        gestorZorro.jugador = MainQueue.DucksList.First().GameObject;
        gestorZorro.InicializarZorro();
    }

    public void AddChildToMainQueue(Duck child)
    {
        var lastDuckInQueue = MainQueue.DucksList.Last();

        child.IsInMainQueue = true;
        child.IsInOrphanQueue = false;
        child.IsHeadOfOrphanQueue = false;
        child.IsHeadOfMainQueue = MainQueue.DucksList.Count == 1;
        child.OrphanQueueUniqueId = default;
        child.DuckAheadOfYou = lastDuckInQueue;

        var gestorPatito = child.GameObject.GetComponent<GestorPatito>();
        gestorPatito.jugador = lastDuckInQueue.GameObject; // el patito al que seguir
        gestorPatito.PatitoInfo = child;
        gestorPatito.InicializarPatito();

        MainQueue.DucksList.Add(child);

        AdjustCameraZoom(child);
    }

    public void AddChildToMainQueue(GameObject childGO)
    {
        AddChildToMainQueue(new Duck { GameObject = childGO });
    }

    public void AddToOrphanQueues(List<Duck> newOrphanQueue)
    {
        if (newOrphanQueue.Count == 0)
            return;

        var newOrphanList = new DucksQueue
        {
            IsOrphanQueue = true,
            DucksList = newOrphanQueue
        };

        foreach (var orphan in newOrphanQueue)
        {
            orphan.OrphanQueueUniqueId = newOrphanList.UniqueId;
            orphan.IsInMainQueue = false;
            orphan.IsInOrphanQueue = true;
            orphan.IsHeadOfMainQueue = false;
            orphan.IsHeadOfOrphanQueue = false;
        }

        MakeHeadOfOrphanQueue(newOrphanQueue.First());

        OrphanQueues.Add(newOrphanList);
    }

    // El primer patito de una cola huérfana se queda quieto esperando a que mamá pato lo encuentre
    private static void MakeHeadOfOrphanQueue(Duck head)
    {
        head.IsHeadOfOrphanQueue = true;
        head.DuckAheadOfYou = null;

        if (head.GameObject != null && head.GameObject.TryGetComponent(out GestorPatito gestorPatito))
            gestorPatito.jugador = null;
    }

    public void RemoveChildFromMainQueue(Duck child)
    {
        MatandoPatitos = true;
        StartCoroutine(ResetMatandoPatitos());

        int index = MainQueue.DucksList.FindIndex(x => x.UniqueId == child.UniqueId);
        if (index < 0)
            return;

        // Los patitos detrás del eliminado forman una nueva cola huérfana
        int indexToSplit = index + 1;
        var newOrphanList = MainQueue.DucksList.GetRange(indexToSplit, MainQueue.DucksList.Count - indexToSplit);
        MainQueue.DucksList.RemoveRange(index, MainQueue.DucksList.Count - index);

        if (newOrphanList.Count > 0)
        {
            Debug.Log("MAIN SPLIT IN ORPHAN !!!");
            AddToOrphanQueues(newOrphanList);
        }

        AdjustCameraZoom(MainQueue.DucksList.Last());
    }

    public void RemoveChildFromOrphanQueue(Duck child)
    {
        MatandoPatitos = true;
        StartCoroutine(ResetMatandoPatitos());

        var orphanQueue = OrphanQueues.FirstOrDefault(x => x.UniqueId == child.OrphanQueueUniqueId);
        if (orphanQueue == null)
            return;

        var orphanList = orphanQueue.DucksList;
        int index = orphanList.FindIndex(x => x.UniqueId == child.UniqueId);
        if (index < 0)
            return;

        if (index == 0)
        {
            orphanList.RemoveAt(0);
            if (orphanList.Count > 0)
                MakeHeadOfOrphanQueue(orphanList[0]);
        }
        else if (index == orphanList.Count - 1)
        {
            orphanList.RemoveAt(index);
        }
        else
        {
            // If duck being removed is in the middle of the OrphanQueue, it will split and generate a new OrphanQueue
            int indexToSplit = index + 1;
            var newOrphanList = orphanList.GetRange(indexToSplit, orphanList.Count - indexToSplit);
            orphanList.RemoveRange(index, orphanList.Count - index);

            AddToOrphanQueues(newOrphanList);
        }

        if (orphanList.Count == 0)
            OrphanQueues.Remove(orphanQueue);
    }

    public void RemoveOrphanQueueAndAddToMain(Guid orphanQueueUniqueId)
    {
        var orphanQueue = OrphanQueues.FirstOrDefault(x => x.UniqueId == orphanQueueUniqueId);
        if (orphanQueue == null)
            return;

        OrphanQueues.Remove(orphanQueue);
        foreach (var item in orphanQueue.DucksList)
        {
            AddChildToMainQueue(item);
        }
    }

    private void AdjustCameraZoom(Duck child)
    {
        // TODO: No funciona bien
        // Adjust camera
        //var cameraSeguir = mainCamera.GetComponent<SeguirJugador>();
        //var zoom = Vector3.Distance(child.GameObject.transform.position, MainQueue.DucksList.First().GameObject.transform.position);
        //float scaledValue = Clamp(zoom * 0.5f, 3, 6);
        //cameraSeguir.zoom = scaledValue;
    }

    public static float Clamp(float value, float min, float max)
    {
        return (value < min) ? min : (value > max) ? max : value;
    }

    private void StartChildrenSpawnCoroutine() => StartCoroutine(ChildrenSpawn());
    private IEnumerator ChildrenSpawn()
    {
        while (true) // infinite spawn
        {
            yield return new WaitForSeconds(SpawnIntervalInSeconds);

            if (GestorPatito.Activos.Count < maxPatitos)
                GenerateChildDuckInRandomPosition();
        }
    }

    private void StartEnemiesSpawnCoroutine() => StartCoroutine(EnemiesSpawn());
    private IEnumerator EnemiesSpawn()
    {
        while (true) // infinite spawn
        {
            enemies.RemoveAll(e => e == null);
            if (enemies.Count < maxZorros)
                GenerateEnemyInRandomPosition();

            yield return new WaitForSeconds(EnemySpawnIntervalInSeconds);
        }
    }

    private IEnumerator ResetMatandoPatitos()
    {
        yield return new WaitForSeconds(3);

        MatandoPatitos = false;
    }
}
