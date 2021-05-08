using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{

    public GameObject TilePrefab;
    public GameObject BlockPrefab;
    public int Width, Height;

    public int[] ControlBoard;
    public int[] GoalBoard;

    public Transform ControlParent;
    public Transform GoalParent;


    public void EmptyBoards()
    {
        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                ControlBoard[i + (Width * j)] = 0;
                GoalBoard[i + (Width * j)] = 0;
            }
        }
    }

    public void PrepareBoards()
    {
        ControlBoard = new int[Width * Height];
        GoalBoard = new int[Width * Height];

        EmptyBoards();
    }

    public int[] GetRandomLine(int max = 3)
    {
        int[] line = { 0, 0, 0, 0, 0 };
        int ones = 0;

        while (ones < max)
        {
            int i = Random.Range(0, line.Length);
            if (line[i] == 0)
            {
                line[i] = 1;
                ones += 1;
            }
        }


        return line;
    }

    public void StartGame()
    {
        PrepareBoards();
        
        PassNewLineToControlBoard(GetRandomLine(2));

        SwipeUpBlocksInControlBoard();
        PassNewLineToControlBoard(GetRandomLine(2));
       
        SwipeUpBlocksInControlBoard();
        PassNewLineToControlBoard(GetRandomLine(2));

        //CheckForGravityInControlBoard();

        RenderBlocks();
    }

    public void RestartGame()
    {
        ClearBlocks();

        EmptyBoards();
        PassNewLineToControlBoard(GetRandomLine(2));

        SwipeUpBlocksInControlBoard();
        PassNewLineToControlBoard(GetRandomLine(2));

        SwipeUpBlocksInControlBoard();
        PassNewLineToControlBoard(GetRandomLine(2));

        //CheckForGravityInControlBoard();

        RenderBlocks();
    }

    private void ClearBlocks()
    {
        foreach(Transform child in ControlParent)
        {
            Destroy(child.gameObject);
        }

        foreach (Transform child in GoalParent)
        {
            Destroy(child.gameObject);
        }
    }

    public void RenderNewLine()
    {
        for (int i = 0; i < Width; i++)
        {
            if (ControlBoard[i] == 1)
            {
                GameObject go = GameObject.Instantiate(BlockPrefab, Vector3.zero, Quaternion.identity);
                go.transform.SetParent(ControlParent);
                go.transform.localPosition = new Vector3(i, -1f, 0f);
                BlockAgent block = go.GetComponent<BlockAgent>();
                block.X = i;
                block.Y = -1;
                block.Value = 1;
                GameManager.instance.ControlBlocks.Add(block);
            }
        }
    }

    private void RenderBlocks()
    {
        GameManager.instance.ControlBlocks.Clear();

        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                if (ControlBoard[i + (Width * j)] == 1)
                {
                    GameObject go1 = GameObject.Instantiate(TilePrefab, Vector3.zero, Quaternion.identity);
                    go1.transform.SetParent(ControlParent);
                    go1.transform.localPosition = new Vector3(i, j, 0f);

                    GameObject go = GameObject.Instantiate(BlockPrefab, Vector3.zero, Quaternion.identity);
                    go.transform.SetParent(ControlParent);
                    go.transform.localPosition = new Vector3(i, j, 0f);
                    BlockAgent block = go.GetComponent<BlockAgent>();
                    block.X = i;
                    block.Y = j;
                    block.Value = 1;
                    block.BoardId = 0;
                    block.name = "block_" + (i * j).ToString();
                    GameManager.instance.ControlBlocks.Add(block);
                }
                else
                {
                    GameObject go = GameObject.Instantiate(TilePrefab, Vector3.zero, Quaternion.identity);
                    go.transform.SetParent(ControlParent);
                    go.transform.localPosition = new Vector3(i, j, 0f);
                }

                if (GoalBoard[i + (Width * j)] == 1)
                {
                    GameObject go2 = GameObject.Instantiate(TilePrefab, new Vector3(i, j, 0f), Quaternion.identity);
                    go2.transform.SetParent(GoalParent);
                    go2.transform.localPosition = new Vector3(i, j, 0f);

                    GameObject go = GameObject.Instantiate(BlockPrefab, Vector3.zero, Quaternion.identity);
                    go.transform.SetParent(GoalParent);
                    go.transform.localPosition = new Vector3(i, j, 0f);
                    BlockAgent block = go.GetComponent<BlockAgent>();
                    block.X = i;
                    block.Y = j;
                    block.Value = 1;
                    block.BoardId = 1;
                    //GameManager.instance.Blocks.Add(block);
                }
                else 
                {
                    GameObject go2 = GameObject.Instantiate(TilePrefab, new Vector3(i, j, 0f), Quaternion.identity);
                    go2.transform.SetParent(GoalParent);
                    go2.transform.localPosition = new Vector3(i, j, 0f);
                }
            }
        }
    }

    public void Redraw()
    {
        ClearBlocks();
        RenderBlocks();
    }

    public int SwipeUpBlocksInControlBoard()
    {
        for (int i = 0; i < Width; i++)
        {
            //Debug.Log("cb: " + ControlBoard[i + (Width * (Height - 1))] + " id: " + (i + (Width * (Height - 1))).ToString());
            if (ControlBoard[i + (Width * (Height - 1))] == 1)
                return -1;
        }

        for (int j = (Height - 1); j > 0; j--)
        {
            for (int i = 0; i < Width; i++)
            {
                ControlBoard[i + (Width * j)] = ControlBoard[i + (Width * (j - 1))];
            }
        }

        return 0;
    }

    public void CheckForGravityInControlBoard()
    {
        for (int i = 0; i < Width; i++)
        {
            for (int j = 1; j < Height; j++)
            {
                if (ControlBoard[i + (Width * j)] == 1)
                {
                    int y = j;
                    int lastY = y;
                    bool isBlockMoved = false;
                    while (y > 0)
                    {
                        y -= 1;
                        if (ControlBoard[i + (Width * y)] == 0)
                        {
                            ControlBoard[i + (Width * y)] = ControlBoard[i + (Width * (y + 1))];
                            ControlBoard[i + (Width * (y + 1))] = 0;

                            isBlockMoved = true;
                            lastY = y;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (isBlockMoved)
                    {
                        BlockAgent block = GameManager.instance.GetBlock(i, j, 0);

                        if (block != null)
                        {
                            //Debug.Log("blok " + block.name + " j: " + j + " y: " + y);
                            block.Y = lastY;
                        }
                    }
                }
            }
        }
    }

    public bool CheckForGravityInGoalBoard()
    {
        bool anyMovements = false;
        for (int i = 0; i < Width; i++)
        {
            for (int j = 1; j < Height; j++)
            {
                if (GoalBoard[i + (Width * j)] == 1)
                {
                    int y = j;
                    int lastY = y;
                    bool isBlockMoved = false;
                    while (y > 0)
                    {
                        y -= 1;
                        if (GoalBoard[i + (Width * y)] == 0)
                        {
                            GoalBoard[i + (Width * y)] = GoalBoard[i + (Width * (y + 1))];
                            GoalBoard[i + (Width * (y + 1))] = 0;

                            isBlockMoved = true;
                            anyMovements = true;
                            lastY = y;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (isBlockMoved)
                    {
                        BlockAgent block = GameManager.instance.GetBlock(i, j, 1);

                        if (block != null)
                        {
                            //Debug.Log("blok " + block.name + " j: " + j + " y: " + y);
                            block.Y = lastY;
                        }
                    }
                }
            }
        }

        return anyMovements;
    }

    public void PassNewLineToControlBoard(int[] newLine)
    {
        for (int i = 0; i < Width; i++)
        {
            ControlBoard[i] = newLine[i];
        }
    }


    public void MoveBlockInControlBoard(int x, int y, int dir)
    {
        int newPos = (x + (Width * y)) + dir;

        if (newPos >= 0 && newPos < (Width * Height))
        {
            if (ControlBoard[newPos] == 0)
            {
                ControlBoard[newPos] = ControlBoard[newPos - dir];
                ControlBoard[newPos - dir] = 0;

                //check if block go down
                //FallBlockInControlBoard((x + dir), y);
            }
        }
    }

    public int FallBlockInControlBoard(int x, int y, int fallAmount)
    {
        if (y > 0)
        {
            if (ControlBoard[(x + (Width * (y-1)))] == 0)
            {
                ControlBoard[(x + (Width * y))] = 0;
                ControlBoard[(x + (Width * (y - 1)))] = 1;

                fallAmount += 1;

                fallAmount = FallBlockInControlBoard(x, (y - 1), fallAmount);
            }
        }

        return fallAmount;
    }

    public bool CanPassLineFromControlToGoal(int y)
    {
        for (int i = 0; i < Width; i++)
        {
            if (ControlBoard[i + (Width * y)] == 1)
            {
                if (GoalBoard[i + (Width * y)] == 1)
                    return false;
            }
        }

        return true;
    }

    public void PassLineFromControlToGoal(int y)
    {
        /*
        for (int i = 0; i < Width; i++)
        {
            if (ControlBoard[i + (Width * y)] == 1)
            {
                if (GoalBoard[i + (Width * y)] == 1)
                    return;
            }
        }
        */

        for (int i = 0; i < Width; i++)
        {
            if (ControlBoard[i + (Width * y)] == 1 && GoalBoard[i + (Width * y)] == 0)
            {
                GoalBoard[i + (Width * y)] = 1;
                ControlBoard[i + (Width * y)] = 0;
            }
        }
    }

    public bool CheckIfGoalLineCompleted(int y)
    {
        //Debug.Log("checking for line: " + y);
        bool isLineFull = true;
        for (int i = 0; i < Width; i++)
        {
            //Debug.Log("line x: " + i + " y:"  + y + " val: " + GoalBoard[i + (Width * y)]);
            if (GoalBoard[i + (Width * y)] == 0)
            {
                isLineFull = false;
                break;
            }
        }

        return isLineFull;
    }

    public void ClearGoalLine(int y)
    {
        //Debug.Log("line cleared: " + y);

        for (int i = 0; i < Width; i++)
        {
            GoalBoard[i + (Width * y)] = 0;            
        }
    }


    public bool CanMoveBlockInControlBoard(int x, int y, int dir)
    {
        int newPos = (x + (Width * y)) + dir;

        if (newPos >= 0 && newPos < (Width * Height))
        {
            if (ControlBoard[newPos] == 0)
            {
                return true;
            }
        }

        return false;
    }


}
