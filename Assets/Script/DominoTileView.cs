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
        private Sprite[] pipSprites;
        private Sprite backSprite;
        private Button button;
        private bool faceDown = false;

        void Awake()
        {
            button = GetComponent<Button>();
        }

        // Setup da peça (face-up ou face-down)
        public void Setup(DominoTile tileData, Sprite[] pipSprites, UnityAction onClick, Sprite backSprite = null, bool startFaceDown = false)
        {
            tile = tileData;
            this.pipSprites = pipSprites;
            this.backSprite = backSprite;
            faceDown = startFaceDown;

            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                if (onClick != null)
                {
                    button.onClick.AddListener(onClick);
                    button.interactable = true;
                }
                else
                {
                    button.interactable = false;
                }
            }

            ApplyVisual();
        }

        private void ApplyVisual()
        {
            if (faceDown && backSprite != null)
            {
                sideAImage.sprite = backSprite;
                sideBImage.sprite = backSprite;
            }
            else if (pipSprites != null && tile != null && tile.A >= 0 && tile.B >= 0)
            {
                sideAImage.sprite = pipSprites[tile.A];
                sideBImage.sprite = pipSprites[tile.B];
            }
            else
            {
                sideAImage.sprite = backSprite;
                sideBImage.sprite = backSprite;
            }
        }

        public void Reveal()
        {
            faceDown = false;
            ApplyVisual();
        }

        public void SetFaceDown(bool down)
        {
            faceDown = down;
            ApplyVisual();
        }

        public DominoTile GetTile() => tile;
    }
}
