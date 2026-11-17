using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public sealed class Grid_3x3 : MonoBehaviour
{
    private readonly Image[] blocks = new Image[9];
    private readonly Material[] gray = new Material[9];
    private readonly bool[] revealed = new bool[9];
    private readonly Outline[] glow = new Outline[9];

    private void Start()
    {
        foreach (Image image in GetComponentsInChildren<Image>(true))
            if (image.name.StartsWith("Region_"))
            {
                int index = int.Parse(image.name.Substring(7));
                blocks[index] = image;
                gray[index] = image.material;
                glow[index] = image.GetComponent<Outline>();
                if (glow[index] != null) glow[index].enabled = false;
            }
    }

    public void RefreshBlocks(int[,] board, int[,] answer)
    {
        for (int block = 0; block < 9; block++)
        {
            if (revealed[block] || blocks[block] == null) continue;
            int startRow = block / 3 * 3, startCol = block % 3 * 3;
            bool solved = true;
            for (int r = startRow; r < startRow + 3; r++)
                for (int c = startCol; c < startCol + 3; c++)
                    if (board[r, c] != answer[r, c]) solved = false;
            if (!solved) continue;

            revealed[block] = true;
            blocks[block].material = null;
            blocks[block].color = new Color(1f, 1f, 1f, .7f);
            if (glow[block] != null) glow[block].enabled = true;
            StartCoroutine(Pulse(blocks[block].rectTransform));
        }
    }

    public void ResetBlocks()
    {
        for (int i = 0; i < 9; i++)
        {
            revealed[i] = false;
            if (blocks[i] == null) continue;
            blocks[i].material = gray[i];
            blocks[i].color = Color.white;
            blocks[i].rectTransform.localScale = Vector3.one;
            if (glow[i] != null) glow[i].enabled = false;
        }
    }

    private static IEnumerator Pulse(RectTransform target)
    {
        const float duration = .42f;
        float time = 0;
        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            float bump = Mathf.Sin(time / duration * Mathf.PI) * .055f;
            target.localScale = Vector3.one * (1f + bump);
            yield return null;
        }
        target.localScale = Vector3.one;
    }
}
