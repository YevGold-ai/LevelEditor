using UnityEngine;
using DG.Tweening;

namespace Code
{
    public class Tile : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private Material _tileMaterial;
        [SerializeField] private GameObject _tile;
        [SerializeField] private GameObject _positionSetUpBlock;

        private GameObject _block;

        private Tween _tileTween;
        private Tween _blockTween;

        public void SetColor(Color color)
        {
            _tileMaterial.color = color;
        }

        public void SetBlock(GameObject block)
        {
            _block = block;
            _block.transform.position = _positionSetUpBlock.transform.position;
            _block.transform.SetParent(this.transform);
            _block.transform.localScale = Vector3.zero; // чтобы при показе красиво появилось

            PlayAnimationShowBlock();
        }

        public void PlayAnimationShowTile()
        {
            _tile.transform.localScale = Vector3.zero;
            _tileTween?.Kill();

            _tileTween = _tile.transform.DOScale(Vector3.one, 0.25f)
                .SetEase(Ease.OutBack);
        }

        public void PlayAnimationHideTile()
        {
            _tileTween?.Kill();

            _tileTween = _tile.transform.DOScale(Vector3.zero, 0.2f)
                .SetEase(Ease.InBack);
        }

        public void PlayAnimationShowBlock()
        {
            if (_block == null) return;

            _block.transform.localScale = Vector3.zero;
            _blockTween?.Kill();

            _blockTween = _block.transform.DOScale(Vector3.one, 0.25f)
                .SetEase(Ease.OutBack)
                .OnComplete(() =>
                {
                    // лёгкая вибрация после появления
                    _block.transform.DOPunchScale(Vector3.one * 0.1f, 0.2f, 8, 0.5f);
                });
        }

        public void PlayAnimationHideBlock()
        {
            if (_block == null) return;

            _blockTween?.Kill();

            _blockTween = _block.transform.DOScale(Vector3.zero, 0.2f)
                .SetEase(Ease.InBack)
                .OnComplete(() =>
                {
                    Destroy(_block);
                    _block = null;
                });
        }
    }
}
