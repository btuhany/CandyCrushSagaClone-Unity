using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScorePointFX : Movable
{
    [SerializeField] private float _colorFadeSpeed = 0.7f;
    private TextMeshProUGUI _text;
    private float _timeCounter = 0f;

    private readonly Color _orangeColor = new Color(1f, 0.4f, 0f);
    private readonly Color _blueColor = new Color(0.06f, 0.5f, 1f);
    protected override void Awake()
    {
        base.Awake();
        _text = GetComponentInChildren<TextMeshProUGUI>();
    }
    private void Update()
    {
        _timeCounter += Time.deltaTime;
        if(_timeCounter > 1 / _colorFadeSpeed)
        {
            _timeCounter = 0f;
            ScorePointFXPool.Instance.ReturnObject(this);
        }
    }
    private IEnumerator FadeColorAlpha()
    {
        float howFar = 0f;
        do
        {
            howFar += Time.deltaTime * _colorFadeSpeed;
            if (howFar > 1f)
                howFar = 1f;
            float colorAlpha = Mathf.Lerp(1, 0, howFar);
            _text.color = new Color(_text.color.r, _text.color.g, _text.color.b, colorAlpha);
            yield return null;
        }
        while (howFar != 1f);
    }
    private void SetTextColor(MatchableColor color)
    {
        switch (color)
        {
            case MatchableColor.Red:
                _text.color = Color.red;
                break;
            case MatchableColor.Blue:
                _text.color = _blueColor;
                break;
            case MatchableColor.Green:
                _text.color = Color.green;
                break;
            case MatchableColor.Purple:
                _text.color = Color.magenta;
                break;
            case MatchableColor.Orange:
                _text.color = _orangeColor;
                break;
            case MatchableColor.Yellow:
                _text.color = Color.yellow;
                break;
            case MatchableColor.None:
                break;
            default:
                break;
        }
    }
    private void PlayAtPos(Vector3 pos)
    {
        transform.position = new Vector3(pos.x, pos.y, 1f);
        _timeCounter = 0f;
        _text.color = new Color(_text.color.r, _text.color.g, _text.color.b, 1.0f);
        StartCoroutine(MoveToPosition(transform.position + Vector3.up * 2f));
        StartCoroutine(FadeColorAlpha());
    }
    public void PlayFX(Vector3 pos, int score, MatchableColor color)
    {
        SetTextColor(color);
        _text.text = score.ToString();
        PlayAtPos(pos);
    }
}
