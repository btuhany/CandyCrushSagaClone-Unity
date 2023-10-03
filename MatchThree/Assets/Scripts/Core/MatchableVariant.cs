using UnityEngine;

[CreateAssetMenu(fileName = "NewMatchableType", menuName = "MatchThree/MatchableType")]
public class MatchableVariant : ScriptableObject
{
    public MatchableColor color;
    public MatchableType type;
    public Sprite sprite;
}
public enum MatchableColor
{
    Red,
    Blue,
    Green,
    Purple,
    Orange,
    Yellow,
    None
}
public enum MatchableType
{
    Normal,
    HorizontalExplode,
    VerticalExplode,
    AreaExplode,
    ColorExplode
}