using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Dice : MonoBehaviour
{
    public GameObject RollUI;
    public Sprite[] diceImages;
    public bool allowRoll;
    public int diceNumber = 0;    
    SpriteRenderer diceRenderer;

    void Start()
    {
        diceRenderer = GetComponent<SpriteRenderer>();
        allowRoll = true;
    }

    private void OnMouseDown()
    {
        if (allowRoll)
        {
            RollUI.SetActive(false);
            StartCoroutine("RollDice");
        }
    }

    IEnumerator RollDice()
    {
        int dummyNumber = 1;
        allowRoll = false;

        for (int i = 1; i <= 10; i++)
        {
            diceRenderer.sprite = diceImages[dummyNumber];
            dummyNumber++;
            if (dummyNumber > 5) dummyNumber = 1;
            yield return new WaitForSeconds(0.1f);
        }

        diceNumber = (int)Mathf.Floor(UnityEngine.Random.Range(1, 7));

        GameManager.Instance.DiceRolled(diceNumber);
        allowRoll = true;
        diceRenderer.sprite = diceImages[diceNumber];
        yield return null;
    }
}
