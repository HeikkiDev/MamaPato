using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Networking;

[RequireComponent(typeof(AudioSource))]
public class Musica : MonoBehaviour
{
    AudioSource audioData;
    public AudioClip[] clips;
    public int iniciar = 0;
    public AudioClip[] usablesArmonia;
    public AudioClip[] usablesMelodia;
    public string carpeta = "__Audio/notas";
    [System.Serializable]
    public class Tiempo
    {
        public int pulso = 1;
        public int parte = 1;
        [Range(8,16)]
        public int pulsoMax = 8;
        public int parteMax = 4;
        [Range(45,120)]
        public int bpm = 65;
    }
    [System.Serializable]
    public class Escala
    {
        [Range(0,11)]
        public int tonica = 0;
        public int complejidad = 14;
    }
    [SerializeField] public Tiempo tiempo = default;
    [SerializeField] public Escala escala = default;
    public AudioSource melodia;
    public AudioSource armonia0;
    public AudioSource armonia1;
    public AudioSource armonia2;
    public AudioSource armonia3;
    void Start()
    {
        LoadAudioClips();
        if (escala.complejidad % 2 > 0)
            escala.complejidad -= 1;
    }
    void Update()
    {
        if (iniciar == 1)
        {
            iniciar = 2;
            //DefinirEscala();
            //StartCoroutine(Loop());
        }
    }
    IEnumerator Loop()
    {
        while (true)
        {
            Bucle();
            Siguiente();
            yield return new WaitForSeconds(30f / tiempo.bpm);
        }
    }

    void Bucle()
    {
        Melodia();
        Armonia();
    }
    void Siguiente()
    {
        if (tiempo.pulso < tiempo.pulsoMax)
            tiempo.pulso += 1;
        else
        {
            tiempo.pulso = 1;
            if (tiempo.parte < tiempo.parteMax)
                tiempo.parte += 1;
            else
                tiempo.parte = 1;
        }
    }
    void Melodia()
    {
        if (Random.Range(0, 100) < 25)
            return;
        int nota = Random.Range(0, 14);
        melodia.clip = usablesMelodia[nota];
        melodia.Play(0);
    }
    void Armonia()
    {
        if (tiempo.pulso != 1)
            return;
        if (tiempo.parte == 1)
        {
            armonia1.clip = usablesArmonia[0];
            armonia2.clip = usablesArmonia[3];
            armonia3.clip = usablesArmonia[5];
            return;
        }
        int nota0 = Random.Range(0, escala.complejidad/2);
        int nota1 = Random.Range(nota0 + 1, nota0 + 3);
        int nota2 = Random.Range(nota1 + 1, nota1 + 3);
        
        armonia1.clip = usablesArmonia[nota0];
        armonia2.clip = usablesArmonia[nota1];
        armonia3.clip = usablesArmonia[nota2];
        armonia1.Play(0);
        armonia2.Play(0);
        armonia3.Play(0);
    }
    void DefinirEscala()
    {
        usablesArmonia[0]  = clips[escala.tonica + 0];
        usablesArmonia[1]  = clips[escala.tonica + 2];
        usablesArmonia[2]  = clips[escala.tonica + 4];
        usablesArmonia[3]  = clips[escala.tonica + 5];
        usablesArmonia[4]  = clips[escala.tonica + 7];
        usablesArmonia[5]  = clips[escala.tonica + 9];
        usablesArmonia[6]  = clips[escala.tonica + 11];
        
        usablesArmonia[7]  = clips[escala.tonica + 0   +12];
        usablesArmonia[8]  = clips[escala.tonica + 2   +12];
        usablesArmonia[9]  = clips[escala.tonica + 4   +12];
        usablesArmonia[10] = clips[escala.tonica + 5   +12];
        usablesArmonia[11] = clips[escala.tonica + 7   +12];
        usablesArmonia[12] = clips[escala.tonica + 9   +12];
        usablesArmonia[13] = clips[escala.tonica + 11  +12];

        usablesMelodia[0]  = clips[escala.tonica + 0   +24];
        usablesMelodia[1]  = clips[escala.tonica + 2   +24];
        usablesMelodia[2]  = clips[escala.tonica + 4   +24];
        usablesMelodia[3]  = clips[escala.tonica + 5   +24];
        usablesMelodia[4]  = clips[escala.tonica + 7   +24];
        usablesMelodia[5]  = clips[escala.tonica + 9   +24];
        usablesMelodia[6]  = clips[escala.tonica + 11  +24];
        
        usablesMelodia[7]  = clips[escala.tonica + 0   +36];
        usablesMelodia[8]  = clips[escala.tonica + 2   +36];
        usablesMelodia[9]  = clips[escala.tonica + 4   +36];
        usablesMelodia[10] = clips[escala.tonica + 5   +36];
        usablesMelodia[11] = clips[escala.tonica + 7   +36];
        usablesMelodia[12] = clips[escala.tonica + 9   +36];
        usablesMelodia[13] = clips[escala.tonica + 11  +36];
    }
    void LoadAudioClips()
    {
        clips = Resources.LoadAll<AudioClip>(carpeta);
    }
}
