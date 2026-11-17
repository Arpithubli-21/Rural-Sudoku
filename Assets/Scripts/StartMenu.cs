using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class StartMenu : MonoBehaviour
{
    private void Start()
    {
        GameAudio audio = GameAudio.GetOrCreate();
        audio.StartBackgroundMusic();
        Button start = GameObject.Find("StartButton")?.GetComponent<Button>();
        if (start == null) return;
        start.onClick.AddListener(audio.PlayClick);
        start.onClick.AddListener(() => SceneManager.LoadScene("Main Game"));
    }
}
