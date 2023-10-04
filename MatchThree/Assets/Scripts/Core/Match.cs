using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Match
{
	private List<Matchable> _matchableList;
	public int Count => _matchableList.Count;
	public bool Collectable => _originExclusive ? Count >= _minMatch - 1 : Count >= _minMatch;
	private  const int _minMatch = 3;
	private bool _originExclusive;
	public Match()
	{
		_matchableList = new List<Matchable>();
        _originExclusive = true;
    }
	public Match(Matchable matchable)
	{
        _matchableList = new List<Matchable>();
        _originExclusive = false;
        _matchableList.Add(matchable);
    }
	public void AddMatchable(Matchable matchable)
	{
		_matchableList.Add(matchable);
	}
	public Match Merge(Match matchToMerge)
	{
		this._matchableList.AddRange(matchToMerge._matchableList);
		return this;
	}
	public void Resolve()
	{
		for (int i = 0; i < _matchableList.Count; i++)
		{
			_matchableList[i].gameObject.SetActive(false);
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
