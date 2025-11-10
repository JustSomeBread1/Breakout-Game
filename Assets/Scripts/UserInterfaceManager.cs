using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SocialPlatforms.Impl;
using System;
using UnityEngine.Events;
using System.Diagnostics.CodeAnalysis;
using Unity.Transforms;
using NUnit.Framework;
using Unity.VisualScripting;

public class UserInterfaceManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _playerScoreText;
    [SerializeField] private GameObject _LifeBar;
    [SerializeField] public GameObject LoseScreen;
    [SerializeField] private TextMeshProUGUI _HighscoreText;
    [SerializeField] private TextMeshProUGUI _LoseText;
    [SerializeField] private PlayerScore _playerScore;
    [SerializeField] int[] scoreThresholds;
    [SerializeField] private Color[] _scoreColours;
    private Image[] _gameBorders = new Image[3];

    private BallMovement _ballMovement;
    private GameObject _playerPaddle;
    private GameObject _gameScreen;
    private SpriteRenderer _plyrSpriteRenderer;
    private Image _losePanelImg;

    private int _scoreThresholdIndex;
    private float curLerpPercent;

    private void OnEnable()
    {
        PlayerHealth.onPlayerDeath += DeathInterface;
    }

    private void Start()
    {   
        _playerPaddle = GameObject.Find("Player");
        _plyrSpriteRenderer = _playerPaddle.GetComponent<SpriteRenderer>();

        var ball = GameObject.Find("Ball");
        _ballMovement = ball.GetComponent<BallMovement>();

        _losePanelImg = LoseScreen.transform.GetChild(0).GetComponent<Image>();

        // Grabs the Image components from the immediate children of the GameScreen
        var GameScreen = this.transform.GetChild(0);
        var validChildren = 0;
        for (int i = 0; i < GameScreen.childCount; i++) {
            var currentChild = GameScreen.transform.GetChild(i).gameObject;

            if (currentChild.transform.parent == GameScreen)
            {
                _gameBorders[validChildren] = currentChild.GetComponent<Image>();
                validChildren++;
            }
        }

        // Just some null checks
        if (_playerScoreText == null)
        {
            Console.WriteLine("No PlayerScore UI element found.");
            return;
        }

        if (_LifeBar == null)
        {
            Console.WriteLine("No LifeBar GameObject found.");
            return;
        }
    }

    private void OnDisable()
    {
        PlayerHealth.onPlayerDeath -= DeathInterface;
    }

    public void UpdateScoreUI(int currentScore)
    {
        _playerScoreText.text = ("Score: " + currentScore.ToString());
    }

    public void UpdateHealthUI(int healthChange)
    {
        // Reference to the lifeCounter images
        Image[] lives = _LifeBar.GetComponentsInChildren<Image>(true);

        // Check to see whether they are taking damage or healing
        bool gettingDamaged = false;
        int changeAmount = healthChange;
        if (healthChange < 0)
        {
            gettingDamaged = true;
        }

        // Makes sure that we're looping through the damage change for each point of damage
        if (gettingDamaged) {
            changeAmount = -changeAmount;
        }

        // Disables the lifeCounter UI elements depending on whether we're taking or adding health
        for (int x = 0; x < changeAmount; x++)
        {
            for (int i = (lives.Length - 1); i != -1; i--)
            {

                // Player getting damaged
                if (lives[i].IsActive() && healthChange < 0)
                {
                    lives[i].gameObject.SetActive(false);
                    break;
                }

                // Player getting healed
                if (!(lives[i].IsActive()) && healthChange > 0)
                {
                    lives[i].gameObject.SetActive(true);
                    break;
                }
            }
        }
    }


    // NOTE: If you set the pointsChange argument to 0 it will reset the lerp.
    public void LerpBorderColour(int pointsChange)
    {
        // Resets the current state of the Lerp
        if (pointsChange == 0)
        {
            _scoreThresholdIndex = 0;
            curLerpPercent = 0;
        }

        // Calculates the distance between the previous score threshold and the new one
        int thresholdDist = 0;
        if (_scoreThresholdIndex == 0)
        {
            thresholdDist = scoreThresholds[_scoreThresholdIndex];

        } else {

            thresholdDist = scoreThresholds[_scoreThresholdIndex] - scoreThresholds[_scoreThresholdIndex - 1];
        }
        
        // Find the progress percentage of the current lerp stage
        float lerpAmount = (float) pointsChange / thresholdDist;
        curLerpPercent += lerpAmount;

        // This changes the colour of the borders, it *should* also handle the overflow if the score percentage is above 100%
        bool keepDoing = true;
        while (keepDoing)
        {
            keepDoing = false;
            // Return a colour between the two thresholds based on the percentage of the way to the next threshold
            Debug.Log("Lerping by: " + curLerpPercent);
            var colourChange = Color.Lerp(_scoreColours[_scoreThresholdIndex], _scoreColours[_scoreThresholdIndex + 1], Mathf.Clamp(curLerpPercent, 0f, 1f));

            // Inverts the sent colour and applies it to the gameUI so its always distinct
            InvertLerpGameUI(colourChange);

            // Once lerp percent is 100%(1) increase the score threshold and then reset the percentage
            if (curLerpPercent >= 1)
            {
                _scoreThresholdIndex++;

                var remainder = curLerpPercent % 1;

                curLerpPercent = 0 + remainder;
                keepDoing = true;
            }

            foreach (Image border in _gameBorders)
            {
                border.color = colourChange;
            }

            _plyrSpriteRenderer.color = colourChange;
            _ballMovement.ChangeBallColour(colourChange);
            _losePanelImg.color = colourChange;
        }
    }

    private void InvertLerpGameUI(Color colour)
    {
        // Creates an inverted colour based on the colour sent via LerpBorderColour()
        float newR = 1.0f - colour.r;
        float newG = 1.0f - colour.g;
        float newB = 1.0f - colour.b;
        Color invertedColour = new Color(newR, newG, newB, 1);

        _playerScoreText.color = invertedColour;

        Image[] lives = _LifeBar.GetComponentsInChildren<Image>(true);
        foreach (Image life in lives)
        {
            life.color = invertedColour;
        }

        Camera.main.backgroundColor = invertedColour;
        _HighscoreText.color = invertedColour;
        _LoseText.color = invertedColour;
    }

    public void DeathInterface()
    {
        // Resets the health back to 3 for the next game
        UpdateHealthUI(3);

        // Changes the losescreen UI text based on if the player reached a new Highscore or not
        string scoreText = "";
        if (_playerScore.NewHighscore())
        {
            scoreText = "New Personal Best!\n" + _playerScore.highscore;

        } else {
            var distScore = (_playerScore.highscore - _playerScore.playerScore);
            scoreText = "Highscore unbeaten\n" + distScore + " away from beating highscore.";
        }
        _HighscoreText.text = scoreText;

        // Activates the losescreen for the player to see.
        LoseScreen.SetActive(true);
    }
}