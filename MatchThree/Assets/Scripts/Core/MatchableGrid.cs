using UnityEngine;
using Tools;
using System.Collections;
using TMPro;

namespace Core
{
    public class MatchableGrid : GridSystem<Matchable>
    {
        [SerializeField] private TextMeshProUGUI _debugText;
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
            _pool.ChangeToAnotherRandomVariant(matchable);
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
            Match matchOnLeft = GetMatchesInDirection(matchable, Vector2Int.left);
            Match matchOnRight = GetMatchesInDirection(matchable, Vector2Int.right);
            Match matchOnUp = GetMatchesInDirection(matchable, Vector2Int.up);
            Match matchDown = GetMatchesInDirection(matchable, Vector2Int.down);

            Match horizontalMatch = matchOnLeft.Merge(matchOnRight, true);
            Match verticalMatch = matchOnUp.Merge(matchDown, true);

            if (horizontalMatch.Collectable || verticalMatch.Collectable)
                return true;
            return false;
        }
        private bool IsPartOfAMatch(Matchable matchable, out Match matchGroup)
        {
            Match matchOnLeft = GetMatchesInDirection(matchable, Vector2Int.left);
            //Debug.Log("matchOnLeft: " + matchOnLeft);
            Match matchOnRight = GetMatchesInDirection(matchable, Vector2Int.right);
            //Debug.Log("matchOnRight: " + matchOnRight);
            Match matchOnUp = GetMatchesInDirection(matchable, Vector2Int.up);
            //Debug.Log("matchOnUp: " + matchOnUp);
            Match matchDown = GetMatchesInDirection(matchable, Vector2Int.down);
            //Debug.Log("matchOnDown: " + matchDown);

            Match horizontalMatch = matchOnLeft.Merge(matchOnRight);
            horizontalMatch.AddMatchable(matchable);
            horizontalMatch.OriginExclusive = false;
            //Debug.Log("horizontalMatch: " + horizontalMatch);
            Match verticalMatch = matchOnUp.Merge(matchDown);
            verticalMatch.AddMatchable(matchable);
            verticalMatch.OriginExclusive = false;
            //Debug.Log("verticalMatch: " + verticalMatch);

            
            if(horizontalMatch.Collectable)
            {
                matchGroup = horizontalMatch;
                if(verticalMatch.Collectable)
                {
                    matchGroup.Merge(verticalMatch, true);
                }
                return true;
            }
            else if(verticalMatch.Collectable)
            {
                matchGroup = verticalMatch;
                return true;
            }
            else
            {
                matchGroup = null;
                return false;
            }
        }
        //Origin match exclusive
        private Match GetMatchesInDirection(Matchable matchable, Vector2Int direction, bool originExclusive = true)
        {
            Match match;
            if (originExclusive)
                match = new Match();
            else
                match = new Match(matchable);

            int counter = 0;
            Vector2Int pos = matchable.GridPosition + direction;
            while (CheckBounds(pos) && !IsEmpty(pos))
            {
                Matchable otherMatchable = GetItemAt(pos);
                if(AreTwoMatch(matchable, otherMatchable)) //&& !otherMatchable.IsMoving && !otherMatchable.isSwapping)
                {
                    match.AddMatchable(otherMatchable);
                    counter++;
                    pos += direction;
                }
                else
                {
                    break;
                }
            }
            return match;
        }
        private void SwapMatchables(Matchable matchable1, Matchable matchable2)
        {
            SwapItems(matchable1.GridPosition, matchable2.GridPosition);
            Vector2Int temp = matchable1.GridPosition;
            matchable1.GridPosition = matchable2.GridPosition;
            matchable2.GridPosition = temp;
        }
        private void CollapseGrid()
        {
            for (int x = 0; x < Dimensions.x; x++)
            {
                for (int y = 0; y < Dimensions.y; y++)
                {
                    if(IsEmpty(x, y))
                    {
                        for (int yEmptyIndex = y + 1; yEmptyIndex < Dimensions.y; yEmptyIndex++)
                        {
                            if(!IsEmpty(x, yEmptyIndex) && !GetItemAt(x, yEmptyIndex).isSwapping && !GetItemAt(x, yEmptyIndex).IsMoving)
                            {
                               MoveMatchableTo(GetItemAt(x, yEmptyIndex), x, y);
                               break; //??
                            }
                        }
                    }
                }
            }
        }
        public void SetMatchablePosition(Matchable matchable, int x, int y)
        {
            matchable.transform.position = new Vector3(x * _spacing.x, y * _spacing.y);
        }
        private void MoveMatchableTo(Matchable matchable, int posX, int posY)
        {
            //RemoveItemAt(matchable.GridPosition);
            //PutItemAt(matchable, posX, posY);
            MoveItemTo(matchable.GridPosition.x, matchable.GridPosition.y, posX, posY);
            matchable.GridPosition = new Vector2Int(posX, posY);
            SetMatchablePosition(matchable, posX, posY);
            //TODO: Anim
        }
        private IEnumerator SwapAnim(Matchable matchable1, Matchable matchable2)
        {
            StartCoroutine(matchable1.MoveToPosition(matchable2.transform.position));
            yield return StartCoroutine(matchable2.MoveToPosition(matchable1.transform.position));
        }
        public void PopulateGrid(bool allowMatches = false)
        {
            for (int y = 0; y < Dimensions.y; y++)
            {
                for (int x = 0; x < Dimensions.x; x++)
                {
                    if (CheckBounds(x, y) && !IsEmpty(x, y)) continue;
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
        public IEnumerator TryMatch(Matchable matchable1, Matchable matchable2)
        {
            if (matchable1.isSwapping || matchable2.isSwapping || matchable1.IsMoving || matchable2.IsMoving)
                yield break;

            matchable1.isSwapping = matchable2.isSwapping = true;
            yield return SwapAnim(matchable1, matchable2);

            SwapMatchables(matchable1, matchable2);

            bool swapBack = true;
            if(IsPartOfAMatch(matchable1, out Match match1))
            {
                match1?.Resolve();
                swapBack = false;
            }
            if(IsPartOfAMatch(matchable2, out Match match2))
            {
                match2?.Resolve();
                swapBack = false;
            }
            if(swapBack)
            {
                SwapMatchables(matchable1, matchable2);
                yield return SwapAnim(matchable1, matchable2);
            }
            else
            {
                CollapseGrid();
                PopulateGrid();
                //for (int i = 0; i < Dimensions.y; i++)
                //{
                //    for (int x = 0; x < Dimensions.x; x++)
                //    {
                //        if (IsEmpty(x, i))
                //            Debug.Log($"x: {x}, y: {i}");
                //    }
                //}
            }
            matchable1.isSwapping = matchable2.isSwapping = false;
            _debugText.text = this.ToString();
            //Debug.Log(this.ToString());
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

