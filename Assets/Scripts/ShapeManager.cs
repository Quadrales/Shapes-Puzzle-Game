using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class ShapeManager : MonoBehaviour
{
    // Prefabs, shape start positions, and grid manager
    [SerializeField] private List<GameObject> _shapePrefabs;
    [SerializeField] private List<Vector2Int> _startingPositions;
    [SerializeField] private List<GameObject> _ghostShapePrefabs;
    [SerializeField] private List<Vector2Int> _ghostStartingPositions;
    [SerializeField] GridManager _gridManager;

    // Shape related fields
    private List<Shape> _shapes = new List<Shape>();
    private List<Shape> _ghostShapes = new List<Shape>();
    private List<Shape> _completedShapes = new List<Shape>();
    private int _smallestEdgeCount = 1;

    public bool PuzzleComplete { get; set; } = false;

    // Movement related fields
    public InputAction shapeMovement;
    [SerializeField] private float moveCooldown = 0.3f;
    private float moveTimer;
    private int _moveCount = 0;

    public void HandleShapeMovement()
    {
        // fix move cooldown, still doesn't work (probably bc there is no use of OnEnable or OnDisable)
        moveTimer -= Time.deltaTime;

        Vector2 moveInput = shapeMovement.ReadValue<Vector2>();
        Vector2Int moveDirection = new Vector2Int(Mathf.RoundToInt(moveInput.x), Mathf.RoundToInt(moveInput.y));

        if (moveDirection != Vector2Int.zero && moveTimer <= 0f)
        {
            _moveCount += _smallestEdgeCount;
            Debug.Log("Move count: " + _moveCount);
            Debug.Log("Smallest edge count: " + _smallestEdgeCount);
            MoveShapes(moveDirection, _smallestEdgeCount);
            moveTimer = moveCooldown;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        InstantiateShapes(_shapePrefabs, _startingPositions, _shapes);
        InstantiateShapes(_ghostShapePrefabs, _ghostStartingPositions, _ghostShapes);
    }

    private void InstantiateShapes(List<GameObject> prefabs, List<Vector2Int> startPositions, List<Shape> shapes)
    {
        if (prefabs.Count != startPositions.Count)
        {
            Debug.LogError("Shapes and Starting Positions count do not match");
            return;
        }

        for (int i = 0; i < prefabs.Count; i++)
        {
            // Instantiate shape prefab
            var shapeInstance = Instantiate(prefabs[i]);
            Shape shapeComponent = shapeInstance.GetComponent<Shape>();
            var startPosition = startPositions[i];

            if (shapeComponent != null)
            {
                // Ensure the shape position is within grid bounds
                startPosition.x = Mathf.Clamp(startPosition.x, 0, _gridManager.Width - 1);
                startPosition.y = Mathf.Clamp(startPosition.y, 0, _gridManager.Height - 1);

                // Setting the shape position
                shapeComponent.GridPosition = startPositions[i];
                shapeInstance.transform.position = new Vector3(shapeComponent.GridPosition.x, shapeComponent.GridPosition.y, 0);

                shapes.Add(shapeComponent);
                Debug.Log($"{shapeComponent.name} initialized at {shapeComponent.GridPosition}");
            }
            else
            {
                Debug.LogError($"Shape component missing on prefab: {prefabs[i].name}");
            }
        }
    }

    private void OnEnable()
    {
        shapeMovement.Enable();
    }

    private void OnDisable()
    {
        shapeMovement.Disable();
    }

    private void MoveShapes(Vector2Int direction, int minEdgeCount)
    {
        HashSet<Vector2Int> occupiedPositions = new HashSet<Vector2Int>();

        // Populate set of occupied positions with shapes that won't move
        foreach (Shape shape in _shapes)
        {
            if ((shape.EdgeCount != 1) && (_moveCount % shape.EdgeCount != 0))
            {
                occupiedPositions.Add(shape.GridPosition);
            }
        }

        foreach (Shape shape in _shapes)
        {
            // Only move shapes that have not been completed
            if (!_completedShapes.Contains(shape))
            {
                if (ShapeShouldMove(shape, minEdgeCount))
                {
                    Vector2Int currentPosition = shape.GridPosition;

                    int maxX = _gridManager.Width - 1;
                    int maxY = _gridManager.Height - 1;

                    // Calculate new grid position, ensuring a wrap-around grid
                    Vector2Int newPosition = (direction.x, direction.y, currentPosition.x, currentPosition.y) switch
                    {
                        (0, 1, _, var y) when y == maxY => new Vector2Int(currentPosition.x, 0), // Up
                        (0, -1, _, 0) => new Vector2Int(currentPosition.x, maxY), // Down
                        (-1, 0, 0, _) => new Vector2Int(maxX, currentPosition.y), // Left
                        (1, 0, var x, _) when x == maxX => new Vector2Int(0, currentPosition.y), // Right
                        _ => currentPosition + direction
                    };

                    // Ensure shapes stay within bounds and position isn't occupied
                    if ((newPosition.x >= 0 && newPosition.x < _gridManager.Width) &&
                        (newPosition.y >= 0 && newPosition.y < _gridManager.Height) &&
                            (!occupiedPositions.Contains(newPosition)))
                    {
                        // Update shape position
                        shape.GridPosition = newPosition;
                        shape.transform.position = new Vector3(newPosition.x, newPosition.y, 0);
                    }
                    else
                    {
                        Debug.Log($"Shape {shape.name} blocked from moving to {newPosition}");
                        occupiedPositions.Add(shape.GridPosition);
                    }

                    // Complete shape if moved to respective ghost shape
                    CheckShapeCompletion(shape);

                    if (CheckPuzzleCompletion())
                    {
                        PuzzleComplete = true;
                    }
                }
                else
                {
                    // Shape doesn't move, so mark current position as occupied
                    occupiedPositions.Add(shape.GridPosition);
                }
            }
        }
    }

    private bool ShapeShouldMove(Shape shape, int minEdgeCount)
    {
        if (shape.EdgeCount == 1) return true;

        // Only move if current or skipped move count is divisible by this shape's edge count
        for (int i = 0; i < minEdgeCount; i++)
        {
            if ((_moveCount - i) % shape.EdgeCount == 0)
            {
                return true;
            }
        }

        return false;
    }

    private bool CheckPuzzleCompletion()
    {
        foreach (Shape shape in _shapes)
        {
            if (!_completedShapes.Contains(shape))
            {
                return false;
            }
        }

        return true;
    }

    private void CheckShapeCompletion(Shape shape)
    {
        foreach (var ghostShape in _ghostShapes)
        {
            if ((shape.EdgeCount == ghostShape.EdgeCount) &&
                (shape.GridPosition.Equals(ghostShape.GridPosition)))
            {
                _completedShapes.Add(shape);
                _smallestEdgeCount = FindSmallestEdgeCount();
            }
        }
    }

    private int FindSmallestEdgeCount()
    {
        foreach (Shape shape in _shapes)
        {
            if (!_completedShapes.Contains(shape))
            {
                return shape.EdgeCount;
            }
        }

        return 1;
    }
}
