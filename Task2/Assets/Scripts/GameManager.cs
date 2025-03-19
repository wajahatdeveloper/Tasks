using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum Player { None, Player1, Player2 }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI References")]
    public TextMeshProUGUI turnText;
    public GameObject winnerPanel;
    public TextMeshProUGUI winnerText;

    private Player[,] boardState = new Player[3, 3];
    private Cell[,] cells = new Cell[3, 3];
    private bool isPlayer1Turn = true;
    private bool gameEnded = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        winnerPanel.SetActive(false);
        InitializeBoard();
        UpdateTurnDisplay();
    }

    private void InitializeBoard()
    {
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                boardState[i, j] = Player.None;
            }
        }
    }

    public void RegisterCell(Cell cell, int row, int col)
    {
        cells[row, col] = cell;
    }

    public void CellClicked(int row, int column)
    {
        if (gameEnded || boardState[row, column] != Player.None)
            return;

        Player currentPlayer = isPlayer1Turn ? Player.Player1 : Player.Player2;
        boardState[row, column] = currentPlayer;
        cells[row, column].SetSymbol(currentPlayer == Player.Player1 ? "X" : "O");
        cells[row, column].SetInteractable(false);

        if (CheckWin(currentPlayer))
        {
            DeclareWinner(currentPlayer);
        }
        else if (IsBoardFull())
        {
            DeclareDraw();
        }
        else
        {
            SwitchTurn();
        }
    }

    private bool CheckWin(Player player)
    {
        // Check rows and columns
        for (int i = 0; i < 3; i++)
        {
            if (boardState[i, 0] == player && boardState[i, 1] == player && boardState[i, 2] == player)
                return true;
            if (boardState[0, i] == player && boardState[1, i] == player && boardState[2, i] == player)
                return true;
        }

        // Check diagonals
        if (boardState[0, 0] == player && boardState[1, 1] == player && boardState[2, 2] == player)
            return true;
        if (boardState[0, 2] == player && boardState[1, 1] == player && boardState[2, 0] == player)
            return true;

        return false;
    }

    private bool IsBoardFull()
    {
        foreach (Player cell in boardState)
        {
            if (cell == Player.None)
                return false;
        }
        return true;
    }

    private void SwitchTurn()
    {
        isPlayer1Turn = !isPlayer1Turn;
        UpdateTurnDisplay();
    }

    private void UpdateTurnDisplay()
    {
        turnText.text = isPlayer1Turn ? "Player 1's Turn" : "Player 2's Turn";
    }

    private void DeclareWinner(Player winner)
    {
        gameEnded = true;
        winnerPanel.SetActive(true);
        winnerText.text = winner == Player.Player1 ? "Player 1 Wins!" : "Player 2 Wins!";
        DisableAllCells();
    }

    private void DeclareDraw()
    {
        gameEnded = true;
        winnerPanel.SetActive(true);
        winnerText.text = "It's a Draw!";
        DisableAllCells();
    }

    private void DisableAllCells()
    {
        foreach (Cell cell in cells)
        {
            cell.SetInteractable(false);
        }
    }

    public void RestartGame()
    {
        gameEnded = false;
        winnerPanel.SetActive(false);
        InitializeBoard();
        isPlayer1Turn = true;
        UpdateTurnDisplay();

        foreach (Cell cell in cells)
        {
            cell.SetSymbol("");
            cell.SetInteractable(true);
        }
    }
}