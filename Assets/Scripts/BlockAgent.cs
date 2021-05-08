using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockAgent : MonoBehaviour
{
    public int X, Y;
    public int Value;
    public int BoardId;

    public float MoveSpeed;

    private bool _isMoving;

    private void OnMouseDown()
    {
        //Debug.Log("clicked on " + this.gameObject.name);

        GameManager.instance.SelectedBlock = this;

    }

    IEnumerator MoveTo(Vector3 pos, float t, bool checkForFall)
    {
        float elapsedTime = 0;

        while (elapsedTime < t)
        {
            this.transform.position = Vector3.Lerp(this.transform.position, pos, (elapsedTime / t));

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        _isMoving = false;

        
        if (checkForFall)
            GameManager.instance.CheckBlockFall(this);
        
    }

    public void GoLeft()
    {
        _isMoving = true;
        X -= 1;
        Vector3 target = transform.position + new Vector3(-1f, 0f, 0f);
        StartCoroutine(MoveTo(target, MoveSpeed, false));
    }

    public void GoRight()
    {
        _isMoving = true;
        X += 1;
        Vector3 target = transform.position + new Vector3(1f, 0f, 0f);
        StartCoroutine(MoveTo(target, MoveSpeed, false));
    }

    public void GoUp()
    {
        Y += 1;
        Vector3 target = transform.position + new Vector3(0f, 1f, 0f);
        StartCoroutine(MoveTo(target, MoveSpeed, false));
    }

    public void GoDown(int amount)
    {
        Y -= amount;
        Vector3 target = transform.position + new Vector3(0f, -amount, 0f);
        StartCoroutine(MoveTo(target, MoveSpeed, false));
    }

    public void PassToGoalBoard()
    {
        BoardId = 1;
        Vector3 target = transform.position + new Vector3(5.5f, 0f, 0f);
        transform.SetParent(GameManager.instance.Board.GoalParent);
        StartCoroutine(MoveTo(target, MoveSpeed, false));
    }

    public void Cleared()
    {
        //Debug.Log("my name is " + gameObject.name + " x:" + X + " y:" + Y + " board:" + BoardId);
        StartCoroutine(FadeOut(0.5f));
    }

    public void FixPosition()
    {
        Vector3 target = transform.parent.position + new Vector3(X, Y, 0f);
        StartCoroutine(MoveTo(target, MoveSpeed, false));
    }

    IEnumerator FadeOut(float t)
    {
        float elapsedTime = 0;

        while (elapsedTime < t)
        {
            this.transform.localScale = Vector3.Lerp(this.transform.localScale, Vector3.zero, (elapsedTime / t));

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        GameManager.instance.ControlBlocks.Remove(this);
        Destroy(this.gameObject);
    }

    public void GotSelected()
    {

    }

    public void UnSelected()
    {

    }
}
