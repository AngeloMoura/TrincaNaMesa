using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Domino;

public class DominoTileView : MonoBehaviour
{
    public Image sideAImage;
    public Image sideBImage;

    private DominoTile tile;
    private Sprite[] pipSprites;
    private UnityAction onClick;
    private bool faceDown = false;
    private Sprite backSprite;

    // Configuração inicial
    public void Setup(DominoTile tile, Sprite[] pipSprites, UnityAction onClick, Sprite backSprite, bool faceDown)
    {
        this.tile = tile;
        this.pipSprites = pipSprites;
        this.onClick = onClick;
        this.backSprite = backSprite;
        this.faceDown = faceDown;

        Refresh();

        var button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            if (onClick != null) button.onClick.AddListener(onClick);
        }
    }

    // Atualiza visual
    private void Refresh()
    {
        if (faceDown && backSprite != null)
        {
            sideAImage.sprite = backSprite;
            sideBImage.sprite = backSprite;
        }
        else if (tile != null && tile.A >= 0 && tile.B >= 0)
        {
            sideAImage.sprite = pipSprites[tile.A];
            sideBImage.sprite = pipSprites[tile.B];
        }
    }

    // Revela peça (quando jogo acaba)
    public void Reveal()
    {
        faceDown = false;
        Refresh();
    }
}