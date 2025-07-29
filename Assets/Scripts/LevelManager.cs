using NUnit.Framework;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private PuzzleData[] _levels;
    [SerializeField] private ShapeManager _shapeManager;

    public void LoadLevel(int levelIndex)
    {
        _shapeManager.gameObject.SetActive(true);

        _shapeManager.LoadCurrentPuzzle(_levels[levelIndex - 1]);
    }

    void Update()
    {
        
    }
}
