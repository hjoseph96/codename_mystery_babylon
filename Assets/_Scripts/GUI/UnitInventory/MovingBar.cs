using DG.Tweening;
using UnityEngine;

public class MovingBar : MonoBehaviour
{
    [SerializeField] private float _startX, _endX, _movementDuration;

    private void OnEnable()
    {
        ResetPosition();

        var tr = transform;
        tr.DOLocalMoveX(_endX, _movementDuration)
            .SetEase(Ease.InOutCubic);
        DOTween.Play(tr);
    }

    private void OnDisable()
    {
        ResetPosition();
    }

    private void ResetPosition()
    {
        var tr = transform;
        var pos = tr.localPosition;
        tr.localPosition = new Vector3(_startX, pos.y, pos.z);
    }
}
