using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Waypoint CurrentWaypoint;

  /*  public GameObject[] waypoints;
    public int waypointIndex;

    public int targetPosition;

    public bool allowMovement;

    public Action PlayerMoved;
    public Action PlayerWon;


    public bool isAlternateMovement = false;
    // Start is called before the first frame update
    void Start()
    {
        waypointIndex = 0;
        allowMovement = false;
        transform.position = waypoints[waypointIndex].transform.position;
        targetPosition = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if(isAlternateMovement)
        {
            AlternateMovement(waypoints[waypointIndex].GetComponent<Waypoint>().targetWaypoint);
        }
        if (allowMovement)
            Move(targetPosition);
    }

    private void Move(int _targetPosition)
    {
        if (_targetPosition > 100)
            _targetPosition = 100;

        if (waypointIndex <= _targetPosition)
        {
            transform.position = Vector2.MoveTowards(transform.position, waypoints[waypointIndex].transform.position, 4 * Time.deltaTime);
            if (transform.position == waypoints[waypointIndex].transform.position)
            {
                waypointIndex++;
            }
        }
        else
        {
            allowMovement = false;
            waypointIndex--;

            if (waypointIndex == 100)
            {
                PlayerWon();
            }

            if (waypoints[waypointIndex].GetComponent<Waypoint>().isAlternatePath)
            {
               isAlternateMovement = true;
            }
            else
            {
                PlayerMoved();
            }
        }

    }

    private void AlternateMovement(int _targetPosition)
    {
       
        transform.position = Vector2.MoveTowards(transform.position, waypoints[_targetPosition].transform.position, 4 * Time.deltaTime);
        if(transform.position == waypoints[_targetPosition].transform.position)
        {
            isAlternateMovement = false;
            waypointIndex = _targetPosition;
            PlayerMoved();
        }
    }
  */
}
