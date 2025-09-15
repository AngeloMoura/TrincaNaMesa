using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject tilePrefab;

    [Header("Sprites")]
    public Sprite[] pipSprites;
    public Sprite backSprite;

    [Header("UI Areas")]
    public Transform playerHandArea;
    public Transform botHandArea;
    public Transform deckArea;
    public Transform boardArea;

    [Header("UI Panels")]
    public GameObject sideChoicePanel;
    public Button leftButton;
    public Button rightButton;

    public GameObject resultPanel;
    public Text resultText;

    private List<DominoTile> deck = new List<DominoTile>();
    private List<DominoTile> playerHand = new List<DominoTile>();
    private List<DominoTile> botHand = new List<DominoTile>();
    private List<DominoTile> board = new List<DominoTile>();

    private HardBotAI botAI = new HardBotAI();
    private DominoTile pendingTile;

    private void Start()
    {
        StartGame();
    }

    void StartGame()
    {
        GenerateDeck();
        ShuffleDeck();

        for (int i = 0; i < 7; i++)
        {
            playerHand.Add(deck[0]); deck.RemoveAt(0);
            botHand.Add(deck[0]); deck.RemoveAt(0);
        }

        RenderHands();
        RenderDeck();
    }

    void GenerateDeck()
    {
        deck.Clear();
        for (int i = 0; i <= 6; i++)
        {
            for (int j = i; j <= 6; j++)
            {
                deck.Add(new DominoTile(i, j));
            }
        }
    }

    void ShuffleDeck()
    {
        for (int i = 0; i < deck.Count; i++)
        {
            int rand = Random.Range(i, deck.Count);
            var temp = deck[i];
            deck[i] = deck[rand];
            deck[rand] = temp;
        }
    }

    void RenderHands()
    {
        foreach (Transform child in playerHandArea) Destroy(child.gameObject);
        foreach (Transform child in botHandArea) Destroy(child.gameObject);

        foreach (var tile in playerHand)
        {
            var go = Instantiate(tilePrefab, playerHandArea);
            var view = go.GetComponent<DominoTileView>();
            view.Setup(tile, pipSprites, () => OnPlayerClick(tile, view));
        }

        foreach (var tile in botHand)
        {
            var go = Instantiate(tilePrefab, botHandArea);
            var view = go.GetComponent<DominoTileView>();
            view.Setup(new DominoTile(-1, -1), new Sprite[] { backSprite }, null);
        }
    }

    void RenderDeck()
    {
        foreach (Transform child in deckArea) Destroy(child.gameObject);

        foreach (var tile in deck)
        {
            var go = Instantiate(tilePrefab, deckArea);
            var view = go.GetComponent<DominoTileView>();
            view.Setup(new DominoTile(-1, -1), new Sprite[] { backSprite }, () => OnDrawClick());
        }
    }

    void RenderBoard()
    {
        foreach (Transform child in boardArea) Destroy(child.gameObject);

        foreach (var tile in board)
        {
            var go = Instantiate(tilePrefab, boardArea);
            var view = go.GetComponent<DominoTileView>();
            view.Setup(tile, pipSprites, null);
        }
    }

    void OnPlayerClick(DominoTile tile, DominoTileView view)
    {
        if (board.Count == 0)
        {
            board.Add(tile);
            playerHand.Remove(tile);
            AfterPlayerTurn();
            return;
        }

        int left = board[0].SideA;
        int right = board[board.Count - 1].SideB;

        if (tile.Matches(left) && tile.Matches(right))
        {
            pendingTile = tile;
            sideChoicePanel.SetActive(true);

            leftButton.onClick.RemoveAllListeners();
            rightButton.onClick.RemoveAllListeners();

            leftButton.onClick.AddListener(() => PlaceTile(tile, true));
            rightButton.onClick.AddListener(() => PlaceTile(tile, false));
        }
        else if (tile.Matches(left))
        {
            PlaceTile(tile, true);
        }
        else if (tile.Matches(right))
        {
            PlaceTile(tile, false);
        }
    }

    void OnDrawClick()
    {
        if (deck.Count > 0)
        {
            playerHand.Add(deck[0]);
            deck.RemoveAt(0);
            RenderHands();
            RenderDeck();
        }
    }

    void PlaceTile(DominoTile tile, bool leftSide)
    {
        if (leftSide)
        {
            if (tile.SideB == board[0].SideA)
                board.Insert(0, tile);
            else
                board.Insert(0, tile.Flipped());
        }
        else
        {
            if (tile.SideA == board[board.Count - 1].SideB)
                board.Add(tile);
            else
                board.Add(tile.Flipped());
        }

        playerHand.Remove(tile);
        sideChoicePanel.SetActive(false);

        AfterPlayerTurn();
    }

    void AfterPlayerTurn()
    {
        RenderHands();
        RenderDeck();
        RenderBoard();

        if (playerHand.Count == 0) EndGame("Vitória!");
        else BotTurn();
    }

    void BotTurn()
    {
        if (board.Count == 0)
        {
            var tile = botHand[0];
            botHand.Remove(tile);
            board.Add(tile);
            AfterBotTurn();
            return;
        }

        int left = board[0].SideA;
        int right = board[board.Count - 1].SideB;

        DominoTile choice = botAI.ChooseMove(botHand, left, right);

        if (choice != null)
        {
            if (choice.SideB == left)
                board.Insert(0, choice);
            else if (choice.SideA == left)
                board.Insert(0, choice.Flipped());
            else if (choice.SideA == right)
                board.Add(choice);
            else if (choice.SideB == right)
                board.Add(choice.Flipped());

            botHand.Remove(choice);
            AfterBotTurn();
        }
        else if (deck.Count > 0)
        {
            botHand.Add(deck[0]);
            deck.RemoveAt(0);
            BotTurn();
        }
        else
        {
            CheckForDraw();
        }
    }

    void AfterBotTurn()
    {
        RenderHands();
        RenderDeck();
        RenderBoard();

        if (botHand.Count == 0) EndGame("Derrota!");
    }

    void CheckForDraw()
    {
        bool playerCanMove = false;
        bool botCanMove = false;

        int left = board[0].SideA;
        int right = board[board.Count - 1].SideB;

        foreach (var tile in playerHand)
            if (tile.Matches(left) || tile.Matches(right)) playerCanMove = true;

        foreach (var tile in botHand)
            if (tile.Matches(left) || tile.Matches(right)) botCanMove = true;

        if (!playerCanMove && !botCanMove)
            EndGame("Empate!");
    }

    void EndGame(string result)
    {
        resultPanel.SetActive(true);
        resultText.text = result;
    }
}
