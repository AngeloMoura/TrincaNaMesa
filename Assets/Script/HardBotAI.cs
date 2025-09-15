using System.Collections.Generic;

public class HardBotAI
{
    // Escolhe qual peça o bot vai jogar
    public DominoTile ChooseMove(List<DominoTile> hand, List<DominoTile> board)
    {
        if (hand.Count == 0) return null;
        if (board.Count == 0) return hand[0]; // primeira jogada, qualquer peça serve

        int left = board[0].A;
        int right = board[board.Count - 1].B;

        DominoTile bestTile = null;
        int bestScore = -1;

        foreach (var tile in hand)
        {
            bool matchLeft = (tile.A == left || tile.B == left);
            bool matchRight = (tile.A == right || tile.B == right);

            if (matchLeft || matchRight)
            {
                // pontuação baseada na soma dos valores da peça
                int score = tile.A + tile.B;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestTile = tile;
                }
            }
        }

        return bestTile;
    }

    // Escolhe em qual lado jogar (esquerda ou direita)
    public bool ChooseSide(DominoTile tile, List<DominoTile> board)
    {
        if (board.Count == 0) return true;

        int left = board[0].A;
        int right = board[board.Count - 1].B;

        bool matchLeft = (tile.A == left || tile.B == left);
        bool matchRight = (tile.A == right || tile.B == right);

        if (matchLeft && matchRight)
        {
            // regra simples: se a soma da peça é par → esquerda, ímpar → direita
            return (tile.A + tile.B) % 2 == 0;
        }
        else if (matchLeft)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}