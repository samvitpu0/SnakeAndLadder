using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using PsypherLibrary.SupportLibrary.Utils.Generics;
using PsypherLibrary.SupportLibrary.Extensions;

public class GameManager : GenericManager<GameManager>
{
    public static Action<int, int> MovePlayer;

    [SerializeField] private int totalNumberOfPlayers = 1;

    private const int WIN_POINT_INDEX = 99;

    private List<Player> players;
    private List<Player> playersWon;
    private int currentPlayerTurn = 1;

    public int CurrentPlayerTurn
    {
        get
        {
            return currentPlayerTurn;
        }
        set
        {
            if (value > totalNumberOfPlayers)
                currentPlayerTurn = 1;
        }
    }

    private void Start()
    {
        playersWon = new List<Player>();
    }

    public void InitPlayers(List<Player> _players)
    {
        players = _players;
    }

    public void DiceRolled(int _diceNumber)
    {
        MovePlayer.SafeInvoke(CurrentPlayerTurn, _diceNumber);
    }
    public void PlayerMoved(Player _player)
    {
        if(_player.CurrentWaypoint.WaypointIndex >= WIN_POINT_INDEX)
        {
            playersWon.Add(_player);
        }

        ChangeTurn();
    }
    private void ChangeTurn()
    {
        CurrentPlayerTurn++;                     
    }
}
