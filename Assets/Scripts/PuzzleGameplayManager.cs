using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PuzzleGameplayManager : MonoBehaviour
{
    // Prefabs, shape start positions, and grid manager
    [SerializeField] private List<GameObject> _shapePrefabs;
    [SerializeField] private List<GameObject> _ghostShapePrefabs;
    [SerializeField] GridManager _gridManager;
    [SerializeField] PuzzleTextManager _puzzleTextManager;

    // Shape related fields
    private List<Shape> _shapes = new List<Shape>();
    private List<Shape> _ghostShapes = new List<Shape>();
    private List<Shape> _completedShapes = new List<Shape>();
    private int _smallestEdgeCount = 1;

    public bool PuzzleComplete { get; set; } = false;
    // add PuzzleFailed field?

    // Movement related fields
    public InputAction shapeMovement;
    [SerializeField] private float moveCooldown = 0.3f; // Default cooldown of 0.3s
    private float moveTimer;
    private int _moveCount = 0;
    private int _moveLimit = 0;

    public void LoadCurrentPuzzle(PuzzleData currentPuzzle)
    {
        _moveLimit = currentPuzzle.MoveLimit;

        _puzzleTextManager.InstantiateTextUI(_moveLimit);

        InstantiateShapes(_shapePrefabs, currentPuzzle.ShapeStartingPositions, _shapes);
        InstantiateShapes(_ghostShapePrefabs, currentPuzzle.GhostShapeStartingPositions, _ghostShapes);
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

    private void Update()
    {
        // might be better to put this in FixedUpdate but I'm not too sure
        if (!PuzzleComplete && _moveCount < _moveLimit)
        {
            HandleShapeMovement();
        }

        // handle winning/losing (if lost, either reset immediately or put up a fail screen then ask to reset)
    }

    public void HandleShapeMovement()
    {
        moveTimer -= Time.deltaTime;

        Vector2 moveInput = shapeMovement.ReadValue<Vector2>();

        int x = Mathf.RoundToInt(moveInput.x);
        int y = Mathf.RoundToInt(moveInput.y);

        // Prevent diagonal movement by taking larger input vector value (horizontal or vertical)
        if (x != 0 && y != 0)
        {
            // Prioritising horizontal movement
            if (Mathf.Abs(moveInput.y) > Mathf.Abs(moveInput.x))
            {
                x = 0;
            }
            else
            {
                y = 0;
            }
        }

        Vector2Int moveDirection = new Vector2Int(x, y);

        if (moveDirection != Vector2Int.zero && moveTimer <= 0f)
        {
            _moveCount += _smallestEdgeCount;
            _puzzleTextManager.UpdateMoveCountText(_moveCount);

            MoveShapes(moveDirection, _smallestEdgeCount);
            moveTimer = moveCooldown;
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
                // Check if shape should move
                int moveCountDecrement = ShapeShouldMove(shape, minEdgeCount);
                if (moveCountDecrement != -1)
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

                    // Complete puzzle if all shapes in correct positions
                    if (CheckPuzzleCompletion())
                    {
                        // Subtract excess moves when shape with larger edge count than moves needed completes puzzle
                        _moveCount -= moveCountDecrement;
                        _puzzleTextManager.UpdateMoveCountText(_moveCount);

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

    // Returns -1 if false, otherwise returns possible move decrement if shape completes puzzle
    private int ShapeShouldMove(Shape shape, int minEdgeCount)
    {
        // If shape is circle, there should be no count decrement
        if (shape.EdgeCount == 1) return 0;

        // Only move if current or skipped move count is divisible by this shape's edge count
        for (int i = 0; i < minEdgeCount; i++)
        {
            if ((_moveCount - i) % shape.EdgeCount == 0)
            {
                return i; // Decrement of move count post-completion of puzzle
            }
        }

        return -1; // Don't move
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

        if (_moveCount > _moveLimit)
        {
            return false;
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
