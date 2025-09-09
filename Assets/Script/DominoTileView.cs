using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace Domino
{
    public class DominoTileView : MonoBehaviour
    {
        public Image sideAImage;
        public Image sideBImage;
        private DominoTile tile;

        public void Setup(DominoTile tileData, Sprite[] pipSprites, UnityAction onClick)
        {
            tile = tileData;

            if (tileData.A >= 0 && tileData.B >= 0)
            {
                sideAImage.sprite = pipSprites[tileData.A];
                sideBImage.sprite = pipSprites[tileData.B];
            }
            else
            {
                // peça virada (deck)
                sideAImage.sprite = pipSprites[0];
                sideBImage.sprite = pipSprites[0];
            }

            GetComponent<Button>().onClick.RemoveAllListeners();
            GetComponent<Button>().onClick.AddListener(onClick);
        }
    }
}