using System;
using TMPro;
using UnityEngine;
using System.Collections;
using Tools;
namespace Core
{
    public class GameManager : SingletonMonoBehaviour<GameManager>
    {
        [SerializeField] private Vector2Int _dimensions;
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
        protected override void Awake()
        {
            base.Awake();
            _grid = (MatchableGrid) MatchableGrid.Instance;
            _grid.InitializeGrid(_dimensions);
            _grid.PopulateGrid();
        }
    }
}


