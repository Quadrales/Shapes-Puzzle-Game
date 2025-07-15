using TMPro;
using UnityEngine;

public class MoveCountManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI moveCountText;
    [SerializeField] private TextMeshProUGUI moveLimitText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        moveCountText.text = "0";
        moveLimitText.text = "30"; // later set this to the move limit of the current level
    }

    public void IncrementMoveCountText(int increment)
    {
        int moveCount = int.Parse(moveCountText.text) + increment;
        moveCountText.text = moveCount.ToString();
    }
}
