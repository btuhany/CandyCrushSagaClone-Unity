using Core;
using System.Collections.Generic;

public class Match
{
    private List<Matchable> _matchableList;
    public int Count => _matchableList.Count;
    public bool Collectable => Count >= _minMatch;

    public bool OriginExclusive { get => _originExclusive; set => _originExclusive = value; }
    public List<Matchable> MatchableList { get => _matchableList; set => _matchableList = value; }

    private const int _minMatch = 3;
    private bool _originExclusive;
    private MatchableGrid _grid;
    private MatchablePool _pool;
    private Matchable _originMatchable;
    public Match(Matchable matchable)
    {
        _matchableList = new List<Matchable>();
        _matchableList.Add(matchable);
        _originMatchable = matchable;
        _grid = (MatchableGrid)MatchableGrid.Instance;
        _pool = (MatchablePool)MatchablePool.Instance;
    }
    private void CollectMatchPoint()
    {
       
    }
    public void AddMatchable(Matchable matchable)
    {
        if(!_matchableList.Contains(matchable))
        {
            _matchableList.Add(matchable);
        }
    }
    public Match Merge(Match matchToMerge)
    {
        foreach (Matchable mathableToAdd in matchToMerge._matchableList)
        {
            AddMatchable(mathableToAdd);
        }
        return this;
    }
    public void Resolve()
    {
        CollectMatchPoint();
        for (int i = 0; i < _matchableList.Count; i++)
        {
            Matchable matchable = _matchableList[i];
        //    //if (matchable == null) continue;
            if (matchable.Variant.type == MatchableType.AreaExplode)
            {
        //    //    isTriggeredSpecialType = true;
        //    //    _matchableList.Remove(matchable);
                _grid.TriggerAreaExplode(matchable, this);
            }
            else if (matchable.Variant.type == MatchableType.HorizontalExplode)
            {
                _grid.StartCoroutine(_grid.TriggerHorizontalExplode(matchable, this));
            }
            else if (matchable.Variant.type == MatchableType.VerticalExplode)
            {
                _grid.StartCoroutine(_grid.TriggerVerticalExplode(matchable, this));
            }
        }

        for (int i = 0; i < _matchableList.Count; i++)
        {
            Matchable matchable = _matchableList[i];
            if (matchable.IsMoving) continue;
            matchable.CollectScorePoint();
            _grid.RemoveItemAt(matchable.GridPosition);
            _pool.ReturnObject(matchable);
            _grid.columnCoroutines[matchable.GridPosition.x] = _grid.StartCoroutine(_grid.CollapseRepopulateAndScanColumn(matchable.GridPosition.x));
        }
    }
    public override string ToString()
    {
        string s = "";
        s = $"Matchable Count: {_matchableList.Count}, \r\n";
        foreach (Matchable matchable in _matchableList)
        {
            s += "Matchable at " + matchable.GridPosition + ", Variant: " + matchable.Variant + ",\r\n";
        }
        return s;
    }
}
