using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class UserInterface : MonoBehaviour
{
    public AudioMixer audioMixer;
    public void SalirDelJuego()
    {
        Application.Quit();
    }
    public void ConfigurarVolumen(float volumen)
    {
        audioMixer.SetFloat("AudioGlobal", volumen);
    }
}
