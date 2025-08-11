using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private PuzzleData[] levels;
    [SerializeField] private PuzzleGameplayManager puzzleGameplayManager;

    [SerializeField] private GameObject puzzleObjects;
    [SerializeField] private GameObject levelSelectionMenu;

    public void LoadLevel(int levelIndex)
    {
        puzzleObjects.SetActive(true);

        puzzleGameplayManager.LoadCurrentPuzzle(levels[levelIndex - 1]);
    }

    public void CloseLevelSelectionMenu()
    {
        levelSelectionMenu.SetActive(false);
    }

    private void Awake()
    {
        
    }

    // Currently doesn't work, meant to load all level scriptable objects into the levels array
    // but this may not even be necessary, would just make things more robust for when new levels are added
    // (no need to keep adding each SO to the levels list through the inspector)
    private void ParseLevelData()
    {
        string[] levelStrings = AssetDatabase.FindAssets("Level");

        for (int i = 0; i < levelStrings.Length; i++)
        {
            
        }
    }

    void Update()
    {
        
    }
}
