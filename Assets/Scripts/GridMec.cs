using TMPro;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public sealed class GridMec : MonoBehaviour
{
    private const int Size = 9;
    private readonly int[,] values = new int[Size, Size];
    private readonly int[,] solution = new int[Size, Size];
    private readonly bool[,] clues = new bool[Size, Size];
    private readonly Button[,] cells = new Button[Size, Size];
    private readonly TMP_Text[,] labels = new TMP_Text[Size, Size];
    private Grid_3x3 blockArt;
    private TMP_Text status;
    private GameObject victoryOverlay;
    private bool hasWon;
    private int selectedRow = -1, selectedColumn = -1;

    private static readonly Color Ink = new Color(.13f, .24f, .19f);
    private static readonly Color Empty = new Color(.98f, .96f, .88f, .26f);
    private static readonly Color Given = new Color(1f, .99f, .92f, .38f);
    private static readonly Color Related = new Color(.78f, .86f, .69f, .62f);
    private static readonly Color Selected = new Color(.98f, .79f, .37f, .84f);
    private static readonly Color Conflict = new Color(.91f, .48f, .39f, .88f);

    private void Start()
    {
        blockArt = GetComponent<Grid_3x3>();
        foreach (Button button in GetComponentsInChildren<Button>(true))
        {
            if (!button.name.StartsWith("Cell_")) continue;
            string[] part = button.name.Substring(5).Split('_');
            int row = int.Parse(part[0]), col = int.Parse(part[1]);
            cells[row, col] = button;
            labels[row, col] = button.GetComponentInChildren<TMP_Text>(true);
            int r = row, c = col;
            button.onClick.AddListener(() => SelectCell(r, c));
        }
        foreach (TMP_Text text in GetComponentsInChildren<TMP_Text>(true))
            if (text.name == "StatusLabel") status = text;

        CreateVictoryOverlay();

        for (int row = 0; row < Size; row++)
            for (int col = 0; col < Size; col++)
            {
                solution[row, col] = (row * 3 + row / 3 + col) % Size + 1;
                clues[row, col] = (row * 7 + col * 5) % 3 != 0;
                values[row, col] = clues[row, col] ? solution[row, col] : 0;
            }
        Refresh();
        Say("Tap a square, then choose a number");
    }

    private void SelectCell(int row, int col)
    {
        selectedRow = row; selectedColumn = col;
        Refresh();
        Say(clues[row, col] ? "That clue is fixed" : "Choose a number below");
    }

    public void EnterNumber(int number)
    {
        if (selectedRow < 0) { Say("Tap a square first"); return; }
        if (clues[selectedRow, selectedColumn]) { Say("That clue is fixed"); return; }
        values[selectedRow, selectedColumn] = number;
        Refresh();
        if (HasConflict(selectedRow, selectedColumn)) Say("That number conflicts in its row, column, or block");
        else if (IsSolved()) ShowVictory();
        else Say("Nice choice — keep going");
    }

    public void EraseSelected()
    {
        if (selectedRow < 0) { Say("Tap a square first"); return; }
        if (!clues[selectedRow, selectedColumn]) values[selectedRow, selectedColumn] = 0;
        Refresh(); Say("Square cleared");
    }

    public void ResetGrid()
    {
        for (int r = 0; r < Size; r++)
            for (int c = 0; c < Size; c++) values[r, c] = clues[r, c] ? solution[r, c] : 0;
        selectedRow = selectedColumn = -1;
        hasWon = false;
        if (victoryOverlay != null) victoryOverlay.SetActive(false);
        if (blockArt != null) blockArt.ResetBlocks();
        Refresh(); Say("Board reset — take your time");
    }

    private void Refresh()
    {
        for (int r = 0; r < Size; r++)
            for (int c = 0; c < Size; c++)
            {
                if (cells[r, c] == null) continue;
                labels[r, c].text = values[r, c] == 0 ? "" : values[r, c].ToString();
                labels[r, c].color = clues[r, c] ? Ink : new Color(.12f, .36f, .30f);
                Image image = cells[r, c].GetComponent<Image>();
                image.color = HasConflict(r, c) ? Conflict :
                    r == selectedRow && c == selectedColumn ? Selected :
                    r == selectedRow || c == selectedColumn || SameBlock(r, c, selectedRow, selectedColumn) ? Related :
                    clues[r, c] ? Given : Empty;
            }
        if (blockArt != null) blockArt.RefreshBlocks(values, solution);
    }

    private bool HasConflict(int row, int col)
    {
        int value = values[row, col];
        if (value == 0) return false;
        for (int i = 0; i < Size; i++)
            if ((i != col && values[row, i] == value) || (i != row && values[i, col] == value)) return true;
        for (int r = row / 3 * 3; r < row / 3 * 3 + 3; r++)
            for (int c = col / 3 * 3; c < col / 3 * 3 + 3; c++)
                if ((r != row || c != col) && values[r, c] == value) return true;
        return false;
    }

    private bool IsSolved()
    {
        for (int r = 0; r < Size; r++)
            for (int c = 0; c < Size; c++) if (values[r, c] != solution[r, c]) return false;
        return true;
    }

    private static bool SameBlock(int r, int c, int sr, int sc) => sr >= 0 && r / 3 == sr / 3 && c / 3 == sc / 3;
    private void Say(string message) { if (status != null) status.text = message; }

    private void ShowVictory()
    {
        if (hasWon) return;
        hasWon = true;
        Say("A lovely little forest restored!");
        if (GameAudio.Instance != null) GameAudio.Instance.PlayVictory();
        victoryOverlay.SetActive(true);
        StartCoroutine(Confetti());
    }

    private void CreateVictoryOverlay()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;

        victoryOverlay = new GameObject("VictoryOverlay", typeof(RectTransform), typeof(Image));
        victoryOverlay.transform.SetParent(canvas.transform, false);
        RectTransform overlayRect = victoryOverlay.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = overlayRect.offsetMax = Vector2.zero;
        Image shade = victoryOverlay.GetComponent<Image>();
        shade.color = new Color(.035f, .09f, .07f, .82f);

        Color[] colors = { new Color(.98f,.70f,.24f), new Color(.37f,.72f,.48f), new Color(.96f,.90f,.62f), new Color(.47f,.70f,.83f) };
        for (int i = 0; i < 38; i++)
        {
            GameObject bit = new GameObject("Confetti", typeof(RectTransform), typeof(Image));
            bit.transform.SetParent(victoryOverlay.transform, false);
            RectTransform rect = bit.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(Random.Range(10f, 20f), Random.Range(16f, 30f));
            rect.anchoredPosition = new Vector2(Random.Range(-500f, 500f), Random.Range(-850f, 950f));
            bit.GetComponent<Image>().color = colors[i % colors.Length];
            bit.GetComponent<Image>().raycastTarget = false;
        }

        GameObject card = new GameObject("VictoryCard", typeof(RectTransform), typeof(Image));
        card.transform.SetParent(victoryOverlay.transform, false);
        RectTransform cardRect = card.GetComponent<RectTransform>();
        cardRect.anchorMin = cardRect.anchorMax = new Vector2(.5f, .5f);
        cardRect.sizeDelta = new Vector2(720, 500);
        card.GetComponent<Image>().color = new Color(.96f, .92f, .76f);
        CreateText(card.transform, "WELL DONE!", new Vector2(0, 115), 74, Ink);
        CreateText(card.transform, "The forest is back in bloom!", new Vector2(0, 20), 40, Ink);

        GameObject again = new GameObject("PlayAgainButton", typeof(RectTransform), typeof(Image), typeof(Button));
        again.transform.SetParent(card.transform, false);
        RectTransform againRect = again.GetComponent<RectTransform>();
        againRect.anchorMin = againRect.anchorMax = new Vector2(.5f, .5f);
        againRect.sizeDelta = new Vector2(390, 96);
        againRect.anchoredPosition = new Vector2(0, -145);
        again.GetComponent<Image>().color = new Color(.27f, .48f, .34f);
        CreateText(again.transform, "PLAY AGAIN", Vector2.zero, 38, Color.white);
        again.GetComponent<Button>().onClick.AddListener(() =>
        {
            ResetGrid();
        });
        victoryOverlay.SetActive(false);
    }

    private static void CreateText(Transform parent, string message, Vector2 position, float size, Color color)
    {
        GameObject labelObject = new GameObject(message, typeof(RectTransform), typeof(TextMeshProUGUI));
        labelObject.transform.SetParent(parent, false);
        RectTransform rect = labelObject.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
        rect.sizeDelta = new Vector2(650, 100);
        rect.anchoredPosition = position;
        TextMeshProUGUI label = labelObject.GetComponent<TextMeshProUGUI>();
        label.text = message;
        label.fontSize = size;
        label.color = color;
        label.fontStyle = FontStyles.Bold;
        label.alignment = TextAlignmentOptions.Center;
        label.raycastTarget = false;
    }

    private IEnumerator Confetti()
    {
        RectTransform[] pieces = new RectTransform[victoryOverlay.transform.childCount - 1];
        for (int i = 0; i < pieces.Length; i++) pieces[i] = victoryOverlay.transform.GetChild(i).GetComponent<RectTransform>();
        float elapsed = 0;
        while (elapsed < 5f && victoryOverlay.activeSelf)
        {
            elapsed += Time.unscaledDeltaTime;
            foreach (RectTransform piece in pieces)
            {
                if (piece == null) continue;
                piece.anchoredPosition += Vector2.down * (160f * Time.unscaledDeltaTime);
                piece.Rotate(0, 0, 45f * Time.unscaledDeltaTime);
                if (piece.anchoredPosition.y < -1000) piece.anchoredPosition = new Vector2(piece.anchoredPosition.x, 1050);
            }
            yield return null;
        }
    }
}
