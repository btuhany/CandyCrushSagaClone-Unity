using UnityEngine;
using Tools;
using System.Collections;
using TMPro;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace Core
{
    public class MatchableGrid : GridSystem<Matchable>
    {
        [SerializeField] private float _matchableSpawnSpeedFactor = 1f;
        [SerializeField] private float _collapseSpeedFactor = 1f;
        [SerializeField] private float _horizontalVerticalExplodeDelays = 0.4f;

        [SerializeField] private float _gridCollapsePopulateScanDelay = 0.1f;
        [SerializeField] private float _scanGridDelay = 0.5f;
        [SerializeField] private float _repopulateGridDelay = 0.1f;
        
        [SerializeField] private float _columnCollapsePopulateScanDelay = 0.1f;
        [SerializeField] private float _repopulateColumnDelay = 0.1f;
        [SerializeField] private float _scanColumnDelay = 0.5f;
        
        [SerializeField] private float _horizontalVerticalExplodeStartDelay = 0.1f;
        [SerializeField] private TextMeshProUGUI _debugText;
        [Header("Grid Config")]
        [SerializeField] private Vector2 _spacing;
        private Transform _transform;
        private MatchablePool _pool;
        private Movable _move;
        private List<int> _lockedColumns = new List<int>();
        private WaitForSeconds _scanWaitDelay;
        private WaitForSeconds _explodeWaitDelay;
        private WaitForSeconds _collapsePopulateScanWaitDelay;
        protected override void Awake()
        {
            base.Awake();
            _transform = GetComponent<Transform>();
            _pool = (MatchablePool)MatchablePool.Instance;
            _move = GetComponent<Movable>();
           // _scanWaitDelay = new WaitForSeconds(_scanDelay);
           // _explodeWaitDelay = new WaitForSeconds(_explodeDelays);
           //_collapsePopulateScanWaitDelay = new WaitForSeconds(_gridCollapsePopulateScanDelay);
        }
        private void Start()
        {
            StartCoroutine(_move.MoveToPosition(_transform.position)); //from offset
        }
        private void Update()
        {
            _debugText.text = this.ToString();
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
                            if(!IsEmpty(x, yEmptyIndex))// && !GetItemAt(x, yEmptyIndex).isSwapping && !GetItemAt(x, yEmptyIndex).IsMoving)
                            {
                               MoveMatchable(GetItemAt(x, yEmptyIndex), x, y, _collapseSpeedFactor);
                               break; //??
                            }
                        }
                    }
                }
            }
        }
        private void CollapseColumn(int x)
        {
            for (int y = 0; y < Dimensions.y; y++)
            {
                if (IsEmpty(x, y))
                {
                    for (int yEmptyIndex = y + 1; yEmptyIndex < Dimensions.y; yEmptyIndex++)
                    {
                        if (!IsEmpty(x, yEmptyIndex))// && !GetItemAt(x, yEmptyIndex).isSwapping && !GetItemAt(x, yEmptyIndex).IsMoving)
                        {
                            MoveMatchable(GetItemAt(x, yEmptyIndex), x, y, _collapseSpeedFactor);
                            break; //??
                        }
                    }
                }
            }
        }
        public void SetMatchablePosition(Matchable matchable, int x, int y)
        {
            matchable.transform.position = new Vector3(x * _spacing.x, y * _spacing.y);
        }
        private void MoveMatchable(Matchable matchable, int posX, int posY, float speed)
        {
            //RemoveItemAt(matchable.GridPosition);
            //PutItemAt(matchable, posX, posY);
            MoveItemTo(matchable.GridPosition.x, matchable.GridPosition.y, posX, posY);
            matchable.GridPosition = new Vector2Int(posX, posY);
            //SetMatchablePosition(matchable, posX, posY);
            matchable.StartCoroutine(matchable.MoveToPositionNoLerp(new Vector3(posX * _spacing.x, posY * _spacing.y, 0f), speed));
            //TODO: Anim
        }
        private IEnumerator SwapAnim(Matchable matchable1, Matchable matchable2)
        {
            StartCoroutine(matchable1.MoveToPosition(matchable2.transform.position));
            yield return StartCoroutine(matchable2.MoveToPosition(matchable1.transform.position));
        }
        private IEnumerator CollapseRepopulateAndScanTheGrid()
        {
            yield return new WaitForSeconds(_gridCollapsePopulateScanDelay);
            CollapseGrid();
            yield return StartCoroutine(RepopulateGrid());
            yield return StartCoroutine(ScanForMatches());
        }
        public IEnumerator CollapseRepopulateAndScanColumn(int x, bool forced = false)
        {
            if(!forced)
            {
                if (_lockedColumns.Contains(x))         
                yield break;
            }
            _lockedColumns.Add(x);
            yield return new WaitForSeconds(_columnCollapsePopulateScanDelay);
            CollapseColumn(x);
            yield return StartCoroutine(RepopulateColumn(x));
            yield return StartCoroutine(ScanColumn(x));
            _lockedColumns.Remove(x);
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
        public IEnumerator RepopulateGrid(bool allowMatches = false)
        {
            yield return new WaitForSeconds(_repopulateGridDelay);
            Coroutine currentCoroutine = null;
            for (int x = 0; x < Dimensions.x; x++)
            {
                int positionOffset = 0;
                List<Matchable> newMatchables = new List<Matchable>();
                for (int y = 0; y < Dimensions.y; y++)
                {
                    if (CheckBounds(x, y) && !IsEmpty(x, y)) continue;
                    Matchable matchable = _pool.GetRandomVariantMatchable(false);
                    matchable.transform.parent = _transform;
                    PutItemAt(matchable, x, y);
                    matchable.SetColliderSize(_spacing);
                    matchable.GridPosition = new Vector2Int(x, y);
                    matchable.transform.position = _transform.position + new Vector3(matchable.GridPosition.x * _spacing.x, positionOffset * _spacing.y + Dimensions.y * _spacing.y, 0);
                    matchable.gameObject.SetActive(true);
                    newMatchables.Add(matchable);
                    if (!allowMatches && IsPartOfAMatch(matchable))
                    {
                        MakeMatchableUnfit(matchable);
                    }
                    positionOffset++;
                }
                foreach (Matchable matchable in newMatchables)
                {
                    currentCoroutine = matchable.StartCoroutine(
                        matchable.MoveToPositionNoLerp(
                            _transform.position + new Vector3(matchable.GridPosition.x * _spacing.x, matchable.GridPosition.y * _spacing.y), 
                            _matchableSpawnSpeedFactor
                        )
                     );
                }
            }
            yield return currentCoroutine;   
        }
        private IEnumerator RepopulateColumn(int x, bool allowMatches = false)
        {
            yield return new WaitForSeconds(_repopulateColumnDelay);
            Coroutine currentCoroutine = null;
            int positionOffset = 0;
            List<Matchable> newMatchables = new List<Matchable>();
            for (int y = 0; y < Dimensions.y; y++)
            {
                if (CheckBounds(x, y) && !IsEmpty(x, y)) continue;
                Matchable matchable = _pool.GetRandomVariantMatchable(false);
                matchable.transform.parent = _transform;
                PutItemAt(matchable, x, y);
                matchable.SetColliderSize(_spacing);
                matchable.GridPosition = new Vector2Int(x, y);
                matchable.transform.position = _transform.position + new Vector3(matchable.GridPosition.x * _spacing.x, positionOffset * _spacing.y + Dimensions.y * _spacing.y, 0);
                matchable.gameObject.SetActive(true);
                newMatchables.Add(matchable);
                if (!allowMatches && IsPartOfAMatch(matchable))
                {
                    MakeMatchableUnfit(matchable);
                }
                positionOffset++;
            }
            foreach (Matchable matchable in newMatchables)
            {
                currentCoroutine = matchable.StartCoroutine(
                    matchable.MoveToPositionNoLerp(
                        _transform.position + new Vector3(matchable.GridPosition.x * _spacing.x, matchable.GridPosition.y * _spacing.y),
                        _matchableSpawnSpeedFactor
                    )
                    );
            }

            yield return currentCoroutine;
        }
        public IEnumerator ScanForMatches()
        {
            bool isResolved = false;
            yield return new WaitForSeconds(_scanGridDelay);
            for (int x = 0; x < Dimensions.x; x++)
            {
                for (int y = 0; y < Dimensions.y; y++)
                {
                    if(!IsEmpty(x, y) && !GetItemAt(x, y).isSwapping && !GetItemAt(x, y).IsMoving)
                    {
                        if(IsPartOfAMatch(GetItemAt(x, y), out Match match))
                        {
                            match?.Resolve();
                            isResolved = true;
                        }
                    }
                }
            }
            if (isResolved)
                StartCoroutine(CollapseRepopulateAndScanTheGrid());
        }
        private IEnumerator ScanColumn(int x)
        {
            yield return new WaitForSeconds(_scanColumnDelay);
            for (int y = 0; y < Dimensions.y; y++)
            {
                if (!IsEmpty(x, y) && !GetItemAt(x, y).isSwapping && !GetItemAt(x, y).IsMoving)
                {
                    if (IsPartOfAMatch(GetItemAt(x, y), out Match match))
                    {
                        match?.Resolve();
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
                if((match1 != null && match1.isTriggeredSpecialType) || (match2 != null && match2.isTriggeredSpecialType))
                {

                }
                else
                {
                    //StartCoroutine(CollapseRepopulateAndScanTheGrid());
                }
            }
            matchable1.isSwapping = matchable2.isSwapping = false;
        }
        public IEnumerator TriggerHorizontalExplode(Matchable horizontalMatchable, bool delayed = false)
        {
            if(delayed)
                yield return new WaitForSeconds(_horizontalVerticalExplodeStartDelay);

            int y = horizontalMatchable.GridPosition.y;
            int horizontalX = horizontalMatchable.GridPosition.x;
            yield return new WaitForSeconds(_horizontalVerticalExplodeDelays);

            int x2 = horizontalX;
            int x1 = horizontalX;

            horizontalMatchable.CollectScorePoint();
            RemoveItemAt(horizontalMatchable.GridPosition);
            _pool.ReturnObject(horizontalMatchable);
            StartCoroutine(CollapseRepopulateAndScanColumn(horizontalMatchable.GridPosition.x, true));

            while (x1 >= 0 || x2 < Dimensions.x)
            {
                x2++;
                if (x2 < Dimensions.x && !IsEmpty(x2, y))
                {
                    Matchable matchable2 = GetItemAt(x2, y);
                    if (!matchable2.isSwapping && !matchable2.IsMoving)
                    {
                        if (matchable2.Variant.type == MatchableType.AreaExplode)
                        {
                            //TriggerBombExplode(matchable2);
                        }
                        else if (matchable2.Variant.type == MatchableType.VerticalExplode)
                        {
                            StartCoroutine(TriggerVerticalExplode(matchable2, true));
                        }
                        matchable2.CollectScorePoint();
                        RemoveItemAt(matchable2.GridPosition);
                        _pool.ReturnObject(matchable2);
                        StartCoroutine(CollapseRepopulateAndScanColumn(x2));

                    }
                }
                //if (!CheckBounds(x, y))
                //    continue;
                x1--;
                if (x1 >= 0 && !IsEmpty(x1, y))
                {
                    Matchable matchable = GetItemAt(x1, y);
                    if (!matchable.isSwapping && !matchable.IsMoving)
                    {
                        if (matchable.Variant.type == MatchableType.AreaExplode)
                        {
                            //TriggerBombExplode(matchable);
                        }
                        else if (matchable.Variant.type == MatchableType.VerticalExplode)
                        {
                            StartCoroutine(TriggerVerticalExplode(matchable, true));
                        }
                        matchable.CollectScorePoint();
                        RemoveItemAt(matchable.GridPosition);
                        _pool.ReturnObject(matchable);
                        StartCoroutine(CollapseRepopulateAndScanColumn(x1));
                    }
                }
                yield return new WaitForSeconds(_horizontalVerticalExplodeDelays);
            }
        }
        public IEnumerator TriggerVerticalExplode(Matchable verticalMatchable, bool delayed = false)
        {
            _lockedColumns.Add(verticalMatchable.GridPosition.x);
            if (delayed)
                yield return new WaitForSeconds(_horizontalVerticalExplodeStartDelay);

            int x = verticalMatchable.GridPosition.x;

            int y1 = verticalMatchable.GridPosition.y;
            int y2 = verticalMatchable.GridPosition.y;

            verticalMatchable.CollectScorePoint();
            RemoveItemAt(verticalMatchable.GridPosition);
            _pool.ReturnObject(verticalMatchable);
            yield return new WaitForSeconds(_horizontalVerticalExplodeDelays);

            while (y2 < Dimensions.y || y1 >= 0)
            {
                y2++;
                if (y2 < Dimensions.y && !IsEmpty(x, y2))
                {
                    Matchable matchable2 = GetItemAt(x, y2);
                    if (!matchable2.isSwapping && !matchable2.IsMoving)
                    {
                        if (matchable2.Variant.type == MatchableType.AreaExplode)
                        {
                            //TriggerBombExplode(matchable2);
                        }
                        else if (matchable2.Variant.type == MatchableType.HorizontalExplode)
                        {
                            StartCoroutine(TriggerHorizontalExplode(matchable2, true));
                        }
                        matchable2.CollectScorePoint();
                        RemoveItemAt(matchable2.GridPosition);
                        _pool.ReturnObject(matchable2);
                    }
                }
                y1--;
                //if (!CheckBounds(x, y1))
                //    continue;
                if (y1 >= 0 && !IsEmpty(x, y1))
                {
                    Matchable matchable = GetItemAt(x, y1);
                    if(!matchable.isSwapping && !matchable.IsMoving)
                    {
                        if (matchable.Variant.type == MatchableType.AreaExplode)
                        {
                            //TriggerBombExplode(matchable);
                        }
                        else if (matchable.Variant.type == MatchableType.HorizontalExplode)
                        {
                            StartCoroutine(TriggerHorizontalExplode(matchable, true));
                        }
                        matchable.CollectScorePoint();
                        RemoveItemAt(matchable.GridPosition);
                        _pool.ReturnObject(matchable);
                    }
                }
                yield return new WaitForSeconds(_horizontalVerticalExplodeDelays);
            }
            _lockedColumns.Remove(verticalMatchable.GridPosition.x);
            yield return StartCoroutine(CollapseRepopulateAndScanColumn(verticalMatchable.GridPosition.x));
        }
        public void TriggerBombExplode(Matchable bombMatchable)
        {
            bombMatchable.CollectScorePoint();
            for (int x = bombMatchable.GridPosition.x - 1; x <= bombMatchable.GridPosition.x + 1; x++)
            {
                for (int y = bombMatchable.GridPosition.y - 1; y <= bombMatchable.GridPosition.y + 1; y++)
                {
                    if (!CheckBounds(x, y) || IsEmpty(x, y))
                        continue;
                    Matchable matchable = GetItemAt(x, y);

                    if (matchable == bombMatchable || matchable.isSwapping || matchable.IsMoving)
                        continue;

                    //if(matchable.Variant.type == MatchableType.HorizontalExplode)
                    //{
                    //    StartCoroutine(TriggerHorizontalExplode(matchable, 0.1f));
                    //}
                    //else if(matchable.Variant.type == MatchableType.HorizontalExplode)
                    //{
                    //    StartCoroutine(TriggerVerticalExplode(matchable, 0.1f));
                    //}
                    //else if(matchable.Variant.type == MatchableType.AreaExplode)
                    //{
                    //    gridCheckAfter = false;
                    //    TriggerBombExplode(matchable);
                    //}
                    //matchable.CollectScorePoint();
                    //RemoveItemAt(matchable.GridPosition);
                    //_pool.ReturnObject(matchable);
                }
            }
        }
        public override void ClearGrid()
        {
            for (int y = 0; y < Dimensions.y; y++)
            {
                for (int x = 0; x < Dimensions.x; x++)
                {
                    _pool.ReturnObject(GetItemAt(x, y));
                }
            }
            base.ClearGrid();
        }
    }
}

