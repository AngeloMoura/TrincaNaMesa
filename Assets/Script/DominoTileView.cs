using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Domino
{
    /// <summary>
    /// Componente visual para representar uma pedra de dominó em 2D (Unity).
    /// </summary>
    public class DominoTileView : MonoBehaviour, IPointerClickHandler
    {
        public Image topValueImage;
        public Image bottomValueImage;

        private DominoTile tile;
        private System.Action<DominoTileView> onTileClicked;

        /// <summary>
        /// Inicializa a pedra visual com os valores corretos.
        /// </summary>
        public void Setup(DominoTile tile, Sprite[] pipSprites, System.Action<DominoTileView> onClick)
        {
            this.tile = tile;
            onTileClicked = onClick;

            // Assume que pipSprites[0..6] são os sprites de 0 a 6
            topValueImage.sprite = pipSprites[tile.A];
            bottomValueImage.sprite = pipSprites[tile.B];
        }

        /// <summary>
        /// Evento de clique (para o jogador escolher qual pedra jogar).
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            onTileClicked?.Invoke(this);
        }

        /// <summary>
        /// Obtém a pedra representada por este componente.
        /// </summary>
        public DominoTile GetTile() => tile;
    }
}
