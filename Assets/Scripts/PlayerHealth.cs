using System;
using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 3;
    public int health;

    // Creates the playerDeath event
    public delegate void PlayerDeath();
    public static event PlayerDeath onPlayerDeath;

    GameObject uiCanvas;
    UserInterfaceManager userInterfaceManager;


    private void Awake()
    {
        // Grab References
        uiCanvas = GameObject.Find("UICanvas");
        userInterfaceManager = uiCanvas.GetComponent<UserInterfaceManager>();

        // Set stats
        health = maxHealth;
    }

    public int ModifyLife(int change)
    {
        health += change;

        // Checks to see whether the player is dead or not
        if (health == 0)
        {
            onPlayerDeath.Invoke();
        }

        userInterfaceManager.UpdateHealthUI(change);

        return health;
    }
}