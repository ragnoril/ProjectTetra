using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<GameManager>();
                DontDestroyOnLoad(_instance.gameObject);
            }

            return _instance;
        }
    }

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            if (this != _instance)
                Destroy(this.gameObject);
        }
    }

    Vector3 _startPos;
    Vector3 _endPos;

    public UIManager UI;
    public BoardManager Board;
    public List<BlockAgent> ControlBlocks = new List<BlockAgent>();
    public BlockAgent SelectedBlock;
    public GameObject LineSelect;
    public GameObject BlockSelect;

    public int MaxScore
    {
        get => PlayerPrefs.GetInt("MaxScore", 0);
        set
        {
            PlayerPrefs.SetInt("MaxScore", value);
        }
    }

    public int ScoreMultiplier;
    public int Score;
    public bool IsGameOver;

    public bool _pausePlayerInputForGravity;

    private void Start()
    {
        _pausePlayerInputForGravity = false;
        IsGameOver = false;
        Score = 0;
        ScoreMultiplier = 1;
        Board.StartGame();
        UI.UpdateUI();
    }

    // Update is called once per frame
    void Update()
    {
        if (IsGameOver) return;

        if (_pausePlayerInputForGravity) return;

        if (Input.GetMouseButtonDown(0))
        {
            ScoreMultiplier = 1;
            _startPos = Input.mousePosition;

            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit.collider != null && hit.collider.GetComponent<BlockAgent>() != null)
            {
                SelectedBlock = hit.collider.GetComponent<BlockAgent>();
            }
        }
        else if (Input.GetMouseButton(0))
        {
            //_endPos = Input.mousePosition;
            Vector2 distance = Input.mousePosition - _startPos;

            if (SelectedBlock != null)
            {
                if (distance.x < 100)
                {
                    // target is control board
                    if (distance.x < -20)
                    {
                        if (SelectedBlock.X > 0)
                        {
                            LineSelect.SetActive(false);
                            SelectedBlock.GotSelected();
                            Vector3 pos = SelectedBlock.transform.position;
                            pos -= Vector3.right;
                            BlockSelect.SetActive(true);
                            BlockSelect.transform.position = pos;
                        }
                    }
                    else if(distance.x > 20)
                    {
                        if (SelectedBlock.X == 4)
                        {
                            LineSelect.SetActive(true);
                            BlockSelect.SetActive(false);
                            Vector3 pos = LineSelect.transform.position;
                            pos.y = -2f + SelectedBlock.Y;
                            LineSelect.transform.position = pos;
                        }
                        else
                        {
                            LineSelect.SetActive(false);
                            SelectedBlock.GotSelected();
                            Vector3 pos = SelectedBlock.transform.position;
                            pos += Vector3.right;
                            BlockSelect.SetActive(true);
                            BlockSelect.transform.position = pos;
                        }
                    }
                    else
                    {
                        BlockSelect.SetActive(false);
                        LineSelect.SetActive(false);
                    }
                    
                }
                else 
                {
                    // target is goal board
                    LineSelect.SetActive(true);
                    BlockSelect.SetActive(false);
                    Vector3 pos = LineSelect.transform.position;
                    pos.y = -2f + SelectedBlock.Y;
                    LineSelect.transform.position = pos;
                }
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            _endPos = Input.mousePosition;
            Vector2 distance = _endPos - _startPos;

            if (SelectedBlock != null)
            {
                if (distance.x > 20)
                {
                    if (SelectedBlock.X == 4)
                    {
                        PassAllRight();
                    }
                    else if (distance.x < 100)
                    {
                        PassBlockRight();
                    }
                    else
                    {
                        PassAllRight();
                    }
                }
                else if (distance.x < -20)
                {
                    if (SelectedBlock.X > 0)
                    {
                        PassBlockLeft();
                    }
                }

                SelectedBlock.UnSelected();
                SelectedBlock = null;
                LineSelect.SetActive(false);
                BlockSelect.SetActive(false);
            }
        }
    }

    public void PassBlockRight()
    {
        //Debug.Log("right");

        if (SelectedBlock.BoardId == 1) return;

        if (Board.CanMoveBlockInControlBoard(SelectedBlock.X, SelectedBlock.Y, 1))
        {
            // blegh
            Board.MoveBlockInControlBoard(SelectedBlock.X, SelectedBlock.Y, 1);
            SelectedBlock.GoRight();
            StartCoroutine(WaitForSwipeUp());
        }
        
        //Board.Redraw();
    }

    public void PassBlockLeft()
    {
        //Debug.Log("left");

        if (SelectedBlock.BoardId == 1) return;

        if (Board.CanMoveBlockInControlBoard(SelectedBlock.X, SelectedBlock.Y, -1))
        {
            // blegh
            Board.MoveBlockInControlBoard(SelectedBlock.X, SelectedBlock.Y, -1);
            SelectedBlock.GoLeft();
            StartCoroutine(WaitForSwipeUp());
        }

        //Board.Redraw();
    }

    public void AddNewBlocksLine()
    {
        int result = Board.SwipeUpBlocksInControlBoard();
        if (result == 0)
        {
            //int[] line = { 0, 1, 1, 0, 1 };
            Board.PassNewLineToControlBoard(Board.GetRandomLine(Random.Range(1, 3)));
            Board.RenderNewLine();

            foreach (BlockAgent block in ControlBlocks)
            {
                if (block.BoardId == 0)
                    block.GoUp();
            }

            
        }
        else if (result == -1)
        {
            GameOver();
        }
    }

    IEnumerator WaitForSwipeUp()
    {
        //_pausePlayerInputForGravity = true;
        yield return new WaitForSeconds(0.5f);
        //Debug.Log("Swipe up");
        AddNewBlocksLine();
        //Board.CheckForGravityInControlBoard();
        //StartCoroutine(WaitForFixPosition());
    }

    IEnumerator WaitForFixPosition()
    {
        yield return new WaitForSeconds(0.5f);
        //Debug.Log("gravity");
        FixBlocksPositions();
        //_pausePlayerInputForGravity = false;
    }

    public void FixBlocksPositions()
    {
        foreach (BlockAgent block in ControlBlocks)
        {
            Vector3 pos = block.transform.localPosition;
            int y = Mathf.RoundToInt(pos.y);

            //Debug.Log("block: " + block.name + " posy: " + y + " valy: " + block.Y + " board: " + block.BoardId);

            if (block.Y != y)
            {
                //Debug.Log("block: " + block.name + " posy: " + y + " valy: " + block.Y + " board: " + block.BoardId);
                //Debug.Log("block: " + block.name + " fix position called");
                block.FixPosition();
            }
        }
    }

    public BlockAgent GetBlock(int x, int y, int boardId)
    {
        foreach(BlockAgent block in ControlBlocks)
        {
            if (block.X == x && block.Y == y && block.BoardId == boardId)
                return block;
        }

        return null;
    }

    public void CheckBlockFall(BlockAgent agent)
    {
        int fall = Board.FallBlockInControlBoard(agent.X, agent.Y, 0);
        if (fall > 0)
            agent.GoDown(fall);
    }

    /*
    public void MoveBlockDown(int x, int y)
    {
        foreach(BlockAgent block in ControlBlocks)
        {
            if (block.X == x && block.Y == y && block.BoardId == 0)
            {
                block.GoDown(1);
                break;
            }
        }
    }
    */

    public void PassAllRight()
    {
        if (SelectedBlock.BoardId == 1) return;

        //if (Board.CanPassLineFromControlToGoal(SelectedBlock.Y))
        {
            Board.PassLineFromControlToGoal(SelectedBlock.Y);
            
            foreach (BlockAgent block in ControlBlocks)
            {
                if (block.Y == SelectedBlock.Y && block.BoardId == 0)
                {
                    BlockAgent target = GetBlock(block.X, block.Y, 1);
                    if (target == null)
                        block.PassToGoalBoard();
                }
            }

            StartCoroutine(WaitForSwipeUp());

            StartCoroutine(WaitForCheckLine(SelectedBlock.Y));

        }


        //Board.Redraw();
    }

    IEnumerator WaitForCheckLine(int y)
    {
        if (Board.CheckIfGoalLineCompleted(y))
        {
            StartCoroutine(ClearGoalLine(y));
            yield return new WaitForSeconds(0.5f);
            StartCoroutine(WaitForGoalBoardGravity());
        }
        else
        {
            StartCoroutine(WaitForGoalBoardGravity());
        }
    }

    IEnumerator WaitForGoalBoardGravity()
    {
        _pausePlayerInputForGravity = true;
        yield return new WaitForSeconds(0.15f);
        bool gravityWorks = Board.CheckForGravityInGoalBoard();
        while (gravityWorks)
        {
            StartCoroutine(WaitForFixPosition());
            yield return new WaitForSeconds(0.5f);
            for(int y = 0; y < Board.Height; y++)
            {
                if (Board.CheckIfGoalLineCompleted(y))
                {
                    //Debug.Log("before clearing line : " + y);
                    StartCoroutine(ClearGoalLine(y));
                }
            }

            yield return new WaitForSeconds(0.5f);
            gravityWorks = Board.CheckForGravityInGoalBoard();
        }
        _pausePlayerInputForGravity = false;
    }

    public void AddScore(int val)
    {
        Score += val;
        if (Score > MaxScore)
            MaxScore = Score;
    }

    IEnumerator ClearGoalLine(int y)
    {
        // add score;
        AddScore(25 * ScoreMultiplier);
        ScoreMultiplier += 1;
        UI.UpdateUI();

        // clear line from board
        Board.ClearGoalLine(y);

        yield return new WaitForSeconds(0.5f);

        // clear tile objects
        foreach (BlockAgent block in ControlBlocks)
        {
            if (block.Y == y && block.BoardId == 1)
                block.Cleared();
        }
    }

    public void RestartGame()
    {
        IsGameOver = false;
        Board.RestartGame();
        UI.UpdateUI();
    }

    public void GameOver()
    {
        IsGameOver = true;
        UI.UpdateUI();
    }

}
