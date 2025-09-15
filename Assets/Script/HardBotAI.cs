using System.Collections.Generic;
using UnityEngine;

public class HardBotAI
{
    public DominoTile ChooseMove(List<DominoTile> hand, int leftValue, int rightValue)
    {
        DominoTile bestTile = null;
        int bestScore = -1;

        foreach (var tile in hand)
        {
            if (tile.Matches(leftValue) || tile.Matches(rightValue))
            {
                int score = CountOccurrences(hand, tile.SideA) + CountOccurrences(hand, tile.SideB);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestTile = tile;
                }
            }
        }

        return bestTile;
    }

    private int CountOccurrences(List<DominoTile> hand, int value)
    {
        int count = 0;
        foreach (var t in hand)
        {
            if (t.SideA == value || t.SideB == value) count++;
        }
        return count;
    }
}
