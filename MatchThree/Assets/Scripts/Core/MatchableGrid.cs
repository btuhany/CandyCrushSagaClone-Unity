using UnityEngine;
using Tools;
using System;
using System.Collections;

namespace Core
{
    public class MatchableGrid : GridSystem<Matchable>
    {
        [Header("Grid Config")]
        [SerializeField] private Vector2 _spacing;
        private Transform _transform;
        private MatchablePool _pool;
        private Movable _move;
        protected override void Awake()
        {
            base.Awake();
            _transform = GetComponent<Transform>();
            _pool = (MatchablePool)MatchablePool.Instance;
            _move = GetComponent<Movable>();
        }
        private void Start()
        {
            StartCoroutine(_move.MoveToPosition(_transform.position)); //from offset
        }
     
        private void MakeMatchableUnfit(Matchable matchable)
        {
            return;
        }
        private bool AreTwoMatchables(Matchable matchable1, Matchable matchable2)
        {
            if (matchable1.Variant.color != MatchableColor.None && matchable2.Variant.color != MatchableColor.None)
            {
                if (matchable1.Variant.color != matchable2.Variant.color)
                    return false;
            }
            return true;
        }
        private bool AreMatchables(params Matchable[] matchables)
        {
            for (int i = 0; i < matchables.Length - 1; i++)
            {
                if (!AreTwoMatchables(matchables[i], matchables[i + 1]))
                    return false;
            }
            return true;
        }
        private bool TryMatchingAt(Matchable matchable, Vector2Int gridPos)
        {
            int x = gridPos.x;
            int y = gridPos.y;

            if(y < Dimensions.y - 2)
                if (AreMatchables(matchable, _data[x, y + 1], _data[x, y + 2]))
                    return true;
            if(y > 2)
                if (AreMatchables(matchable, _data[x, y - 1], _data[x, y - 2]))
                    return true;
            if(x < Dimensions.x - 2)
                if (AreMatchables(matchable, _data[x + 1, y], _data[x + 2, y]))
                    return true;
            if(x > 2)
                if (AreMatchables(matchable, _data[x - 1, y], _data[x - 2, y]))
                    return true;

            return false;
        }
        private bool TryMatching(Matchable matchable)
        {
            return false;
        }
        private IEnumerator SwapAnim(Matchable matchable1, Matchable matchable2)
        {
            StartCoroutine(matchable1.MoveToPosition(matchable2.transform.position));
            yield return StartCoroutine(matchable2.MoveToPosition(matchable1.transform.position));
        }
        public void PopulateGrid(bool allowMatches = false)
        {
            for (int y = 0; y < Dimensions.y - 1; y++)
            {
                for (int x = 0; x < Dimensions.x - 1; x++)
                {
                    Matchable matchable = _pool.GetRandomVariantMatchable(false);
                    matchable.transform.parent = _transform;
                    PutItemAt(matchable, x, y);
                    matchable.transform.position = _transform.position + new Vector3(x * _spacing.x, y * _spacing.y);
                    matchable.SetColliderSize(_spacing);
                    matchable.GridPosition = new Vector2Int(x, y);
                    matchable.gameObject.SetActive(true);

                    if (!allowMatches && IsPartOfAMatchAt(matchable))
                    {
                        MakeMatchableUnfit(matchable);
                    }
                }
            }
        }
        public bool AreAdjacents(Matchable matchable1, Matchable matchable2)
        {
            int x1 = matchable1.GridPosition.x;
            int y1 = matchable1.GridPosition.y;

            int x2 = matchable2.GridPosition.x;
            int y2 = matchable2.GridPosition.y;

            if(x1 == x2)
            {
                if (y1 == y2 + 1 || y1 == y2 - 1)
                    return true;
            }
            else if(y1 == y2)
            {
                if (x1 == x2 + 1 || x1 == x2 - 1)
                    return true;
            }

            return false;
        }
        public IEnumerator TrySwap(Matchable matchable1, Matchable matchable2)
        {
            yield return SwapAnim(matchable1, matchable2);

            if(!TryMatchingAt(matchable1, matchable2.GridPosition) && !TryMatchingAt(matchable2, matchable1.GridPosition))
            {
                yield return SwapAnim(matchable1, matchable2);
            }
            else
            {
                Debug.Log("Part Of A Match!");
            }
           

        }
        public override void ClearGrid()
        {
            for (int y = 0; y < Dimensions.y - 1; y++)
            {
                for (int x = 0; x < Dimensions.x - 1; x++)
                {
                    _pool.ReturnObject(_data[x, y]);
                }
            }
            base.ClearGrid();
        }
    }
}

