using UnityEngine;

public sealed class GameAudio : MonoBehaviour
{
    public static GameAudio Instance { get; private set; }
    private AudioSource effects;
    private AudioSource music;
    private AudioClip victoryChime;
    private AudioClip backgroundTrack;
    private bool musicStarted;

    public static GameAudio GetOrCreate()
    {
        if (Instance != null) return Instance;
        GameObject audioObject = new GameObject("GameAudio");
        return audioObject.AddComponent<GameAudio>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        effects = gameObject.AddComponent<AudioSource>();
        effects.playOnAwake = false;
        effects.volume = .55f;
        music = gameObject.AddComponent<AudioSource>();
        music.playOnAwake = false;
        music.loop = true;
        music.volume = .18f;

        AudioClip click = Resources.Load<AudioClip>("Audio/UI_Click");
        if (click != null) effects.clip = click;
        backgroundTrack = Resources.Load<AudioClip>("Audio/BackgroundMusic");
        victoryChime = MakeVictoryChime();
    }

    public void StartBackgroundMusic()
    {
        if (musicStarted || backgroundTrack == null) return;
        music.clip = backgroundTrack;
        music.loop = true;
        music.Play();
        musicStarted = true;
    }

    public void PlayClick()
    {
        if (effects.clip != null) effects.PlayOneShot(effects.clip);
    }

    public void PlayVictory()
    {
        if (victoryChime != null) effects.PlayOneShot(victoryChime, .9f);
    }

    private static AudioClip MakeVictoryChime()
    {
        const int rate = 44100;
        const float duration = 1.45f;
        float[] samples = new float[(int)(rate * duration)];
        float[] notes = { 523.25f, 659.25f, 783.99f, 1046.5f };
        for (int i = 0; i < samples.Length; i++)
        {
            float time = i / (float)rate;
            float value = 0;
            for (int n = 0; n < notes.Length; n++)
            {
                float local = time - n * .22f;
                if (local < 0 || local > .9f) continue;
                float envelope = Mathf.Exp(-3.4f * local) * Mathf.Clamp01(local * 35f);
                value += Mathf.Sin(2f * Mathf.PI * notes[n] * local) * envelope / notes.Length;
            }
            samples[i] = value * .32f;
        }
        AudioClip clip = AudioClip.Create("Sudoku Victory Chime", samples.Length, 1, rate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
