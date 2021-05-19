using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
   /* public Player[] player;
    public Dice dice;

    public GameObject[] playerTurnsUI;
    public GameObject[] playerRankUI;

    public GameObject RollUI;

    public int playerTurn = 0;

    public GameObject gameOverUI;
    public GameObject pauseUI;
    public UIManager uiManager;

    List<int> playersWon;

    private int playerCount = 3;

    private int playerRank = 0;

    private void OnEnable()
    {
       // dice.DiceRolled += HandleDiceRoll;

        for (int i = 0; i < player.Length; i++)
        {
            player[i].PlayerMoved += HandlePlayerMoved;
            player[i].PlayerWon += HandlePlayerWon;
        }

        uiManager.ChangePlayerNumber += PlayerNumberChanged;

        
    }


    void Start()
    {
        playersWon = new List<int>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void HandleDiceRoll()
    {
        player[playerTurn].targetPosition = player[playerTurn].waypointIndex + dice.diceNumber;
        player[playerTurn].allowMovement = true;
    }

    private void HandlePlayerMoved()
    {
        playerTurnsUI[playerTurn].SetActive(false);

        playerTurn++;

        for(int i=0;i< playerCount; i++)
        if (playersWon.Contains(playerTurn))
            playerTurn++;
        

        if(playersWon.Count == playerCount-1)
        {
            gameOverUI.SetActive(true);
            dice.allowRoll = false;
        }

        if (playerTurn > playerCount-1)
            playerTurn = 0;

        for (int i = 0; i < playerCount; i++)
            if (playersWon.Contains(playerTurn))
                playerTurn++;

        playerTurnsUI[playerTurn].SetActive(true);


        dice.allowRoll = true;
        RollUI.SetActive(true);
        Debug.Log("Player Moved");
    }

    private void HandlePlayerWon()
    {
            playersWon.Add(playerTurn);
            playerRankUI[playerRank].GetComponent<TextMeshProUGUI>().text = "Player " + (playerTurn+1);
        playerRankUI[playerRank].SetActive(true);
            playerRank++;
    }

    private void OnDisable()
    {
        //dice.DiceRolled -= HandleDiceRoll;

        for (int i = 0; i < player.Length; i++)
            player[i].PlayerMoved -= HandlePlayerMoved;

        uiManager.ChangePlayerNumber -= PlayerNumberChanged;

    }

    private void PlayerNumberChanged(int _playerCount)
    {
        playerCount = _playerCount;

        for(int i = 0; i < player.Length; i++)
        {
            player[i].gameObject.SetActive(false);
        }

        for(int i=0;i< playerCount;i++)
        {
            player[i].gameObject.SetActive(true);
        }

        dice.gameObject.SetActive(true);
        RollUI.SetActive(true);
    }

    #region UI Functions

    public void OnPause()
    {
        dice.allowRoll = false;
        pauseUI.SetActive(true);
    }

    public void OnReplay()
    {
        SceneManager.LoadScene("Main");
    }

    public void OnExit()
    {
        Application.Quit();
    }

    #endregion*/
}
