using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class UIManager : MonoBehaviour
{
    public TMPro.TMP_Dropdown playerChoseDropDown;

    public GameObject[] playerIcons;

    //public Action<int> ChangePlayerNumber;

    public GameObject playerChoosePanel;

    private int playerNumber = 2;

    public void OnStart()
    {
        //ChangePlayerNumber(playerNumber);
        GameManager.Instance.SetTotalnumberOfPlayers(playerNumber);
        playerChoosePanel.SetActive(false);
    }

    public void OnPlayerNumberChange()
    {
        switch(playerChoseDropDown.value)
        {
            case 0:
                playerNumber = 2;             
                break;

            case 1:
                playerNumber = 3;
                break;

            case 2:
                playerNumber = 4;
                break;

            case 3:
                playerNumber = 5;
                break;

            case 4:
                playerNumber = 6;
                break;

            default:
                playerNumber = 2;
                break;           
        }

        for (int i = 0; i < 6; i++)
            playerIcons[i].SetActive(false);

        for (int i = 0; i < playerNumber; i++)
            playerIcons[i].SetActive(true);
    }


}
