using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShapeManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> _shapePrefabs;
    [SerializeField] private List<Vector2Int> _startingPositions;
    [SerializeField] private List<GameObject> _ghostShapePrefabs;
    [SerializeField] private List<Vector2Int> _ghostStartingPositions;
    [SerializeField] GridManager _gridManager;

    private List<Shape> _shapes = new List<Shape>();
    private List<Shape> _ghostShapes = new List<Shape>();
    private int _moveCount = 0;

    public InputAction shapeMovement;

    [SerializeField] private float moveCooldown = 0.3f;
    private float moveTimer;

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

    // Update is called once per frame
    void Update()
    {
        moveTimer -= Time.deltaTime;

        Vector2 moveInput = shapeMovement.ReadValue<Vector2>();
        Vector2Int moveDirection = new Vector2Int(Mathf.RoundToInt(moveInput.x), Mathf.RoundToInt(moveInput.y));

        if (moveDirection != Vector2Int.zero && moveTimer <= 0f)
        {
            _moveCount++;
            MoveShapes(moveDirection);
            Debug.Log("Move Direction: " + moveDirection);
            moveTimer = moveCooldown;
        }
    }

    private void MoveShapes(Vector2Int direction)
    {
        HashSet<Vector2Int> occupiedPositions = new HashSet<Vector2Int>();

        // Populate set of occupied positions with shapes that won't move
        foreach (var shape in _shapes)
        {
            if ((shape.EdgeCount != 1) && (_moveCount % shape.EdgeCount != 0))
            {
                occupiedPositions.Add(shape.GridPosition);
            }
        }

        foreach (var shape in _shapes)
        {
            // Only move if current move count is divisible by this shape's edge count
            if ((shape.EdgeCount == 1) || (_moveCount % shape.EdgeCount == 0))
            {
                Vector2Int newPosition = shape.GridPosition + direction;

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
            }
            else
            {
                // Shape doesn't move, so mark position as occupied
                occupiedPositions.Add(shape.GridPosition);
            }
        }
    }
}
