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
    private float distanceTravelled;

    private int targetWaypoint;

    private int currentPlayerNumber;
    private int diceNumber;

    private void Start()
    {
        GameManager.MovePlayer += MovePlayer;

        for (int i = 0; i < BoardPoints.Count; i++)
        {
            BoardPoints[i].WaypointIndex = i;
        }
    }

    public void MovePlayer(int _playerNumber, int _diceNumber)
    {
        currentPlayerNumber = _playerNumber;
        diceNumber = _diceNumber;
        
        targetWaypoint = Players[currentPlayerNumber - 1].CurrentWaypoint.WaypointIndex + diceNumber;

        var _targetPosition = BoardPoints[Players[currentPlayerNumber - 1].CurrentWaypoint.WaypointIndex + 1].transform.position;
        Players[currentPlayerNumber - 1].transform.DOMove(_targetPosition, playerMoveDuration, snapping: false).OnComplete(MoveComplete);
      
    }

    private void MoveComplete()
    {
        Players[currentPlayerNumber - 1].CurrentWaypoint = BoardPoints[Players[currentPlayerNumber - 1].CurrentWaypoint.WaypointIndex + 1];
        if (targetWaypoint != Players[currentPlayerNumber - 1].CurrentWaypoint.WaypointIndex)
        {
            var _targetPosition = BoardPoints[Players[currentPlayerNumber - 1].CurrentWaypoint.WaypointIndex + 1].transform.position;
            Players[currentPlayerNumber - 1].transform.DOMove(_targetPosition, playerMoveDuration, snapping: false).OnComplete(MoveComplete);
        }
        else
        {
            CheckWaypointForPaths();
            GameManager.Instance.PlayerMoved(Players[currentPlayerNumber - 1]);
        }
    }
    private void CheckWaypointForPaths()
    {
        if (Players[currentPlayerNumber - 1].CurrentWaypoint.isAlternatePath)
        {
            foreach(SnakeAndLadderPath path in SnakeAndLadderPaths)
            {
                if(path.PathIndex == Players[currentPlayerNumber - 1].CurrentWaypoint.WaypointIndex)
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
            Players[currentPlayerNumber - 1].transform.position = _path.path.GetPointAtDistance(distanceTravelled, EndOfPathInstruction.Stop);
            timer -= Time.deltaTime;
            yield return null;
        }
        distanceTravelled = 0f;
        Players[currentPlayerNumber - 1].transform.position = BoardPoints[path.TargetWaypointIndex].transform.position;
        Players[currentPlayerNumber - 1].CurrentWaypoint = BoardPoints[path.TargetWaypointIndex];
        Debug.Log("PlayerMoved");
        yield return null;
    }

    private void OnDisable()
    {
        GameManager.MovePlayer -= MovePlayer;
    }
}
