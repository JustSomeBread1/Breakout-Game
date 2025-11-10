using UnityEngine;

public class PlayerScore : MonoBehaviour
{
    // References
    GameObject uiCanvas;
    UserInterfaceManager userInterfaceManager;

    private void Awake()
    {
        uiCanvas = GameObject.Find("UICanvas");
        userInterfaceManager = uiCanvas.GetComponent<UserInterfaceManager>();
    }

    public int playerScore;
    public int highscore;

    private void Start()
    {
        highscore = PlayerPrefs.GetInt("Highscore");
        playerScore = 0;    
    }

    public void UpdateScore(int points)
    {
        playerScore += points;
        userInterfaceManager.UpdateScoreUI(playerScore);
        userInterfaceManager.LerpBorderColour(points);
    }

    public bool NewHighscore()
    {
        if (playerScore > highscore)
        {
            highscore = playerScore;
            return true;
        }

        return false;
    }
}