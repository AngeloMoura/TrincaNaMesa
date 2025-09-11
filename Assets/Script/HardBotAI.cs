using System.Collections.Generic;

namespace Domino
{
    public class HardBotAI
    {
        public DominoTile ChooseMove(List<DominoTile> botHand, List<DominoTile> board)
        {
            if (board.Count == 0)
                return botHand[0];

            int left = board[0].A;
            int right = board[board.Count - 1].B;

            DominoTile bestMove = null;
            int highestSum = -1;

            foreach (var tile in botHand)
            {
                bool canPlayLeft = tile.A == left || tile.B == left;
                bool canPlayRight = tile.A == right || tile.B == right;

                if (canPlayLeft || canPlayRight)
                {
                    int sum = tile.A + tile.B;
                    if (sum > highestSum)
                    {
                        highestSum = sum;
                        bestMove = tile;
                    }
                }
            }

            return bestMove;
        }

        public bool ChooseSide(DominoTile tile, List<DominoTile> board)
        {
            if (board.Count == 0) return true;

            int left = board[0].A;
            int right = board[board.Count - 1].B;

            if (tile.A == left || tile.B == left) return true;
            return false;
        }

        public bool CanPlay(DominoTile tile, List<DominoTile> board)
        {
            if (board.Count == 0) return true;

            int left = board[0].A;
            int right = board[board.Count - 1].B;

            return tile.A == left || tile.B == left || tile.A == right || tile.B == right;
        }
    }
}
