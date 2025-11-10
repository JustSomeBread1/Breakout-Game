using System;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class BallMovement : MonoBehaviour
{
    // References
    GameStateHandler gameStateHandler;
    GameObject gameManager;
    GameObject blockManager;
    BlockSpawner blockSpawner;
    PlayerHealth playerHealth;
    PlayerScore playerScore;

    private SpriteRenderer _ballSprite;

    private float forceModifier;
    [SerializeField] private float startingForce;
    [SerializeField] private int consecutiveScoreMod;   
    Vector3 startingPos;
    float badArea;
    float paddleFraction;
    public bool roundStart;
    public bool hitPaddle;

    private Vector2 _regularVelocity;
    private int _speedIncreases;
    private int _maxSpeedIncrease;
    [SerializeField] private float _velocityIncreaseRate;

    private AudioSource _ballHitSound;


    Rigidbody2D rb;

    private void Awake()
    {
        // Grab references
        gameManager = GameObject.Find("GameManager");
        gameStateHandler = gameManager.GetComponent<GameStateHandler>();
        playerScore = gameManager.GetComponent<PlayerScore>();
        playerHealth = gameManager.GetComponent<PlayerHealth>();

        blockManager = GameObject.Find("BlockManager");
        blockSpawner = blockManager.GetComponent<BlockSpawner>();

        _ballSprite = this.gameObject.GetComponent<SpriteRenderer>();
        _ballHitSound = this.gameObject.GetComponentInChildren<AudioSource>();

        _ballHitSound.playOnAwake = false;

        rb = GetComponent<Rigidbody2D>();

        // Initialises variables
        badArea = -9.3f;
        startingPos = new Vector3(0, -8.1f, 0);
        forceModifier = 10f;
        roundStart = true;
        paddleFraction = 0.75f;
    }

    private void OnEnable()
    {
        PlayerHealth.onPlayerDeath += ChangeBallActive;
    }

    private void Update()
    {
        if (transform.position.y <= badArea || roundStart)
        {
            if (roundStart)
            {
                ChangeBallVelocity(true);
                BallSpawn();
            } else {
                playerHealth.ModifyLife(-1);
                BallSpawn();
            }
        }
    }

    private void OnDisable()
    {
        PlayerHealth.onPlayerDeath -= ChangeBallActive;
    }

    private void BallSpawn()
    {
        roundStart = false;

        // Reset balls position and velocity.
        transform.position = startingPos;
        rb.linearVelocity = Vector2.zero;

        // Shoot the ball in a random direction
        Vector2 force = new Vector2((UnityEngine.Random.Range(-1f, 1f) * forceModifier), forceModifier);
        rb.AddForce(force, ForceMode2D.Impulse);
    }

    public void ChangeBallActive()
    {
        this.gameObject.SetActive(!this.gameObject.activeSelf);
    }

    public void ChangeBallColour(Color newColour)
    {
        _ballSprite.color = newColour;
    }

    public void ChangeBallVelocity(bool stopSpeed)
    {
        //Resets the balls speed to normal if it hits the paddle.
        if (stopSpeed)
        {
            for (int i = 0; i < _speedIncreases; i++)
            {
                rb.AddForce((rb.linearVelocity * -_velocityIncreaseRate), ForceMode2D.Force);
            }

            _speedIncreases = 0;
            return;
        }

        if (_speedIncreases !>= _maxSpeedIncrease)
        {
            _speedIncreases++;
            rb.AddForce(rb.linearVelocity * _velocityIncreaseRate, ForceMode2D.Force);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Plays the ball hit sound for every collision
        _ballHitSound.pitch = UnityEngine.Random.Range(1f, 2f);
        _ballHitSound.Play();
        

        if (collision.gameObject.name == "Player")
        {
            var contactPoint = collision.GetContact((collision.contactCount) - 1);
            var paddlePosition = collision.gameObject.transform.position;

            Vector2 tempVel = rb.linearVelocity;
            float offset = contactPoint.point.x - paddlePosition.x;
            if (offset >= paddleFraction)
            {
                // Hit right side
                if (rb.linearVelocity.x < 0 )
                {
                    rb.linearVelocity = new Vector2(-tempVel.x, tempVel.y);
                }
            } else if (offset <= -paddleFraction)
            {
                // Hit left side
                if (rb.linearVelocity.x > 0 )
                {
                    rb.linearVelocity = new Vector2(-tempVel.x, tempVel.y);
                }

            } else {
                // Hit the middle
            }

            hitPaddle = true;
        }
        if (collision.gameObject.name == "Block(Clone)")
        {
            var blockData = collision.gameObject.GetComponent<BlockType>();
            if (blockData == null)
            {
                Debug.Log("No current blocktype component!");
            }

            /* Implements logic for consecutive block breaking increaseing the score */
            if (hitPaddle)
            {
                consecutiveScoreMod = 0;
                
                hitPaddle = false;

            } else { consecutiveScoreMod += 50; }

            playerScore.UpdateScore((blockData.dataObj.points + consecutiveScoreMod));

            blockSpawner.DestroyBlock(collision.gameObject);
        }
        ChangeBallVelocity(hitPaddle);
    }
}