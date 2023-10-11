using System.Collections;
using UnityEngine;
public class Movable : MonoBehaviour
{
    [SerializeField] private Vector3Int _startOffset = Vector3Int.zero;
    [SerializeField] private float _speed = 1f;
    private Vector3 _to, _from;
    private float _howFar;
    private bool _isMoving = false;
    public bool IsMoving { get => _isMoving; }
    private Transform _transform;
    protected virtual void Awake()
    {
        _transform = GetComponent<Transform>();
    }
    public IEnumerator MoveToPosition(Vector3 targetPosition)
    {
        _howFar = 0f;
        _to = targetPosition;
        _from = _transform.position + _startOffset;
        _isMoving = true;
        do
        {
            _howFar += Time.deltaTime * _speed;
            if (_howFar > 1f)
                _howFar = 1f;
            _transform.position = Vector3.LerpUnclamped(_from, _to, EaseFunc(_howFar));
            yield return null;
        }
        while (_howFar != 1f);
        _isMoving = false;
    }
    public IEnumerator MoveToPosition(Vector3 targetPosition, float speed)
    {
        _howFar = 0f;
        _to = targetPosition;
        _from = _transform.position + _startOffset;
        _isMoving = true;
        do
        {
            _howFar += Time.deltaTime * speed;
            if (_howFar > 1f)
                _howFar = 1f;
            _transform.position = Vector3.LerpUnclamped(_from, _to, EaseFunc(_howFar));
            yield return null;
        }
        while (_howFar != 1f);
        _isMoving = false;
    }
    public IEnumerator MoveToPosition(Vector2 targetPosition)
    {
        _howFar = 0f;
        _to = targetPosition;
        _from = _transform.position + _startOffset;
        _isMoving = false;
        do
        {
            _howFar += Time.deltaTime * _speed;
            if (_howFar > 1f)
                _howFar = 1f;
            _transform.position = Vector3.LerpUnclamped(_from, _to, EaseFunc(_howFar));
            yield return null;
        }
        while (_howFar != 1f);
        _isMoving = true;
    }
    public IEnumerator MoveToPositionNoLerp(Vector3 targetPosition, float speed)
    {
        _to = targetPosition;
        _from = _transform.position + _startOffset;
        Vector3 dir = _to - _from;
        _isMoving = true;
        do
        {   
            float distanceBefore = Vector3.Distance(_transform.position, targetPosition);
            _transform.position += dir * speed * Time.deltaTime;
            float distanceAfter = Vector3.Distance(_transform.position, targetPosition);
            if (distanceBefore < distanceAfter)
            {
                _transform.position = _to;
                break;
            }
            yield return null;
        }
        while (Vector3.Distance(transform.position, _to) > 0.02f);
        _transform.position = _to;
        _isMoving = false;
    }
    private float EaseFunc(float t)
    {
        return t;
    }
}