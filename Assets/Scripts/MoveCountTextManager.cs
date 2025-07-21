using TMPro;
using UnityEngine;

public class MoveCountTextManager : MonoBehaviour
{
    [SerializeField] private Canvas textCanvas;
    [SerializeField] private GameObject moveCountPrefab;
    [SerializeField] private GameObject moveLimitPrefab;
    private TextMeshProUGUI moveCountText;
    private TextMeshProUGUI moveLimitText;
    private Vector3 moveCountPosition = new Vector3(320, 140, 0);
    private Vector3 moveLimitPosition = new Vector3(320, 180, 0);

    public void InstantiateTextUI(int moveLimit)
    {
        InstantiateText();

        moveCountText.text = "0";
        moveLimitText.text = moveLimit.ToString();
    }

    public void UpdateMoveCountText(int moveCount)
    {
        moveCountText.text = moveCount.ToString();
    }

    private void InstantiateText()
    {
        GameObject moveCountInstance = Instantiate(moveCountPrefab, moveCountPosition, Quaternion.identity);
        GameObject moveLimitInstance = Instantiate(moveLimitPrefab, moveLimitPosition, Quaternion.identity);

        // Set text objects as child of canvas
        moveCountInstance.transform.SetParent(textCanvas.transform, false);
        moveLimitInstance.transform.SetParent(textCanvas.transform, false);

        moveCountText = moveCountInstance.GetComponent<TextMeshProUGUI>();
        moveLimitText = moveLimitInstance.GetComponent<TextMeshProUGUI>();
    }
}
