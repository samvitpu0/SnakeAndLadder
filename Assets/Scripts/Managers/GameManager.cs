using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using PsypherLibrary.SupportLibrary.Utils.Generics;
using PsypherLibrary.SupportLibrary.Extensions;

public class GameManager : GenericSingleton<GameManager>
{
    public static Action<int> InitPlayerList;
    public static Action<int, int> MovePlayer;
    public static Action GameOver;

    private int totalNumberOfPlayers = 1;
    private const int WIN_POINT_INDEX = 99;
    private List<Player> players;
    private List<int> playersWon;
    private int currentPlayerTurn = 0;

    private void Start()
    {
        playersWon = new List<int>();
    }

    public void SetTotalnumberOfPlayers(int _numberOfPlayers)
    {
        totalNumberOfPlayers = _numberOfPlayers;
        InitPlayerList.SafeInvoke(totalNumberOfPlayers);
    }


    public void InitPlayers(List<Player> _players)
    {
        players = _players;
    }

    public void DiceRolled(int _diceNumber)
    {
        MovePlayer.SafeInvoke(currentPlayerTurn, _diceNumber);
    }
    public void PlayerMoved(int _playerIndex)
    {
        if(players[_playerIndex].CurrentWaypoint.WaypointIndex >= WIN_POINT_INDEX)
        {
            playersWon.Add(_playerIndex);
        }

        if(playersWon.Count >= totalNumberOfPlayers-1)
        {
            //Game Over
            GameOver.SafeInvoke();
            return;
        }
        
        ChangeTurn();
    }
    private void ChangeTurn()
    {
        currentPlayerTurn++;
        if (currentPlayerTurn > totalNumberOfPlayers - 1)
            currentPlayerTurn = 0;

        
        for (int i = 0; i < players.Count; i++)
        {
            if (playersWon.Contains(currentPlayerTurn))
                currentPlayerTurn++;
        }
        
    }
}
