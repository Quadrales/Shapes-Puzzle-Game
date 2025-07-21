using NUnit.Framework;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private PuzzleData[] _levels;
    [SerializeField] private ShapeManager _shapeManager;
    private int _currentIndex = 0;

    void Start()
    {
        _shapeManager.LoadCurrentPuzzle(_levels[_currentIndex]);
    }

    void Update()
    {
        if (!_shapeManager.PuzzleComplete)
        {
            _shapeManager.HandleShapeMovement();
        }
    }

    private void UpdateLevelIndex()
    {
        // set current level index to number of selected level - 1
    }
}
