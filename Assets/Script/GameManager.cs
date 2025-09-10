using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace Domino
{
    public class GameManager : MonoBehaviour
    {
        [Header("UI References")]
        public Transform playerHandArea;
        public Transform botHandArea;
        public Transform deckArea;
        public Transform boardArea;
        public GameObject tilePrefab;
        public Sprite[] pipSprites;

        [Header("Panels & Buttons")]
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
        private bool playerTurn = true;
        private DominoTile selectedTile = null;
        private int initialHandSize = 7;

        void Start()
        {
            SetupGame();
        }

        void SetupGame()
        {
            ClearBoard();
            GenerateDeck();
            ShuffleDeck();
            DistributeHands();
            RenderHands();
            RenderDeck();
        }

        void ClearBoard()
        {
            foreach (Transform child in playerHandArea) Destroy(child.gameObject);
            foreach (Transform child in botHandArea) Destroy(child.gameObject);
            foreach (Transform child in deckArea) Destroy(child.gameObject);
            foreach (Transform child in boardArea) Destroy(child.gameObject);

            playerHand.Clear();
            botHand.Clear();
            deck.Clear();
            board.Clear();
            selectedTile = null;
        }

        void GenerateDeck()
        {
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
            for (int i = deck.Count - 1; i > 0; i--)
            {
                int randomIndex = Random.Range(0, i + 1);
                DominoTile temp = deck[i];
                deck[i] = deck[randomIndex];
                deck[randomIndex] = temp;
            }
        }

        void DistributeHands()
        {
            for (int i = 0; i < initialHandSize; i++)
            {
                playerHand.Add(deck[0]);
                deck.RemoveAt(0);

                botHand.Add(deck[0]);
                deck.RemoveAt(0);
            }
        }

        void RenderHands()
        {
            foreach (var tile in playerHand)
            {
                var obj = Instantiate(tilePrefab, playerHandArea);
                obj.GetComponent<DominoTileView>().Setup(tile, pipSprites, () => OnPlayerSelectTile(tile));
            }

            foreach (var tile in botHand)
            {
                var obj = Instantiate(tilePrefab, botHandArea);
                // Bot não precisa ser clicável
                obj.GetComponent<DominoTileView>().Setup(tile, pipSprites, null);
            }
        }

        void RenderDeck()
        {
            foreach (var tile in deck)
            {
                var obj = Instantiate(tilePrefab, deckArea);
                obj.GetComponent<DominoTileView>().Setup(
                    new DominoTile(-1, -1), // peça virada
                    pipSprites,
                    () => OnPlayerDrawTile(tile)
                );
            }
        }

        void OnPlayerSelectTile(DominoTile tile)
        {
            selectedTile = tile;

            if (board.Count == 0)
            {
                PlaceTile(tile, true);
                EndPlayerTurn();
                return;
            }

            int left = board[0].A;
            int right = board[board.Count - 1].B;

            bool canLeft = tile.A == left || tile.B == left;
            bool canRight = tile.A == right || tile.B == right;

            if (canLeft && canRight)
            {
                sideChoicePanel.SetActive(true);
                leftButton.onClick.RemoveAllListeners();
                rightButton.onClick.RemoveAllListeners();

                leftButton.onClick.AddListener(() =>
                {
                    PlaceTile(tile, true);
                    sideChoicePanel.SetActive(false);
                    EndPlayerTurn();
                });

                rightButton.onClick.AddListener(() =>
                {
                    PlaceTile(tile, false);
                    sideChoicePanel.SetActive(false);
                    EndPlayerTurn();
                });
            }
            else if (canLeft)
            {
                PlaceTile(tile, true);
                EndPlayerTurn();
            }
            else if (canRight)
            {
                PlaceTile(tile, false);
                EndPlayerTurn();
            }
        }

        void OnPlayerDrawTile(DominoTile tile)
        {
            if (playerHand.Contains(tile) || botHand.Contains(tile)) return;

            playerHand.Add(tile);
            deck.Remove(tile);

            var obj = Instantiate(tilePrefab, playerHandArea);
            obj.GetComponent<DominoTileView>().Setup(tile, pipSprites, () => OnPlayerSelectTile(tile));
        }

        void PlaceTile(DominoTile tile, bool placeLeft)
        {
            playerHand.Remove(tile);
            botHand.Remove(tile);

            if (board.Count == 0)
            {
                board.Add(tile);
            }
            else
            {
                if (placeLeft)
                    board.Insert(0, tile);
                else
                    board.Add(tile);
            }

            RenderBoard();
            CheckGameEnd();
        }

        void RenderBoard()
        {
            foreach (Transform child in boardArea) Destroy(child.gameObject);

            foreach (var tile in board)
            {
                var obj = Instantiate(tilePrefab, boardArea);
                obj.GetComponent<DominoTileView>().Setup(tile, pipSprites, null);
            }
        }

        void EndPlayerTurn()
        {
            playerTurn = false;
            StartCoroutine(BotPlay());
        }

        System.Collections.IEnumerator BotPlay()
        {
            yield return new WaitForSeconds(1f);

            var move = botAI.ChooseMove(botHand, board);

            if (move != null)
            {
                bool side = botAI.ChooseSide(move, board);
                PlaceTile(move, side);
            }
            else
            {
                // Bot compra se houver peças
                if (deck.Count > 0)
                {
                    botHand.Add(deck[0]);
                    deck.RemoveAt(0);
                    RenderHands();
                }
                else
                {
                    ShowResult("Empate!");
                }
            }

            playerTurn = true;
        }

        void CheckGameEnd()
        {
            if (playerHand.Count == 0)
            {
                ShowResult("Vitória!");
            }
            else if (botHand.Count == 0)
            {
                ShowResult("Derrota!");
            }
            else if (deck.Count == 0 && !HasValidMove(playerHand) && !HasValidMove(botHand))
            {
                ShowResult("Empate!");
            }
        }

        bool HasValidMove(List<DominoTile> hand)
        {
            if (board.Count == 0) return true;
            int left = board[0].A;
            int right = board[board.Count - 1].B;

            foreach (var tile in hand)
            {
                if (tile.A == left || tile.B == left || tile.A == right || tile.B == right)
                    return true;
            }
            return false;
        }

        void ShowResult(string message)
        {
            resultPanel.SetActive(true);
            resultText.text = message;
        }
    }
}