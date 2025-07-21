using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PuzzleData", menuName = "Data/Puzzle/PuzzleData")]
public class PuzzleData : ScriptableObject
{
    [SerializeField] private List<Vector2Int> shapeStartingPositions;
    public List<Vector2Int> ShapeStartingPositions => shapeStartingPositions;

    [SerializeField] private List<Vector2Int> ghostShapeStartingPositions;
    public List<Vector2Int> GhostShapeStartingPositions => ghostShapeStartingPositions;

    [SerializeField] private int moveLimit;
    public int MoveLimit => moveLimit;
}
