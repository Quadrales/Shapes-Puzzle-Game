using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PuzzleData", menuName = "Data/Puzzle/PuzzleData")]
public class PuzzleData : ScriptableObject
{
    // could turn some of these fields into sets instead of lists, since they should only contain unique values
    // (can't have overlapping starting positions for shapes/ghost shapes)
    [SerializeField] private List<Vector2Int> shapeStartingPositions;
    public List<Vector2Int> ShapeStartingPositions => shapeStartingPositions;

    [SerializeField] private List<Vector2Int> ghostShapeStartingPositions;
    public List<Vector2Int> GhostShapeStartingPositions => ghostShapeStartingPositions;

    [SerializeField] private int moveLimit;
    public int MoveLimit => moveLimit;

    // Can add more puzzle data later on (e.g. if it contains special items/events, or extra shapes)
}
