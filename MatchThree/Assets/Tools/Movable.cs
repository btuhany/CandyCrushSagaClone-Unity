using System.Collections;
using UnityEngine;
public class Movable : MonoBehaviour
{
    [SerializeField] float _speed = 1f;
    private Vector3 _to, _from;
    private float _howFar;
    private bool _isMoving = false;
    public bool IsMoving { get => _isMoving; }
    public IEnumerator MoveToPosition(Vector3 targetPosition)
    {
        _howFar = 0f;
        _to = targetPosition;
        _from = transform.position;
        _isMoving = false;
        do
        {
            _howFar += Time.deltaTime * _speed;
            if (_howFar > 1f)
                _howFar = 1f;
            transform.position = Vector3.LerpUnclamped(_from, _to, EaseFunc(_howFar));
            yield return null;
        }
        while (_howFar != 1f);
        _isMoving = true;
    }
    private float EaseFunc(float t)
    {
        return t * t;
    }
}