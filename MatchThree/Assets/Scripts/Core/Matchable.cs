using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Matchable : Movable
{
    private MatchableVariant _variant;
    private SpriteRenderer _spriteRenderer;
    private Vector2Int _gridPosition;
    private BoxCollider2D _boxCollider;
    private readonly Color _selectedColor = Color.HSVToRGB(0f, 0f, 0.7f);
    public MatchableVariant Variant { get => _variant; }
    public Vector2Int GridPosition { get => _gridPosition; set => _gridPosition = value; }

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _boxCollider = GetComponent<BoxCollider2D>();
    }
    public void SetColliderSize(Vector2 size)
    {
        _boxCollider.size = size;
    }
    public void SetVariant(MatchableVariant variant)
    {
        _variant = variant;
        _spriteRenderer.sprite = variant.sprite;
    }
    public void GetSelected()
    {
        _spriteRenderer.color = _selectedColor;
    }
    public void GetUnselected()
    {
        _spriteRenderer.color = Color.white;
    }
    //public override string ToString()
    //{
    //    return gameObject.name;
    //}
}
