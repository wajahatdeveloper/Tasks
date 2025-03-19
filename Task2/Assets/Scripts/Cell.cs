using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Cell : MonoBehaviour
{
    public int row;
    public int column;
    public TextMeshProUGUI label;
    
    private Button button;

    private void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnCellClicked);
        GameManager.Instance.RegisterCell(this, row, column);
    }

    private void OnCellClicked()
    {
        GameManager.Instance.CellClicked(row, column);
    }

    public void SetSymbol(string symbol)
    {
        label.text = symbol;
    }

    public void SetInteractable(bool interactable)
    {
        button.interactable = interactable;
    }
}