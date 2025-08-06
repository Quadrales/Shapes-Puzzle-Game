using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    // used for handling switching between states, e.g. menus to puzzles or pausing the game

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("Exit game.");
    }
}
