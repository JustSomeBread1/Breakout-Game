using UnityEngine;
using UnityEngine.UI;

public class GameStateHandler : MonoBehaviour
{
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _exitButton;
    PlayerHealth playerHP;
    PlayerScore  playerScr;
    
    GameObject uiCanvas;
    UserInterfaceManager uiManager;

    GameObject blockManager;
    BlockSpawner blockSpawner;

    [SerializeField] GameObject ball;
    BallMovement ballMvmt;

    private void Start()
    {
        ballMvmt = ball.GetComponent<BallMovement>();

        uiCanvas = GameObject.Find("UICanvas");
        uiManager = uiCanvas.GetComponent<UserInterfaceManager>();

        blockManager = GameObject.Find("BlockManager");
        blockSpawner = blockManager.GetComponent<BlockSpawner>();

        playerHP = GetComponent<PlayerHealth>();
        playerScr = GetComponent<PlayerScore>();
        
        _startButton.onClick.AddListener(StartOverButton);
        _exitButton.onClick.AddListener(ExitGameButton);
    }

    public void ExitGameButton()
    {
        Application.Quit();
        PlayerPrefs.SetInt("Highscore", playerScr.highscore);
    }

    public void StartOverButton()
    {
        // Reset player stats
        playerScr.playerScore = 0;
        uiManager.UpdateScoreUI(playerScr.playerScore);
        playerHP.ModifyLife(playerHP.maxHealth);

        // Reset gamestate 
        ball.SetActive(true);
        ballMvmt.roundStart = true;
        uiManager.LoseScreen.SetActive(false);
        uiManager.LerpBorderColour(0);
        blockSpawner.GetNewBlocks();
    }
}