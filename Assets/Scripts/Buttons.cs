using UnityEngine;
using UnityEngine.UI;

public sealed class Buttons : MonoBehaviour
{
    private void Start()
    {
        GridMec grid = GetComponent<GridMec>();
        GameAudio audio = GameAudio.GetOrCreate();
        audio.StartBackgroundMusic();
        foreach (Button button in GetComponentsInChildren<Button>(true))
        {
            button.onClick.AddListener(audio.PlayClick);
            string name = button.name;
            if (name.StartsWith("Number_"))
            {
                int number = int.Parse(name.Substring(7));
                button.onClick.AddListener(() => grid.EnterNumber(number));
            }
            else if (name == "EraseButton") button.onClick.AddListener(grid.EraseSelected);
            else if (name == "ResetButton") button.onClick.AddListener(grid.ResetGrid);
        }
    }
}
