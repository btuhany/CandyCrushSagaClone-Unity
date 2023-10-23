using System;
using TMPro;
using UnityEngine;
using System.Collections;
using Tools;
namespace Core
{
    public class GameManager : SingletonMonoBehaviour<GameManager>
    {
        [SerializeField] private int _maxAllowedMove = 40;
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private TextMeshProUGUI _moveText;
        [SerializeField] private Vector2Int _dimensions;
        private int _score;
        private MatchableGrid _grid;
        [ContextMenu("ClearAndPopulate")]
        private void ClearAndPopulate()
        {
            _grid.ClearGrid();
            _grid.PopulateGrid();
        }
        [ContextMenu("Populate")]
        private void Populate()
        {
            _grid.PopulateGrid();
        }
        [ContextMenu("ClearGrid")]
        private void ClearGrid()
        {
            _grid.ClearGrid();
        }
        [ContextMenu("Test")]
        private void Testt()
        {
            MatchableFX fxx = MatchableFXPool.Instance.GetObject();
            fxx.transform.position = _grid.GetItemAt(0, 0).transform.position;
            fxx.PlayColorExplode(_grid.GetItemAt(9, 9).transform);
        }
        protected override void Awake()
        {
            base.Awake();
            _grid = (MatchableGrid) MatchableGrid.Instance;
            _grid.InitializeGrid(_dimensions);
            _grid.PopulateGrid();
            _scoreText.text = _score.ToString("D5");
            _moveText.text = _maxAllowedMove.ToString();
        }
        public void IncreaseScore(int value)
        {
            _score += value;
            _scoreText.text = _score.ToString("D5");
        }
        public bool CanMoveMatchables()
        {
            if (_maxAllowedMove <= 0)
                return false;
            return true;
        }
        public void DecreaseMove()
        {
            _maxAllowedMove--;
            _moveText.text = _maxAllowedMove.ToString();
        }
    }
}


