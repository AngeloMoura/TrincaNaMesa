using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Domino
{
    public class GameManager : MonoBehaviour
    {
        [Header("Prefabs & Areas")]
        public GameObject tilePrefab;
        public Transform playerHandArea;
        public Transform botHandArea;
        public Transform deckArea;
        public Transform boardArea;
        public Sprite[] pipSprites; // 0..6

        [Header("Back / UI")]
        public Sprite backSprite;
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
        private int initialHandSize = 7;
        private DominoTile pendingTile = null;

        void Start()
        {
            SetupGame();
        }

        void SetupGame()
        {
            ClearAll();
            GenerateDeck();
            ShuffleDeck();
            DistributeHands();
            RenderAll();
            sideChoicePanel.SetActive(false);
            resultPanel.SetActive(false);
        }

        void ClearAll()
        {
            foreach (Transform t in playerHandArea) Destroy(t.gameObject);
            foreach (Transform t in botHandArea) Destroy(t.gameObject);
            foreach (Transform t in deckArea) Destroy(t.gameObject);
            foreach (Transform t in boardArea) Destroy(t.gameObject);

            deck.Clear(); playerHand.Clear(); botHand.Clear(); board.Clear();
        }

        void GenerateDeck()
        {
            for (int i = 0; i <= 6; i++)
                for (int j = i; j <= 6; j++)
                    deck.Add(new DominoTile(i, j));
        }

        void ShuffleDeck()
        {
            for (int i = deck.Count - 1; i > 0; i--)
            {
                int r = Random.Range(0, i + 1);
                var tmp = deck[i];
                deck[i] = deck[r];
                deck[r] = tmp;
            }
        }

        void DistributeHands()
        {
            for (int i = 0; i < initialHandSize; i++)
            {
                playerHand.Add(deck[0]); deck.RemoveAt(0);
                botHand.Add(deck[0]); deck.RemoveAt(0);
            }
        }

        void RenderAll()
        {
            RenderBoard();
            RenderPlayerHand();
            RenderBotHand();
            RenderDeck();
        }

        void RenderPlayerHand()
        {
            foreach (Transform t in playerHandArea) Destroy(t.gameObject);

            foreach (var tile in playerHand)
            {
                var go = Instantiate(tilePrefab, playerHandArea);
                var view = go.GetComponent<DominoTileView>();
                DominoTile captured = tile;
                view.Setup(tile, pipSprites, () => OnPlayerTileClicked(captured), null, false);
            }
        }

        void RenderBotHand()
        {
            foreach (Transform t in botHandArea) Destroy(t.gameObject);

            foreach (var tile in botHand)
            {
                var go = Instantiate(tilePrefab, botHandArea);
                var view = go.GetComponent<DominoTileView>();
                // Bot = faceDown
                view.Setup(tile, pipSprites, null, backSprite, true);
            }
        }

        void RenderDeck()
        {
            foreach (Transform t in deckArea) Destroy(t.gameObject);
            if (deck.Count > 0)
            {
                var go = Instantiate(tilePrefab, deckArea);
                var view = go.GetComponent<DominoTileView>();
                view.Setup(new DominoTile(-1, -1), pipSprites, OnDeckClicked, backSprite, true);
            }
        }

        void OnPlayerTileClicked(DominoTile tile)
        {
            if (!playerTurn || resultPanel.activeSelf) return;

            if (board.Count == 0)
            {
                PlaceTile(tile, true, true);
                EndPlayerTurn();
                return;
            }

            int left = board[0].A;
            int right = board[board.Count - 1].B;
            bool canLeft = (tile.A == left || tile.B == left);
            bool canRight = (tile.A == right || tile.B == right);

            if (canLeft && canRight)
            {
                pendingTile = tile;
                sideChoicePanel.SetActive(true);
                leftButton.onClick.RemoveAllListeners();
                rightButton.onClick.RemoveAllListeners();
                leftButton.onClick.AddListener(() => { PlaceTile(pendingTile, true, true); sideChoicePanel.SetActive(false); pendingTile = null; EndPlayerTurn(); });
                rightButton.onClick.AddListener(() => { PlaceTile(pendingTile, false, true); sideChoicePanel.SetActive(false); pendingTile = null; EndPlayerTurn(); });
            }
            else if (canLeft)
            {
                PlaceTile(tile, true, true);
                EndPlayerTurn();
            }
            else if (canRight)
            {
                PlaceTile(tile, false, true);
                EndPlayerTurn();
            }
        }

        void OnDeckClicked()
        {
            if (!playerTurn || deck.Count == 0) return;

            var drawn = deck[0];
            deck.RemoveAt(0);
            playerHand.Add(drawn);
            RenderPlayerHand();
            RenderDeck();
        }

        void PlaceTile(DominoTile tile, bool placeLeft, bool isPlayer)
        {
            if (isPlayer) playerHand.Remove(tile);
            else botHand.Remove(tile);

            if (board.Count == 0)
                board.Add(tile);
            else if (placeLeft)
            {
                int leftVal = board[0].A;
                if (tile.A == leftVal) board.Insert(0, tile.Flipped());
                else board.Insert(0, tile);
            }
            else
            {
                int rightVal = board[board.Count - 1].B;
                if (tile.B == rightVal) board.Add(tile);
                else board.Add(tile.Flipped());
            }

            RenderBoard();
            RenderPlayerHand();
            RenderBotHand();
            RenderDeck();
            CheckEndConditions();
        }

        void RenderBoard()
        {
            foreach (Transform t in boardArea) Destroy(t.gameObject);
            foreach (var tile in board)
            {
                var go = Instantiate(tilePrefab, boardArea);
                var view = go.GetComponent<DominoTileView>();
                view.Setup(tile, pipSprites, null, null, false);
            }
        }

        void EndPlayerTurn()
        {
            playerTurn = false;
            StartCoroutine(BotRoutine());
        }

        IEnumerator BotRoutine()
        {
            yield return new WaitForSeconds(0.8f);

            var move = botAI.ChooseMove(botHand, board);
            if (move != null)
            {
                bool side = botAI.ChooseSide(move, board);
                PlaceTile(move, side, false);
            }
            else
            {
                if (deck.Count > 0)
                {
                    botHand.Add(deck[0]);
                    deck.RemoveAt(0);
                    RenderBotHand();
                    RenderDeck();
                }
            }

            playerTurn = true;
        }

        bool HasValidMove(List<DominoTile> hand)
        {
            if (hand.Count == 0) return false;
            if (board.Count == 0) return true;

            int left = board[0].A;
            int right = board[board.Count - 1].B;
            foreach (var t in hand)
                if (t.A == left || t.B == left || t.A == right || t.B == right) return true;
            return false;
        }

        void CheckEndConditions()
        {
            if (playerHand.Count == 0)
            {
                ShowResult("Vitória! Você ganhou!");
                return;
            }
            if (botHand.Count == 0)
            {
                ShowResult("Derrota! O bot venceu.");
                return;
            }

            bool playerCan = HasValidMove(playerHand);
            bool botCan = HasValidMove(botHand);
            if (!playerCan && !botCan && deck.Count == 0)
            {
                ShowResult("Empate! Nenhum movimento possível.");
            }
        }

        void ShowResult(string message)
        {
            RevealBotHand();
            resultText.text = message;
            resultPanel.SetActive(true);
        }

        void RevealBotHand()
        {
            foreach (Transform child in botHandArea)
            {
                var view = child.GetComponent<DominoTileView>();
                if (view != null) view.Reveal();
            }
        }
    }
}
