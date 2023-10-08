using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

namespace Tools
{
    public abstract class GridSystem<T> : SingletonMonoBehaviour<GridSystem<T>>
    {
        private T[,] _data;
        private Vector2Int _dimensions = new Vector2Int(1, 1);
        private bool _isReady;
        public Vector2Int Dimensions { get => _dimensions; }
        public bool IsReady { get => _isReady; }
        public void InitializeGrid(Vector2Int dimensions)
        {
            _dimensions = dimensions;
            _data = new T[_dimensions.x, _dimensions.y];
            _isReady = true;
        }
        public virtual void ClearGrid()
        {
            if (!_isReady)
                Debug.LogWarning("Grid has not been initializd.");

            _data = new T[_dimensions.x, _dimensions.y];
            //for (int i = 0; i < _dimensions.x; i++)
            //{
            //    for (int j = 0; j < _dimensions.y; j++)
            //    {
            //        _data[i, j] = default(T);
            //    }
            //}
        }
        public bool CheckBounds(int x, int y)
        {
            if (!_isReady)
                Debug.LogError("Grid has not been initializd.");

            return x >= 0 && y >= 0 && x < _dimensions.x && y < _dimensions.y;
        }
        public bool CheckBounds(Vector2Int position)
        {
            return CheckBounds(position.x, position.y);
        }
        public bool IsEmpty(int x, int y)
        {
            if (!CheckBounds(x, y))
                Debug.LogError($"({x},{y}) are not on the grid.");

            //return _data[x, y] == null;
            return EqualityComparer<T>.Default.Equals(_data[x, y], default(T));

        }
        public bool IsEmpty(Vector2Int position)
        {
            return IsEmpty(position.x, position.y);
        }
        public bool PutItemAt(T item, int x, int y, bool allowOverwrite = false)
        {
            if (!CheckBounds(x, y))
                Debug.LogError($"({x},{y}) are not on the grid.");

            if (!allowOverwrite && !IsEmpty(x, y))
                return false;

            _data[x, y] = item;
            return true;
        }
        public bool PutItemAt(T item, Vector2Int position, bool allowOverwrite = false)
        {
            return PutItemAt(item, position.x, position.y, allowOverwrite);
        }
        public T GetItemAt(int x, int y)
        {
            if (!CheckBounds(x, y))
                Debug.LogError($"({x},{y}) are not on the grid.");

            return _data[x, y];
        }
        public T GetItemAt(Vector2Int position)
        {
            return GetItemAt(position.x, position.y);
        }
        public T RemoveItemAt(int x, int y)
        {
            if (!CheckBounds(x, y))
                Debug.LogError($"({x},{y}) are not on the grid.");

            T item = _data[x, y];
            _data[x, y] = default(T);
            return item;
        }
        public T RemoveItemAt(Vector2Int posiiton)
        {
            return RemoveItemAt(posiiton.x, posiiton.y);
        }
        protected bool MoveItemTo(int itemPosX, int itemPosY, int x, int y, bool allowOverwrite = false)
        {
            if (!CheckBounds(x, y))
                Debug.LogError($"({x},{y}) are not on the grid.");
            if (!CheckBounds(itemPosX, itemPosY))
                Debug.LogError($"({x},{y}) are not on the grid.");

            if (!allowOverwrite && !IsEmpty(x, y))
                return false;

            _data[x, y] = RemoveItemAt(itemPosX, itemPosY);
            return true;
        }
        protected bool MoveItemTo(Vector2Int itemPos, Vector2Int newPos, bool allowOverwrite)
        {
            return MoveItemTo(itemPos.x, itemPos.y, newPos.x, newPos.y, allowOverwrite);
        }
        protected void SwapItems(int x1, int y1, int x2, int y2)
        {
            if (!CheckBounds(x1, y1))
                Debug.LogError($"({x1},{y1}) are not on the grid.");
            if (!CheckBounds(x2, y2))
                Debug.LogError($"({x2},{y2}) are not on the grid.");

            T item1 = _data[x1, y1];
            _data[x1, y1] = _data[x2, y2];
            _data[x2, y2] = item1;
        }
        protected void SwapItems(Vector2Int position1, Vector2Int position2)
        {
            SwapItems(position1.x, position1.y, position2.x, position2.y);
        }

        private bool IsItemInGrid(T item, out Vector2Int position)
        {
            for (int x = 0; x < _dimensions.x; x++)
            {
                for (int y = 0; y < _dimensions.y; y++)
                {
                    if (EqualityComparer<T>.Default.Equals(_data[x, y], item))
                    {
                        position = new Vector2Int(x, y);
                        return true;
                    }
                }
            }
            position = new Vector2Int(0, 0);
            return false;
        }
        public override string ToString()
        {
            string s = "";

            for (int i = _dimensions.y - 1; i >= 0; i--)
            {
                s += "[ ";
                for (int j = 0; j < _dimensions.x; j++)
                {
                    if (IsEmpty(j, i))
                    {
                        s += "x";
                    }
                    else
                    {
                        s += _data[j, i].ToString();
                    }
                    if (j != _dimensions.x - 1)
                        s += ", ";
                }
                s += " ]\n";
            }

            return s;
        }
    }

}
