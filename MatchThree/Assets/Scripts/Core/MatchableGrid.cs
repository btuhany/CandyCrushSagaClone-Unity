using UnityEngine;
using Tools;
using System.Collections;
using TMPro;
using System.Collections.Generic;

namespace Core
{
    public class MatchableGrid : GridSystem<Matchable>
    {
        [SerializeField] private float _matchableSpawnSpeedFactor = 1f;
        [SerializeField] private float _collapseSpeedFactor = 1f;

        //TODO: Cache WaitForSeconds delays in the Awake Funct.
        [SerializeField] private float _horizontalVerticalExplodeDelays = 0.4f;

        [SerializeField] private float _gridCollapsePopulateScanDelay = 0.1f;
        [SerializeField] private float _scanGridDelay = 0.5f;
        [SerializeField] private float _repopulateGridDelay = 0.1f;
        
        [SerializeField] private float _columnCollapsePopulateScanDelay = 0.1f;
        [SerializeField] private float _repopulateColumnDelay = 0.1f;
        [SerializeField] private float _scanColumnDelay = 0.5f;

        [SerializeField] private float _colorExplodeStartDelay = 0.15f;
        [SerializeField] private float _colorExplodeDelays = 0.075f;
        [SerializeField] private TextMeshProUGUI _debugText;
        [Header("Grid Config")]
        [SerializeField] private Vector2 _spacing;
        private Transform _transform;
        private MatchablePool _pool;
        private Movable _move;
        private List<int> _lockedColumns = new List<int>();
        private List<int> _lockedTriggerColumns = new List<int>();
        public Coroutine[] columnCoroutines;
        private Coroutine _checkGridCoroutine;
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
            columnCoroutines = new Coroutine[Dimensions.x];
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
            if (matchable1.Variant.color == MatchableColor.None || matchable2.Variant.color == MatchableColor.None)
                return false;
            if (matchable1.Variant.color != matchable2.Variant.color)
                return false;

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
            
            Match horizontalMatch = matchOnLeft.Merge(matchOnRight);
            Match verticalMatch = matchOnUp.Merge(matchDown);

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

            if (horizontalMatch.Collectable)
            {
                matchGroup = horizontalMatch;
                if(verticalMatch.Collectable)
                {
                    matchGroup.Merge(verticalMatch);
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
        private Match GetMatchesInDirection(Matchable matchable, Vector2Int direction)
        {
            Match match = new Match(matchable);
            Vector2Int pos = matchable.GridPosition + direction;
            while (CheckBounds(pos) && !IsEmpty(pos))
            {
                Matchable otherMatchable = GetItemAt(pos);
                if(AreTwoMatch(matchable, otherMatchable)) //&& !otherMatchable.IsMoving && !otherMatchable.isSwapping)
                {
                    match.AddMatchable(otherMatchable);
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
                        if (!IsEmpty(x, yEmptyIndex) && !GetItemAt(x, yEmptyIndex).IsMoving)// && !GetItemAt(x, yEmptyIndex).isSwapping )
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
        private bool IsSpecialCombo(Matchable matchable1, Matchable matchable2)
        {
            Match nullMatch = null;
            MatchableType type1 = matchable1.Variant.type;
            MatchableType type2 = matchable2.Variant.type;

            if(type1 == MatchableType.ColorExplode)
            {
                int xColorExplode = matchable1.GridPosition.x;
                if (type2 != MatchableType.ColorExplode)
                {
                    RemoveItemAt(matchable1.GridPosition);
                    _pool.ReturnObject(matchable1);
                }
                StartCoroutine(TriggerColorExplode(matchable2, matchable1));
                //columnCoroutines[xColorExplode] = StartCoroutine(CollapseRepopulateAndScanColumn(xColorExplode));
                return true;
            }
            else if(type2 == MatchableType.ColorExplode)
            {
                int xColorExplode = matchable2.GridPosition.x;
                if (type1 != MatchableType.ColorExplode)
                {
                    RemoveItemAt(matchable2.GridPosition);
                    _pool.ReturnObject(matchable2);
                }
                StartCoroutine(TriggerColorExplode(matchable1, matchable2));
                //columnCoroutines[xColorExplode] = StartCoroutine(CollapseRepopulateAndScanColumn(xColorExplode));
                return true;
            }

            if((type1 == MatchableType.HorizontalExplode || type1 == MatchableType.VerticalExplode) && (type2 == MatchableType.HorizontalExplode || type2 == MatchableType.VerticalExplode))
            {
                if(type1 == MatchableType.HorizontalExplode)
                {
                    StartCoroutine(TriggerHorizontalExplode(matchable1, nullMatch, true));
                    StartCoroutine(TriggerVerticalExplode(matchable2, nullMatch));
                }
                else if(type1 == MatchableType.VerticalExplode)
                {
                    StartCoroutine(TriggerVerticalExplode(matchable1, nullMatch));
                    StartCoroutine(TriggerHorizontalExplode(matchable2, nullMatch, true));
                }
                return true;
            }

            if(((type1 == MatchableType.HorizontalExplode || type1 == MatchableType.VerticalExplode) && type2 == MatchableType.AreaExplode) || ((type2 == MatchableType.HorizontalExplode || type2 == MatchableType.VerticalExplode) && type1 == MatchableType.AreaExplode))
            {
                if(matchable1.Variant.type == MatchableType.AreaExplode)
                    StartCoroutine(TriggerAreaStripedCombo(matchable1, matchable2));
                else
                    StartCoroutine(TriggerAreaStripedCombo(matchable2, matchable1));
            }

            if(type1 == MatchableType.AreaExplode && type2 == MatchableType.AreaExplode)
            {
                TriggerAreaExplode(matchable1, null, true);
                TriggerAreaExplode(matchable2, null, true);
            }

            return false;
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
            if (_lockedTriggerColumns.Contains(x))
                yield break;

            if(!forced && _lockedColumns.Contains(x))
            {
                if (columnCoroutines[x] != null)
                    StopCoroutine(columnCoroutines[x]);
                //yield break;
            }
            else
            {
                if(!_lockedColumns.Contains(x))
                    _lockedColumns.Add(x);
                //Debug.Log("++ " + x + " added to locked list.");
            }

            yield return new WaitForSeconds(_columnCollapsePopulateScanDelay);
            CollapseColumn(x);
            yield return StartCoroutine(RepopulateColumn(x));
            if(_lockedColumns.Contains(x))
                _lockedColumns.Remove(x);
            //Debug.Log("-- " + x + " removed from the locked list.");
            yield return StartCoroutine(ScanColumn(x));
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
        private IEnumerator CheckGridForCollapse()
        {
            yield return new WaitForSeconds(3f);
            for (int x = 0; x < Dimensions.x; x++)
            {
                for (int y = 0; y < Dimensions.y; y++)
                {
                    if(IsEmpty(x, y))
                    {
                        StartCoroutine(CollapseRepopulateAndScanColumn(x));
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

            if (_checkGridCoroutine != null)
                StopCoroutine(_checkGridCoroutine);

            matchable1.isSwapping = matchable2.isSwapping = true;
            yield return SwapAnim(matchable1, matchable2);

            matchable1.isSwapping = matchable2.isSwapping = false;
            SwapMatchables(matchable1, matchable2);
            
            bool swapBack = true;
            
            if(IsSpecialCombo(matchable1, matchable2))
            {
                _checkGridCoroutine = StartCoroutine(CheckGridForCollapse());
                yield break;
            }
            if(IsPartOfAMatch(matchable1, out Match match1))
            {
                swapBack = false;
                match1?.Resolve();
            }
            if(IsPartOfAMatch(matchable2, out Match match2))
            {
               swapBack = false;
               match2?.Resolve();
            }
            if(swapBack)
            {
                matchable1.isSwapping = matchable2.isSwapping = true;
                SwapMatchables(matchable1, matchable2);
                yield return SwapAnim(matchable1, matchable2);
                matchable1.isSwapping = matchable2.isSwapping = false;
            }
            else
            {
                _checkGridCoroutine = StartCoroutine(CheckGridForCollapse());
            }
        }
        public IEnumerator TriggerColorExplode(Matchable matchable, Matchable colorExplodeMatchable)
        {
            MatchableType type = matchable.Variant.type;
            int colorExplodeX = colorExplodeMatchable.GridPosition.x;
            Vector3 matchablePos = colorExplodeMatchable.transform.position;
            switch (type)
            {
                case MatchableType.Normal:
                    //yield return new WaitForSeconds(0.1f);
                    for (int x = 0; x < Dimensions.x; x++)
                    {
                        for (int y = 0; y < Dimensions.y; y++)
                        {
                            if (!IsEmpty(x, y))
                            {
                                Matchable matchableAtPos = GetItemAt(x, y);
                                if (matchableAtPos.Variant.color == matchable.Variant.color)
                                {
                                    ColorExplodeFX fxObj = ColorExplodeFXPool.Instance.GetObject();
                                    fxObj.transform.position = matchablePos;
                                    fxObj.PlayFX(matchableAtPos.transform.position);
                                    RemoveItemAt(matchableAtPos.GridPosition);
                                    _pool.ReturnObject(matchableAtPos);
                                    columnCoroutines[x] = StartCoroutine(CollapseRepopulateAndScanColumn(x));
                                }
                            }
                        }
                    }
                    columnCoroutines[colorExplodeX] = StartCoroutine(CollapseRepopulateAndScanColumn(colorExplodeX));
                    break;
                case MatchableType.HorizontalExplode:
                    List<Matchable> horizontalsToTrigger = new List<Matchable>();
                    for (int x = 0; x < Dimensions.x; x++)
                    {
                        for (int y = 0; y < Dimensions.y; y++)
                        {
                            if (!IsEmpty(x, y))
                            {
                                Matchable matchableAtPos = GetItemAt(x, y);
                                if (matchableAtPos.Variant.color == matchable.Variant.color)
                                {
                                    ColorExplodeFX fxObj = ColorExplodeFXPool.Instance.GetObject();
                                    fxObj.transform.position = matchablePos;
                                    fxObj.PlayFX(matchableAtPos.transform.position);
                                    matchableAtPos.SetVariant(_pool.GetVariant(matchable.Variant.color, type));
                                    horizontalsToTrigger.Add(matchableAtPos);

                                }
                            }
                        }
                    }
                    yield return new WaitForSeconds(0.5f);
                    foreach (Matchable matchableToTrigger in horizontalsToTrigger)
                    {
                        RemoveItemAt(matchableToTrigger.GridPosition);
                        _pool.ReturnObject(matchableToTrigger);
                        StartCoroutine(TriggerHorizontalExplode(matchableToTrigger, null, false));
                    }
                    break;
                case MatchableType.VerticalExplode:
                    List<Matchable> verticalsToTrigger = new List<Matchable>();
                    for (int x = 0; x < Dimensions.x; x++)
                    {
                        for (int y = 0; y < Dimensions.y; y++)
                        {
                            if (!IsEmpty(x, y))
                            {
                                Matchable matchableAtPos = GetItemAt(x, y);
                                if (matchableAtPos.Variant.color == matchable.Variant.color)
                                {
                                    ColorExplodeFX fxObj = ColorExplodeFXPool.Instance.GetObject();
                                    fxObj.transform.position = matchablePos;
                                    fxObj.PlayFX(matchableAtPos.transform.position);
                                    matchableAtPos.SetVariant(_pool.GetVariant(matchable.Variant.color, type));
                                    verticalsToTrigger.Add(matchableAtPos);

                                }
                            }
                        }
                    }
                    yield return new WaitForSeconds(_colorExplodeStartDelay);
                    foreach (Matchable matchableToTrigger in verticalsToTrigger)
                    {
                        StartCoroutine(TriggerVerticalExplode(matchableToTrigger, null));
                        yield return new WaitForSeconds(_colorExplodeDelays);
                        //Matchable check for bugs
                        if (!IsEmpty(matchableToTrigger.GridPosition) && GetItemAt(matchableToTrigger.GridPosition) == matchableToTrigger)
                        {
                            RemoveItemAt(matchableToTrigger.GridPosition);
                            _pool.ReturnObject(matchableToTrigger);
                            columnCoroutines[matchableToTrigger.GridPosition.x] = StartCoroutine(CollapseRepopulateAndScanColumn(matchableToTrigger.GridPosition.x));
                        }
                    }
                    break;
                case MatchableType.AreaExplode:
                    List<Matchable> areaExplodesToTrigger = new List<Matchable>();
                    for (int x = 0; x < Dimensions.x; x++)
                    {
                        for (int y = 0; y < Dimensions.y; y++)
                        {
                            if (!IsEmpty(x, y) && !GetItemAt(x, y).isSwapping && !GetItemAt(x, y).IsMoving)
                            {
                                Matchable matchableAtPos = GetItemAt(x, y);
                                if (matchableAtPos.Variant.color == matchable.Variant.color)
                                {
                                    ColorExplodeFX fxObj = ColorExplodeFXPool.Instance.GetObject();
                                    fxObj.transform.position = matchablePos;
                                    fxObj.PlayFX(matchableAtPos.transform.position);
                                    matchableAtPos.SetVariant(_pool.GetVariant(matchable.Variant.color, type));
                                    areaExplodesToTrigger.Add(matchableAtPos);
                                }
                            }
                        }
                    }
                    yield return new WaitForSeconds(_colorExplodeStartDelay);
                    foreach (Matchable matchableToTrigger in areaExplodesToTrigger)
                    {
                        TriggerAreaExplode(matchableToTrigger, null, true);
                    }
                    break;
                case MatchableType.ColorExplode:
                    for (int x = 0; x < Dimensions.x; x++)
                    {
                        for (int y = 0; y < Dimensions.y; y++)
                        {
                            Matchable matchableAtPos = GetItemAt(x, y);
                            if (matchableAtPos == null) continue;
                            ColorExplodeFX fxObj = ColorExplodeFXPool.Instance.GetObject();
                            fxObj.transform.position = matchablePos;
                            fxObj.PlayFX(matchableAtPos.transform.position);
                            RemoveItemAt(matchableAtPos.GridPosition);
                            _pool.ReturnObject(matchableAtPos);
                            columnCoroutines[matchableAtPos.GridPosition.x] = StartCoroutine(CollapseRepopulateAndScanColumn(matchableAtPos.GridPosition.x));
                            yield return new WaitForSeconds(0.02f);
                        }
                    }
                    break;
                default:
                    break;
            }
            
            yield return null;
        }
        public IEnumerator TriggerHorizontalExplode(Matchable horizontalMatchable, Match match, bool removeOrigin = false)
        {
            MatchableFX fxObj = MatchableFXPool.Instance.GetObject();
            fxObj.PlayFX(MatchableType.HorizontalExplode);
            fxObj.transform.position = horizontalMatchable.transform.position;
            int y = horizontalMatchable.GridPosition.y;
            int horizontalX = horizontalMatchable.GridPosition.x;
            yield return new WaitForSeconds(_horizontalVerticalExplodeDelays);

            int x2 = horizontalX;
            int x1 = horizontalX;
            if(removeOrigin)
            {
                RemoveItemAt(horizontalMatchable.GridPosition);
                _pool.ReturnObject(horizontalMatchable);
                columnCoroutines[horizontalX] = StartCoroutine(CollapseRepopulateAndScanColumn(horizontalX));
            }
            while (x1 >= 0 || x2 < Dimensions.x)
            {
                x2++;
                if (x2 < Dimensions.x && !IsEmpty(x2, y))
                {
                    Matchable matchable2 = GetItemAt(x2, y);
                    if(match == null || !match.MatchableList.Contains(matchable2))
                    {
                        if (!matchable2.isSwapping && !matchable2.IsMoving)
                        {
                            if (matchable2.Variant.type == MatchableType.AreaExplode)
                            {
                                TriggerAreaExplode(matchable2, match, true);
                                columnCoroutines[x2] = StartCoroutine(CollapseRepopulateAndScanColumn(x2));
                            }
                            else if (matchable2.Variant.type == MatchableType.VerticalExplode)
                            {
                                StartCoroutine(TriggerVerticalExplode(matchable2, match));
                                RemoveItemAt(matchable2.GridPosition);
                                _pool.ReturnObject(matchable2);
                                columnCoroutines[x2] = StartCoroutine(CollapseRepopulateAndScanColumn(x2));
                            }
                            else
                            {
                                RemoveItemAt(matchable2.GridPosition);
                                _pool.ReturnObject(matchable2);
                                columnCoroutines[x2] = StartCoroutine(CollapseRepopulateAndScanColumn(x2));
                            }
                        }
                    }
                }
                //if (!CheckBounds(x, y))
                //    continue;
                x1--;
                if (x1 >= 0 && !IsEmpty(x1, y))
                {
                    Matchable matchable = GetItemAt(x1, y);
                    if (match == null || !match.MatchableList.Contains(matchable))
                    {
                        if (!matchable.isSwapping && !matchable.IsMoving)
                        {
                            if (matchable.Variant.type == MatchableType.AreaExplode)
                            {
                                TriggerAreaExplode(matchable, match, true);
                                columnCoroutines[x1] = StartCoroutine(CollapseRepopulateAndScanColumn(x1));
                            }
                            else if (matchable.Variant.type == MatchableType.VerticalExplode)
                            {
                                StartCoroutine(TriggerVerticalExplode(matchable, match));
                                RemoveItemAt(matchable.GridPosition);
                                _pool.ReturnObject(matchable);
                                columnCoroutines[x1] = StartCoroutine(CollapseRepopulateAndScanColumn(x1));
                            }
                            else
                            {
                                RemoveItemAt(matchable.GridPosition);
                                _pool.ReturnObject(matchable);
                                columnCoroutines[x1] = StartCoroutine(CollapseRepopulateAndScanColumn(x1));
                            }
                        }
                    }
                }
                yield return new WaitForSeconds(_horizontalVerticalExplodeDelays);
            }

        }
        public IEnumerator TriggerVerticalExplode(Matchable verticalMatchable, Match match, bool removeOrigin = false)
        {
            MatchableFX fxObj = MatchableFXPool.Instance.GetObject();
            fxObj.PlayFX(MatchableType.VerticalExplode);
            fxObj.transform.position = verticalMatchable.transform.position;
            int x = verticalMatchable.GridPosition.x;

            if(_lockedTriggerColumns.Contains(x))
            {
                yield break;
            }

            _lockedTriggerColumns.Add(x);
            if (_lockedColumns.Contains(x))
            {
                if(columnCoroutines[x] != null)
                {
                    StopCoroutine(columnCoroutines[x]);
                }
            }
            else
            {
                _lockedColumns.Add(x);
            }

            int y1 = verticalMatchable.GridPosition.y;
            int y2 = verticalMatchable.GridPosition.y;

            if(removeOrigin)
            {
                RemoveItemAt(verticalMatchable.GridPosition);
                _pool.ReturnObject(verticalMatchable);
            }

            yield return new WaitForSeconds(_horizontalVerticalExplodeDelays);

            while (y2 < Dimensions.y || y1 >= 0)
            {
                y2++;
                if (y2 < Dimensions.y && !IsEmpty(x, y2))
                {
                    Matchable matchable2 = GetItemAt(x, y2);
                    if (match == null || !match.MatchableList.Contains(matchable2))
                    {
                        if (!matchable2.isSwapping && !matchable2.IsMoving)
                        {
                            if (matchable2.Variant.type == MatchableType.AreaExplode)
                            {
                                TriggerAreaExplode(matchable2, match);
                            }
                            else if (matchable2.Variant.type == MatchableType.HorizontalExplode)
                            {
                                StartCoroutine(TriggerHorizontalExplode(matchable2, match));
                                RemoveItemAt(matchable2.GridPosition);
                                _pool.ReturnObject(matchable2);
                            }
                            else
                            {
                                RemoveItemAt(matchable2.GridPosition);
                                _pool.ReturnObject(matchable2);
                            }
                        }
                    }
                }
                y1--;
                //if (!CheckBounds(x, y1))
                //    continue;
                if (y1 >= 0 && !IsEmpty(x, y1))
                {
                    Matchable matchable = GetItemAt(x, y1);
                    if(match == null || !match.MatchableList.Contains(matchable))
                    {
                        if (!matchable.isSwapping && !matchable.IsMoving)
                        {
                            if (matchable.Variant.type == MatchableType.AreaExplode)
                            {
                                TriggerAreaExplode(matchable, match);
                            }
                            else if (matchable.Variant.type == MatchableType.HorizontalExplode)
                            {
                                StartCoroutine(TriggerHorizontalExplode(matchable, match));
                                RemoveItemAt(matchable.GridPosition);
                                _pool.ReturnObject(matchable);
                            }
                            else
                            {
                                RemoveItemAt(matchable.GridPosition);
                                _pool.ReturnObject(matchable);
                            }
                        }
                    }
                }
                yield return new WaitForSeconds(_horizontalVerticalExplodeDelays);
                yield return null;
            }
            if (_lockedColumns.Contains(x))
                _lockedColumns.Remove(x);
            _lockedTriggerColumns.Remove(x);
            columnCoroutines[x] = StartCoroutine(CollapseRepopulateAndScanColumn(x));
        }
        public void TriggerAreaExplode(Matchable bombMatchable, Match match, bool checkLockedVerticalTrigger = false)
        {
            int matchableX = bombMatchable.GridPosition.x;
            int matchableY = bombMatchable.GridPosition.y;

            for (int x = matchableX - 1; x <= matchableX + 1; x++)
            {
                if (x >= Dimensions.x || x < 0) continue;
                for (int y = matchableY - 1; y <= matchableY + 1; y++)
                {
                    if (!CheckBounds(x, y) || IsEmpty(x, y))
                        continue;

                    Matchable matchable = GetItemAt(x, y);

                    if ((match != null && match.MatchableList.Contains(matchable)) || matchable.IsMoving) 
                        continue;

                    if(matchable.Variant.type == MatchableType.HorizontalExplode)
                    {
                        RemoveItemAt(matchable.GridPosition);
                        _pool.ReturnObject(matchable);
                        StartCoroutine(TriggerHorizontalExplode(matchable, match));
                    }
                    else if(matchable.Variant.type == MatchableType.VerticalExplode)
                    {
                        RemoveItemAt(matchable.GridPosition);
                        _pool.ReturnObject(matchable);
                        StartCoroutine(TriggerVerticalExplode(matchable, match));
                    }
                    else if(matchable.Variant.type == MatchableType.AreaExplode)
                    {
                        RemoveItemAt(matchable.GridPosition);
                        _pool.ReturnObject(matchable);
                        TriggerAreaExplode(matchable, match);
                    }
                    else
                    {
                        RemoveItemAt(matchable.GridPosition);
                        _pool.ReturnObject(matchable);
                    }
                }
                if(x != matchableX)
                {
                    if(checkLockedVerticalTrigger)
                    { 
                        if(!_lockedTriggerColumns.Contains(matchableX) && !_lockedColumns.Contains(matchableX))
                        {
                            columnCoroutines[x] = StartCoroutine(CollapseRepopulateAndScanColumn(x));
                        }
                    }
                    else
                    {
                        columnCoroutines[x] = StartCoroutine(CollapseRepopulateAndScanColumn(x));
                    }
                }
            }
        }
        private IEnumerator TriggerAreaStripedCombo(Matchable areaMatchable, Matchable matchable2)
        {
            int posX = areaMatchable.GridPosition.x;
            int posY = areaMatchable.GridPosition.y;

            //RemoveItemAt(areaMatchable.GridPosition);
            //_pool.ReturnObject(areaMatchable);

            Coroutine horizontalCoroutine = null;
            for (int y = posY - 1; y <= posY + 1; y++)
            {
                if (CheckBounds(posX, y))
                    horizontalCoroutine = StartCoroutine(TriggerHorizontalExplode(GetItemAt(posX, y), null));
            }
            Vector3 fxPos = matchable2.transform.position;
            RemoveItemAt(matchable2.GridPosition);
            _pool.ReturnObject(matchable2);
            RemoveItemAt(areaMatchable.GridPosition);
            _pool.ReturnObject(areaMatchable);
            yield return horizontalCoroutine;
            yield return new WaitForSeconds(0.5f);

            for (int x = posX - 1; x <= posX + 1; x++)
            {
                fxPos.x = x * _spacing.x;
                if (CheckBounds(x, posY))
                    StartCoroutine(TriggerVerticalExplode(fxPos, new Vector2Int(x, posY), null));
            }
        }
        public IEnumerator TriggerVerticalExplode(Vector3 fxTransform, Vector2Int pos, Match match, bool removeOrigin = false)
        {
            MatchableFX fxObj = MatchableFXPool.Instance.GetObject();
            fxObj.PlayFX(MatchableType.VerticalExplode);
            fxObj.transform.position = fxTransform;
            int x = pos.x;

            if (_lockedTriggerColumns.Contains(x))
            {
                yield break;
            }

            _lockedTriggerColumns.Add(x);
            if (_lockedColumns.Contains(x))
            {
                if (columnCoroutines[x] != null)
                {
                    StopCoroutine(columnCoroutines[x]);
                }
            }
            else
            {
                _lockedColumns.Add(x);
            }

            int y1 = pos.y;
            int y2 = pos.y;

            yield return new WaitForSeconds(_horizontalVerticalExplodeDelays);

            while (y2 < Dimensions.y || y1 >= 0)
            {
                y2++;
                if (y2 < Dimensions.y && !IsEmpty(x, y2))
                {
                    Matchable matchable2 = GetItemAt(x, y2);
                    if (match == null || !match.MatchableList.Contains(matchable2))
                    {
                        if (!matchable2.isSwapping && !matchable2.IsMoving)
                        {
                            if (matchable2.Variant.type == MatchableType.AreaExplode)
                            {
                                TriggerAreaExplode(matchable2, match);
                            }
                            else if (matchable2.Variant.type == MatchableType.HorizontalExplode)
                            {
                                StartCoroutine(TriggerHorizontalExplode(matchable2, match));
                                RemoveItemAt(matchable2.GridPosition);
                                _pool.ReturnObject(matchable2);
                            }
                            else
                            {
                                RemoveItemAt(matchable2.GridPosition);
                                _pool.ReturnObject(matchable2);
                            }
                        }
                    }
                }
                y1--;
                //if (!CheckBounds(x, y1))
                //    continue;
                if (y1 >= 0 && !IsEmpty(x, y1))
                {
                    Matchable matchable = GetItemAt(x, y1);
                    if (match == null || !match.MatchableList.Contains(matchable))
                    {
                        if (!matchable.isSwapping && !matchable.IsMoving)
                        {
                            if (matchable.Variant.type == MatchableType.AreaExplode)
                            {
                                TriggerAreaExplode(matchable, match);
                            }
                            else if (matchable.Variant.type == MatchableType.HorizontalExplode)
                            {
                                StartCoroutine(TriggerHorizontalExplode(matchable, match));
                                RemoveItemAt(matchable.GridPosition);
                                _pool.ReturnObject(matchable);
                            }
                            else
                            {
                                RemoveItemAt(matchable.GridPosition);
                                _pool.ReturnObject(matchable);
                            }
                        }
                    }
                }
                yield return new WaitForSeconds(_horizontalVerticalExplodeDelays);
                yield return null;
            }
            if (_lockedColumns.Contains(x))
                _lockedColumns.Remove(x);
            _lockedTriggerColumns.Remove(x);
            columnCoroutines[x] = StartCoroutine(CollapseRepopulateAndScanColumn(x));
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

