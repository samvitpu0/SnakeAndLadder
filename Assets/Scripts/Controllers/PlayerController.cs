using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;
using DG.Tweening;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private List<Player> Players;
    [SerializeField] private List<Waypoint> BoardPoints;
    [SerializeField] private List<SnakeAndLadderPath> SnakeAndLadderPaths;
    [SerializeField] private float playerMoveDuration = 0.3f;
    [SerializeField] private float playerSpeedOnPath = 5f;

    private List<Player> PlayerList;
    private float distanceTravelled;
    private int targetWaypoint;
    private int currentPlayerNumber;
    private int diceNumber;


    private void Start()
    {
        GameManager.InitPlayerList += InitPlayerList;
        GameManager.MovePlayer += MovePlayer;

        PlayerList = new List<Player>();

        for (int i = 0; i < BoardPoints.Count; i++)
        {
            BoardPoints[i].WaypointIndex = i;
        }
    }

    private void InitPlayerList(int _totalNumberofPlayers)
    {
        for(int i=0; i < _totalNumberofPlayers; i++)
        {
            Players[i].gameObject.SetActive(true);
            PlayerList.Add(Players[i]);
        }
        GameManager.Instance.InitPlayers(PlayerList);
    }

    public void MovePlayer(int _playerNumber, int _diceNumber)
    {
        currentPlayerNumber = _playerNumber;
        diceNumber = _diceNumber;
        
        targetWaypoint = Players[currentPlayerNumber].CurrentWaypoint.WaypointIndex + diceNumber;

        var _targetPosition = BoardPoints[Players[currentPlayerNumber].CurrentWaypoint.WaypointIndex + 1].transform.position;
        Players[currentPlayerNumber].transform.DOMove(_targetPosition, playerMoveDuration, snapping: false).OnComplete(MoveComplete);
      
    }

    private void MoveComplete()
    {
        Players[currentPlayerNumber].CurrentWaypoint = BoardPoints[Players[currentPlayerNumber].CurrentWaypoint.WaypointIndex + 1];
        if (targetWaypoint != Players[currentPlayerNumber].CurrentWaypoint.WaypointIndex)
        {
            var _targetPosition = BoardPoints[Players[currentPlayerNumber].CurrentWaypoint.WaypointIndex + 1].transform.position;
            Players[currentPlayerNumber].transform.DOMove(_targetPosition, playerMoveDuration, snapping: false).OnComplete(MoveComplete);
        }
        else
        {
            
            CheckWaypointForPaths();
            
            GameManager.Instance.PlayerMoved(currentPlayerNumber);
        }
    }
    private void CheckWaypointForPaths()
    {
        if (Players[currentPlayerNumber].CurrentWaypoint.isAlternatePath)
        {
            foreach(SnakeAndLadderPath path in SnakeAndLadderPaths)
            {
                if(path.PathIndex == Players[currentPlayerNumber].CurrentWaypoint.WaypointIndex)
                {                   
                    StartCoroutine(MovePlayerOnPath(path));
                    break;
                }
            }
        }
        else
        {
            return;
        }
    }

    private IEnumerator MovePlayerOnPath(SnakeAndLadderPath path)
    {
        var _path = path.GetComponent<PathCreator>();
        float timer = _path.path.length/playerSpeedOnPath;

        Debug.Log("Timer :" + timer);
        while (timer > 0)
        {           
            distanceTravelled += playerSpeedOnPath * Time.deltaTime;
            Players[currentPlayerNumber].transform.position = _path.path.GetPointAtDistance(distanceTravelled, EndOfPathInstruction.Stop);
            timer -= Time.deltaTime;
            yield return null;
        }
        distanceTravelled = 0f;
        Players[currentPlayerNumber].transform.position = BoardPoints[path.TargetWaypointIndex].transform.position;
        Players[currentPlayerNumber].CurrentWaypoint = BoardPoints[path.TargetWaypointIndex];
        Debug.Log("PlayerMoved");
        yield return null;
    }

    private void OnDisable()
    {
        GameManager.InitPlayerList -= InitPlayerList;
        GameManager.MovePlayer -= MovePlayer;
    }
}
