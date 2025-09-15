using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class PlacedTile
{
    public DominoTile tile;
    public Vector2 position;
    public float rotation;

    public PlacedTile(DominoTile t, Vector2 pos, float rot)
    {
        tile = t;
        position = pos;
        rotation = rot;
    }
}

public class GameManager : MonoBehaviour
{
    [Header("Prefabs e Sprites")]
    public GameObject tilePrefab;
    public Sprite[] pipSprites;
    public Sprite backSprite;

    [Header("Áreas do Tabuleiro")]
    public Transform playerHandArea;
    public Transform botHandArea;
    public Transform deckArea;
    public Transform boardArea;

    [Header("UI")]
    public GameObject sideChoicePanel;
    public Button leftButton;
    public Button rightButton;
    public GameObject resultPanel;
    public TMP_Text resultText;
    public Image resultImage;
    public Sprite winSprite;
    public Sprite loseSprite;
    public Sprite drawSprite;

    private List<DominoTile> deck = new List<DominoTile>();
    private List<DominoTile> playerHand = new List<DominoTile>();
    private List<DominoTile> botHand = new List<DominoTile>();

    // Novo: tabuleiro snake
    private List<PlacedTile> placedBoard = new List<PlacedTile>();
    private Vector2 leftEnd = Vector2.zero;
    private Vector2 rightEnd = Vector2.zero;
    private Vector2 leftDir = Vector2.left;
    private Vector2 rightDir = Vector2.right;
    private float spacing = 80f;

    private DominoTile selectedTile;
    private bool awaitingSideChoice;

    void Start()
    {
        StartGame();
    }

    void StartGame()
    {
        resultPanel.SetActive(false);
        sideChoicePanel.SetActive(false);
        awaitingSideChoice = false;
        selectedTile = null;

        deck.Clear();
        playerHand.Clear();
        botHand.Clear();
        placedBoard.Clear();
        leftDir = Vector2.left;
        rightDir = Vector2.right;
        leftEnd = Vector2.zero;
        rightEnd = Vector2.zero;

        // Criar baralho
        for (int i = 0; i <= 6; i++)
        {
            for (int j = i; j <= 6; j++)
            {
                deck.Add(new DominoTile(i, j));
            }
        }

        Shuffle(deck);

        // Distribuir
        for (int k = 0; k < 7; k++)
        {
            playerHand.Add(DrawTile());
            botHand.Add(DrawTile());
        }

        RenderAll();

        leftButton.onClick.AddListener(() => OnSideChosen(true));
        rightButton.onClick.AddListener(() => OnSideChosen(false));
    }

    void Shuffle(List<DominoTile> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            DominoTile temp = list[i];
            int r = Random.Range(i, list.Count);
            list[i] = list[r];
            list[r] = temp;
        }
    }

    DominoTile DrawTile()
    {
        if (deck.Count == 0) return null;
        DominoTile t = deck[0];
        deck.RemoveAt(0);
        return t;
    }

    // ---------------- RENDER ----------------
    void RenderAll()
    {
        RenderPlayerHand();
        RenderBotHand();
        RenderDeck();
        RenderBoard();
    }

    void RenderPlayerHand()
    {
        foreach (Transform t in playerHandArea) Destroy(t.gameObject);
        foreach (var tile in playerHand)
        {
            var go = Instantiate(tilePrefab, playerHandArea);
            var view = go.GetComponent<DominoTileView>();
            view.Setup(tile, pipSprites,
                () => OnTileClicked(tile), // clique
                null,
                true);
        }
    }

    void RenderBotHand()
    {
        foreach (Transform t in botHandArea) Destroy(t.gameObject);
        foreach (var tile in botHand)
        {
            var go = Instantiate(tilePrefab, botHandArea);
            var view = go.GetComponent<DominoTileView>();
            view.Setup(tile, pipSprites, null, null, false, backSprite);
        }
    }

    void RenderDeck()
    {
        foreach (Transform t in deckArea) Destroy(t.gameObject);
        foreach (var tile in deck)
        {
            var go = Instantiate(tilePrefab, deckArea);
            var view = go.GetComponent<DominoTileView>();
            view.Setup(tile, pipSprites,
                () => OnDeckClicked(),
                null,
                false,
                backSprite);
        }
    }

    void RenderBoard()
    {
        foreach (Transform t in boardArea) Destroy(t.gameObject);

        foreach (var pt in placedBoard)
        {
            var go = Instantiate(tilePrefab, boardArea);
            var view = go.GetComponent<DominoTileView>();
            view.Setup(pt.tile, pipSprites, null, null, false);

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = pt.position;
            rt.localRotation = Quaternion.Euler(0, 0, pt.rotation);

            if (pt.tile.A == pt.tile.B)
                rt.localRotation = Quaternion.Euler(0, 0, pt.rotation + 90);
        }
    }

    // ---------------- CLIQUE ----------------
    void OnTileClicked(DominoTile tile)
    {
        if (awaitingSideChoice) return;
        if (CanPlay(tile))
        {
            if (MustChooseSide(tile))
            {
                selectedTile = tile;
                awaitingSideChoice = true;
                sideChoicePanel.SetActive(true);
            }
            else
            {
                bool left = Matches(leftEnd, tile);
                PlaceTile(tile, left, true);
                BotTurn();
            }
        }
    }

    void OnDeckClicked()
    {
        if (awaitingSideChoice) return;
        DominoTile drawn = DrawTile();
        if (drawn != null) playerHand.Add(drawn);
        RenderAll();
    }

    void OnSideChosen(bool left)
    {
        sideChoicePanel.SetActive(false);
        PlaceTile(selectedTile, left, true);
        selectedTile = null;
        awaitingSideChoice = false;
        BotTurn();
    }

    // ---------------- REGRAS ----------------
    bool CanPlay(DominoTile tile)
    {
        if (placedBoard.Count == 0) return true;
        int leftNum = placedBoard[0].tile.A;
        int rightNum = placedBoard[placedBoard.Count - 1].tile.B;
        return (tile.A == leftNum || tile.B == leftNum ||
                tile.A == rightNum || tile.B == rightNum);
    }

    bool MustChooseSide(DominoTile tile)
    {
        if (placedBoard.Count == 0) return false;
        int leftNum = placedBoard[0].tile.A;
        int rightNum = placedBoard[placedBoard.Count - 1].tile.B;
        bool canLeft = (tile.A == leftNum || tile.B == leftNum);
        bool canRight = (tile.A == rightNum || tile.B == rightNum);
        return canLeft && canRight;
    }

    bool Matches(Vector2 end, DominoTile tile)
    {
        // simplificado, pode-se expandir para checar valores corretos
        return true;
    }

    void PlaceTile(DominoTile tile, bool placeLeft, bool isPlayer)
    {
        if (isPlayer) playerHand.Remove(tile);
        else botHand.Remove(tile);

        if (placedBoard.Count == 0)
        {
            var first = new PlacedTile(tile, Vector2.zero, 0);
            placedBoard.Add(first);
            leftEnd = first.position;
            rightEnd = first.position;
        }
        else if (placeLeft)
        {
            Vector2 newPos = leftEnd + leftDir * spacing;
            float rot = DirToRotation(leftDir);
            placedBoard.Insert(0, new PlacedTile(tile, newPos, rot));
            leftEnd = newPos;

            if (Mathf.Abs(newPos.x) > 500) leftDir = Vector2.down;
            else if (Mathf.Abs(newPos.y) > 300) leftDir = Vector2.right;
        }
        else
        {
            Vector2 newPos = rightEnd + rightDir * spacing;
            float rot = DirToRotation(rightDir);
            placedBoard.Add(new PlacedTile(tile, newPos, rot));
            rightEnd = newPos;

            if (Mathf.Abs(newPos.x) > 500) rightDir = Vector2.down;
            else if (Mathf.Abs(newPos.y) > 300) rightDir = Vector2.left;
        }

        RenderAll();
        CheckEndConditions();
    }

    float DirToRotation(Vector2 dir)
    {
        if (dir == Vector2.right) return 0f;
        if (dir == Vector2.down) return -90f;
        if (dir == Vector2.left) return 180f;
        if (dir == Vector2.up) return 90f;
        return 0f;
    }

    // ---------------- BOT ----------------
    void BotTurn()
    {
        DominoTile playable = null;
        bool placeLeft = false;

        foreach (var t in botHand)
        {
            if (CanPlay(t))
            {
                if (MustChooseSide(t))
                    placeLeft = Random.value > 0.5f;
                else
                    placeLeft = Matches(leftEnd, t);
                playable = t;
                break;
            }
        }

        if (playable != null) PlaceTile(playable, placeLeft, false);
        else
        {
            DominoTile drawn = DrawTile();
            if (drawn != null) botHand.Add(drawn);
            RenderAll();
        }
    }

    // ---------------- FIM ----------------
    void CheckEndConditions()
    {
        if (playerHand.Count == 0) ShowResult("Você venceu!", winSprite);
        else if (botHand.Count == 0) ShowResult("Você perdeu!", loseSprite);
        else if (deck.Count == 0 && !HasPlayableMove())
            ShowResult("Empate!", drawSprite);
    }

    bool HasPlayableMove()
    {
        foreach (var t in playerHand) if (CanPlay(t)) return true;
        foreach (var t in botHand) if (CanPlay(t)) return true;
        return false;
    }

    void ShowResult(string msg, Sprite img)
    {
        resultPanel.SetActive(true);
        resultText.text = msg;
        resultImage.sprite = img;
    }
}
