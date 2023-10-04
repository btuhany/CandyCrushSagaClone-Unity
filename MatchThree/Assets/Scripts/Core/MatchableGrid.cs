using UnityEngine;
using Tools;
using System;
using System.Collections;
using Unity.VisualScripting;

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
        private bool AreTwoMatch(Matchable matchable1, Matchable matchable2)
        {
            //if (matchable1.Variant.color != MatchableColor.None && matchable2.Variant.color != MatchableColor.None)
            //{
                if (matchable1.Variant.color != matchable2.Variant.color)
                    return false;
            //}
            return true;
        }
        private bool AreMatchables(params Matchable[] matchables)
        {
            for (int i = 0; i < matchables.Length - 1; i++)
            {
                if (!AreTwoMatch(matchables[i], matchables[i + 1]))
                    return false;
            }
            return true;
        }
        private bool IsPartOfAMatch(Matchable matchable)
        {
            int verticalMatchSize = 0;
            int horizontalMatchSize = 0;
            
            horizontalMatchSize += CountMatches(matchable, Vector2Int.left);
            //Debug.Log("Left count: " + horizontalMatchSize);
            horizontalMatchSize += CountMatches(matchable, Vector2Int.right);
            //Debug.Log("Horizontal count: " + horizontalMatchSize);

            verticalMatchSize += CountMatches(matchable, Vector2Int.up);
            //Debug.Log("Up count: " + verticalMatchSize);
            verticalMatchSize += CountMatches(matchable, Vector2Int.down);
            //Debug.Log("Vertical count: " + verticalMatchSize);

            if(verticalMatchSize >= 2 || horizontalMatchSize >= 2)
                return true;
            return false;
        }
        //Origin match exclusive
        //TODO: Check if matchable is moving or not
        private int CountMatches(Matchable matchable, Vector2Int direction)
        {
            int counter = 0;
            Vector2Int pos = matchable.GridPosition + direction;
            while (CheckBounds(pos) && !IsEmpty(pos))
            {
                Matchable nextMatchable = GetItemAt(pos);
                if(AreTwoMatch(matchable, nextMatchable) && !nextMatchable.IsMoving)
                {
                    counter++;
                    pos += direction;
                }
                else
                {
                    break;
                }
            }
            return counter;
        }
        private void SwapMatchables(Matchable matchable1, Matchable matchable2)
        {
            SwapItems(matchable1.GridPosition, matchable2.GridPosition);
            Vector2Int temp = matchable1.GridPosition;
            matchable1.GridPosition = matchable2.GridPosition;
            matchable2.GridPosition = temp;
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

                    if (!allowMatches && IsPartOfAMatch(matchable))
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
            matchable1.isSwapping = matchable2.isSwapping = true;
            yield return SwapAnim(matchable1, matchable2);

            SwapMatchables(matchable1, matchable2);

            if(!IsPartOfAMatch(matchable1)  && !IsPartOfAMatch(matchable2))
            {
                SwapMatchables(matchable1, matchable2);
                yield return SwapAnim(matchable1, matchable2);
            }
            matchable1.isSwapping = matchable2.isSwapping = false;
        }
        public override void ClearGrid()
        {
            for (int y = 0; y < Dimensions.y - 1; y++)
            {
                for (int x = 0; x < Dimensions.x - 1; x++)
                {
                    _pool.ReturnObject(GetItemAt(x, y));
                }
            }
            base.ClearGrid();
        }
    }
}

