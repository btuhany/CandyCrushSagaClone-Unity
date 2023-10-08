using Core;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Match
{
	private List<Matchable> _matchableList;
	public int Count => _matchableList.Count;
	public bool Collectable => _originExclusive ? Count >= _minMatch - 1 : Count >= _minMatch;

    public bool OriginExclusive { get => _originExclusive; set => _originExclusive = value; }

    private  const int _minMatch = 3;
	private bool _originExclusive;
	private MatchableGrid _grid;
	private MatchablePool _pool;
	public Match()
	{
		_matchableList = new List<Matchable>();
        _originExclusive = true;
		_grid = (MatchableGrid)MatchableGrid.Instance;
		_pool = (MatchablePool)MatchablePool.Instance;
    }
	public Match(Matchable matchable)
	{
        _matchableList = new List<Matchable>();
        _originExclusive = false;
        _matchableList.Add(matchable);
        _grid = (MatchableGrid)MatchableGrid.Instance;
        _pool = (MatchablePool)MatchablePool.Instance;
    }
	public void AddMatchable(Matchable matchable, bool checkIsAlreadyInMatch = false)
	{
		if(checkIsAlreadyInMatch)
		{
			bool isInList = false;
			foreach (Matchable matchableInList in _matchableList)
			{
				if (matchableInList == matchable)
				{
					isInList = true;
					break;
				}
            }
			if(!isInList)
				_matchableList.Add(matchable);
        }
		else
		{
			_matchableList.Add(matchable);
		}
	}
	public Match Merge(Match matchToMerge, bool checkIsAlreadyInList = false)
	{
		if(checkIsAlreadyInList)
		{
			List<Matchable> matchListToMerge = matchToMerge._matchableList;
			List<Matchable> matchablesToRemove = new List<Matchable>();
			for (int i = 0; i < matchListToMerge.Count; i++)
			{
				for (int j = 0; j < _matchableList.Count; j++)
				{
					if (_matchableList[j] == matchListToMerge[i])
					{
                        matchablesToRemove.Add(matchListToMerge[i]);
					}
				}
			}
			for (int i = 0; i < matchablesToRemove.Count; i++)
			{
				matchListToMerge.Remove(matchablesToRemove[i]);
			}
        }
		this._matchableList.AddRange(matchToMerge._matchableList);
		return this;
	}
	public void Resolve()
	{
		for (int i = 0; i < _matchableList.Count; i++)
		{
            Matchable matchable = _matchableList[i];
            if (matchable.Variant.type == MatchableType.AreaExplode)
			{
				for (int x = matchable.GridPosition.x - 1; x <= matchable.GridPosition.x + 1; x++)
				{
					for (int y = matchable.GridPosition.y - 1; y <= matchable.GridPosition.y + 1; y++)
					{
						if(!_grid.CheckBounds(x, y))
							continue;
						if (x == matchable.GridPosition.x && y == matchable.GridPosition.y)
							continue;
						AddMatchable(_grid.GetItemAt(x, y), true);
					}
				}
			}
		}
		for (int i = 0; i < _matchableList.Count; i++)
		{
			Matchable matchable = _matchableList[i];
			_grid.RemoveItemAt(matchable.GridPosition);
			_pool.ReturnObject(matchable);
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
