using UnityEngine;

public class MatchableFX : MonoBehaviour
{
    [SerializeField] private ParticleSystem _mainParticle;
    [SerializeField] private ParticleSystem _colorParticle;
    [SerializeField] private ParticleSystem _horizontalParticle;
    [SerializeField] private ParticleSystem _verticalParticle;

    private float _timeCounter = 0f;
    private const float _timeToReturnToPool = 3f;

    private readonly Color _orangeColor = new Color(1f, 0.4f, 0f);
    private readonly Color _blueColor = new Color(0.06f, 0.5f, 1f);
    private void Awake()
    {
        
    }
    private void OnEnable()
    {
        _timeCounter = 0f;
    }
    private void Update()
    {
        _timeCounter += Time.deltaTime;
        if(_timeCounter > _timeToReturnToPool)
        {
            _timeCounter = 0f;
            MatchableFXPool.Instance.ReturnObject(this);
        }
    }
    public void ChangeColor(MatchableColor color)
    {
        ParticleSystem.MainModule main = _colorParticle.main;
        switch (color)
        {
            case MatchableColor.Red:
                main.startColor = Color.red;
                break;
            case MatchableColor.Blue:
                main.startColor = _blueColor;
                break;
            case MatchableColor.Green:
                main.startColor = Color.green;
                break;
            case MatchableColor.Purple:
                main.startColor = Color.magenta;
                break;
            case MatchableColor.Orange:
                main.startColor = _orangeColor;
                break;
            case MatchableColor.Yellow:
                main.startColor = Color.yellow;
                break;
            case MatchableColor.None:
                break;
            default:
                break;
        }
        
    }
    public void PlayFX(MatchableType type)
    {
        if (type == MatchableType.Normal)
        {
            _mainParticle.Play();
        }
        else if (type == MatchableType.HorizontalExplode)
        {
            _horizontalParticle.Play();
        }
        else if (type == MatchableType.VerticalExplode)
        {
            _verticalParticle.Play();
        }
    }
    public void PlayColorExplode(Transform target)
    {

    }
}
