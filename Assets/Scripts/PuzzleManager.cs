using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    [SerializeField] ShapeManager shapeManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!shapeManager.PuzzleComplete)
        {
            shapeManager.HandleShapeMovement();
        }
        else
        {
            // display puzzle completion screen/animation
        }
    }
}
