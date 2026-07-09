using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class MenuMusicRandom : MonoBehaviour
{
    [Header("Músicas do Menu")]
    public List<AudioClip> musicas = new List<AudioClip>();

    [Header("Configurações")]
    [Range(0f, 1f)] public float volumeMax = 1f;
    public float tempoFade = 2f;

    private AudioSource audioSource;
    private int musicaAnterior = -1;
    private bool trocandoMusica = false;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = false;
        audioSource.playOnAwake = false;
        audioSource.volume = 0f;
    }

    void Start()
    {
        if (musicas.Count == 0) return;
        TocarMusicaRandom();
    }

    void Update()
    {
        if (!audioSource.isPlaying && !trocandoMusica)
        {
            TocarMusicaRandom();
        }
    }

    void TocarMusicaRandom()
    {
        if (musicas.Count == 0) return;

        int index;

        do
        {
            index = Random.Range(0, musicas.Count);
        }
        while (index == musicaAnterior && musicas.Count > 1);

        musicaAnterior = index;

        StartCoroutine(TrocarMusica(musicas[index]));
    }

    IEnumerator TrocarMusica(AudioClip novaMusica)
    {
        trocandoMusica = true;

        // Fade Out
        float tempo = 0f;
        float volumeInicial = audioSource.volume;

        while (tempo < tempoFade && audioSource.isPlaying)
        {
            tempo += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(volumeInicial, 0f, tempo / tempoFade);
            yield return null;
        }

        audioSource.Stop();
        audioSource.clip = novaMusica;
        audioSource.Play();

        // Fade In
        tempo = 0f;
        audioSource.volume = 0f;

        while (tempo < tempoFade)
        {
            tempo += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, volumeMax, tempo / tempoFade);
            yield return null;
        }

        audioSource.volume = volumeMax;
        trocandoMusica = false;
    }

    public void PararMusica()
    {
        if (!trocandoMusica)
            StartCoroutine(PararComFade());
    }

    IEnumerator PararComFade()
    {
        trocandoMusica = true;

        float tempo = 0f;
        float volumeInicial = audioSource.volume;

        while (tempo < tempoFade)
        {
            tempo += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(volumeInicial, 0f, tempo / tempoFade);
            yield return null;
        }

        audioSource.Stop();
        trocandoMusica = false;
    }
}
