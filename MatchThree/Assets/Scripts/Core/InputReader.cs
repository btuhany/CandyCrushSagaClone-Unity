using Core;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputReader : MonoBehaviour, InputActions.ITouchscreenActions
{
    [SerializeField] Camera _mainCam;
    private InputActions _controls;
    private Vector2 _touchPos;
    private Matchable[] _selectedMatchables = new Matchable[2];
    private MatchableGrid _grid;
    private void Awake()
    {
        _controls = new InputActions();
        _controls.Touchscreen.SetCallbacks(this);
        _controls.Touchscreen.Enable();
        _grid = (MatchableGrid) MatchableGrid.Instance;
    }
    private void OnDisable()
    {
        _controls.Touchscreen.Disable();
    }
    public void OnTouchPosition(InputAction.CallbackContext context)
    {
        if (context.performed)
            _touchPos = context.ReadValue<Vector2>();
    }

    public void OnTouchPress(InputAction.CallbackContext context)
    {
        if (context.performed)
            HandleOnTouchedDown();
        if (context.canceled)
            HandleOnTouchedUp();
    }
    private void HandleOnTouchedDown()
    {
        Vector2 worldPoint = _mainCam.ScreenToWorldPoint(_touchPos);
        RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);
        if (hit.collider != null)
        {
            if (hit.collider.TryGetComponent(out Matchable matchable))
            {
                if(!matchable.IsMoving)
                {
                    _selectedMatchables[0] = matchable;
                    _selectedMatchables[0].GetSelected();
                }
            }
        }
    }
    private void HandleOnTouchedUp()
    {
        Vector2 worldPoint = _mainCam.ScreenToWorldPoint(_touchPos);
        RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);
        
        if (hit.collider != null)
        {
            if (hit.collider.TryGetComponent(out Matchable matchable))
            {
                if(matchable != _selectedMatchables[0] && !matchable.IsMoving)
                    _selectedMatchables[1] = matchable;
            }
        }

        if (_selectedMatchables[0] != null && _selectedMatchables[1] != null)
        {
            if (_grid.AreAdjacents(_selectedMatchables[0], _selectedMatchables[1]))
            {
                //Debug.Log("First: " + _selectedMatchables[0].GridPosition + "Second: " + _selectedMatchables[1].GridPosition);
                StartCoroutine(_grid.TryMatch(_selectedMatchables[0], _selectedMatchables[1]));
            }
            else
            {
                int x = _selectedMatchables[0].GridPosition.x;
                int y = _selectedMatchables[0].GridPosition.y;

                int selectedX = _selectedMatchables[1].GridPosition.x;
                int selectedY = _selectedMatchables[1].GridPosition.y;

                if (selectedX > x)
                {
                    if (selectedY == y)
                    {
                        StartCoroutine(_grid.TryMatch(_selectedMatchables[0], _grid.GetItemAt(x + 1, selectedY)));
                    }
                }
                else if(selectedX < x)
                {
                    if (selectedY == y)
                    {
                        StartCoroutine(_grid.TryMatch(_selectedMatchables[0], _grid.GetItemAt(x - 1, selectedY)));
                    }
                }
                else
                {
                    if (selectedY > y)
                    {
                        StartCoroutine(_grid.TryMatch(_selectedMatchables[0], _grid.GetItemAt(x, y + 1)));
                    }
                    else
                    {
                        StartCoroutine(_grid.TryMatch(_selectedMatchables[0], _grid.GetItemAt(x, y - 1)));
                    }
                }
            }
        }
        _selectedMatchables[0]?.GetUnselected();
        _selectedMatchables[0] = _selectedMatchables[1] = null;
    }
}
