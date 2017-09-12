using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    private float lastTick;
    public static int w = 6;
    public static int h = 24;
    private float totalMovement = 0.0f;
    private Vector3 relativePosition;
    private Vector3 mouseStart;
    private Vector3 touchStart;
    private Rect spawnArea;
    private float cooldown = 0;
    private float stopTime = 0;
    private Grid grid;
    private bool awaitsInput = true;
    private bool gameover = false;
    // Use this for initialization
    void Start ()
    {
        lastTick = 0;
        spawnArea = new Rect(0, 0, 0, 1.0f);
        grid = new Grid(6, 12);
    }
	
	// Update is called once per frame
	void Update ()
    {
        if(!gameover)
        {
            if (Time.time - lastTick > 0.1f)
            {
                // no moving blocks when blocks are still active
                if (!hasRemovingBlocks())
                {
                    moveGrid(false);
                }
                lastTick = Time.time;
            }

            if (stopTime >= 0)
            {
                stopTime -= Time.deltaTime;
            }
            else if (stopTime <= 0)
            {
                stopTime = 0;
            }

            // Handle player input
            updateGrid();
            handleInputWindows();
            handleInputAndroid();
            checkGrid();
        }
    }

    void handleInputWindows()
    {
        
        if (awaitsInput)
        {
            if (Input.GetMouseButtonDown(0))
            {
                mouseStart = FindObjectOfType<Camera>().ScreenToWorldPoint(Input.mousePosition);
                relativePosition = mouseStart - transform.position;
            }
            if (Input.GetMouseButton(0))
            {
                Vector2 mouseDeltaPosition = mouseStart - FindObjectOfType<Camera>().ScreenToWorldPoint(Input.mousePosition);
                if (Mathf.RoundToInt(mouseStart.x) >= 0 && Mathf.RoundToInt(mouseStart.x) < 6
                    && Mathf.RoundToInt(mouseStart.y) >= 0 && Mathf.RoundToInt(mouseStart.y) < 12)
                {
                    if (mouseDeltaPosition.y < -1.0f)
                    {
                        stopTime = 0;
                        moveGrid(true);
                    }
                    else if (mouseDeltaPosition.x > 0.5f)
                    {
                        Vector2 gridPos = worldPosToGrid(mouseStart);
                        Block startBlock = grid.getBlock((int)grid.convertPositionToGrid(relativePosition).x, (int)grid.convertPositionToGrid(relativePosition).y);
                        // moving mouse left
                        if (Mathf.RoundToInt(gridPos.x) > 0)
                        {
                            if (startBlock && grid.canSwap((int)grid.convertPositionToGrid(relativePosition).x, (int)grid.convertPositionToGrid(relativePosition).x - 1, (int)grid.convertPositionToGrid(relativePosition).y) && !hasSwappingBlocks())
                            {
                                grid.swap((int)grid.convertPositionToGrid(relativePosition).x - 1, (int)grid.convertPositionToGrid(relativePosition).x, (int)grid.convertPositionToGrid(relativePosition).y);
                                awaitsInput = false;
                            }
                        }
                    }
                    else if (mouseDeltaPosition.x < -0.5f)
                    {
                        // moving mouse right
                        Vector2 gridPos = worldPosToGrid(mouseStart);
                        Block startBlock = grid.getBlock((int)grid.convertPositionToGrid(relativePosition).x, (int)grid.convertPositionToGrid(relativePosition).y);
                        if (Mathf.RoundToInt(gridPos.x) < 5)
                        {
                            if (startBlock && grid.canSwap((int)grid.convertPositionToGrid(relativePosition).x, (int)grid.convertPositionToGrid(relativePosition).x + 1, (int)grid.convertPositionToGrid(relativePosition).y) && !hasSwappingBlocks())
                            {
                                grid.swap((int)grid.convertPositionToGrid(relativePosition).x, (int)grid.convertPositionToGrid(relativePosition).x + 1, (int)grid.convertPositionToGrid(relativePosition).y);
                                awaitsInput = false;
                            }
                        }
                    }
                }
            }
        }
        if(Input.GetMouseButtonUp(0))
        {
            awaitsInput = true;
        }
    }

    void handleInputAndroid()
    {
        // Android
        if (awaitsInput)
        {
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                touchStart = FindObjectOfType<Camera>().ScreenToWorldPoint(Input.GetTouch(0).position);
                relativePosition = touchStart - transform.position;
            }
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
            {
                Vector2 touchDeltaPosition = touchStart - FindObjectOfType<Camera>().ScreenToWorldPoint(Input.GetTouch(0).position);
                if (Mathf.RoundToInt(touchStart.x) > 0 && Mathf.RoundToInt(touchStart.x) < 6
                    && Mathf.RoundToInt(touchStart.y) > 0 && Mathf.RoundToInt(touchStart.y) < 12)
                {
                    if (touchDeltaPosition.y < -1.0f)
                    {
                        stopTime = 0;
                        moveGrid(true);
                    }
                    else if (touchDeltaPosition.x > 0.5f)
                    {
                        Vector2 gridPos = worldPosToGrid(touchStart);
                        Block startBlock = grid.getBlock((int)grid.convertPositionToGrid(relativePosition).x, (int)grid.convertPositionToGrid(relativePosition).y);
                        if (Mathf.RoundToInt(gridPos.x) > 0)
                        {
                            if (startBlock && grid.canSwap((int)grid.convertPositionToGrid(relativePosition).x - 1, (int)grid.convertPositionToGrid(relativePosition).x, (int)grid.convertPositionToGrid(relativePosition).y) && !hasSwappingBlocks())
                            {
                                grid.swap((int)grid.convertPositionToGrid(relativePosition).x - 1, (int)grid.convertPositionToGrid(relativePosition).x, (int)grid.convertPositionToGrid(relativePosition).y);
                                awaitsInput = false;
                            }
                        }
                    }
                    else if (touchDeltaPosition.x < -0.5f)
                    {
                        Vector2 gridPos = worldPosToGrid(touchStart);
                        Block startBlock = grid.getBlock((int)grid.convertPositionToGrid(relativePosition).x, (int)grid.convertPositionToGrid(relativePosition).y);
                        if (Mathf.RoundToInt(gridPos.x) < 5)
                        {
                            if (startBlock && grid.canSwap((int)grid.convertPositionToGrid(relativePosition).x, (int)grid.convertPositionToGrid(relativePosition).x + 1, (int)grid.convertPositionToGrid(relativePosition).y))
                            {
                                grid.swap((int)grid.convertPositionToGrid(relativePosition).x, (int)grid.convertPositionToGrid(relativePosition).x + 1, (int)grid.convertPositionToGrid(relativePosition).y);
                                awaitsInput = false;
                            }
                        }
                    }
                }
            }
        }
        if(Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
        {
            awaitsInput = true;
        }
    }

    // Do various things to keep the grid up-to-date
    void updateGrid()
    {
        keepComboAlive();
        // Check if blocks can fall
        checkFalling();
        // re-align all blocks
        grid.realignBlocksToGrid();
        // Set spawned blocks in a row higher than 0 as spawned blocks
        Block[] spawnedBlocks = grid.getRow(1);
        foreach(Block spawnedBlock in spawnedBlocks)
        {
            if(spawnedBlock)
            {
                spawnedBlock.setSpawned();
            }
        }
        int highestColumn = grid.getColumnHeight(grid.getHighestColumn());
        for (int x = 0; x < grid.getWidth(); x++)
        {
            if (grid.getColumnHeight(x) == 12)
            {
                if(stopTime <= 0)
                {
                    foreach (Block block in grid.getColumn(x))
                    {
                        block.setGameOver(true);
                        gameover = true;
                    }
                }
                else
                {
                    foreach (Block block in grid.getColumn(x))
                    {
                        if (block && !block.isSpawning())
                        {
                            block.setEndangered(false);
                            block.setPressured(true);
                        }
                    }
                }
                
            }
            else if (grid.getColumnHeight(x) == highestColumn && highestColumn > 9)
            {
                foreach (Block block in grid.getColumn(x))
                {
                    if(block && !block.isSpawning())
                    {
                        block.setPressured(false);
                        block.setEndangered(true);
                    } 
                }
            }
            else
            {
                foreach (Block block in grid.getColumn(x))
                {
                    if (block)
                    {
                        block.setPressured(false);
                        block.setEndangered(false);
                    }
                }
            }
        }
    }

    // Update combo of blocks above blocks set for removal
    void keepComboAlive()
    {
        for (int x = 0; x < grid.getWidth(); x++)
        {
            for (int y = 1; y < grid.getHeight(); y++)
            {
                if (!grid.isEmpty(x, y) && !grid.getBlock(x, y).isSetForRemoval() && !grid.getBlock(x, y).isSwapping())
                {
                    if(!grid.isEmpty(x, y - 1))
                    {
                        if(grid.getBlock(x, y - 1).isSetForRemoval())
                        {
                            grid.getBlock(x, y).setSavedCombo(grid.getBlock(x, y - 1).getSavedCombo() + 1);
                        }
                    }
                }
            }
        }
    }

    // Check each and every block if there's space below.. if so flag it to fall
    void checkFalling()
    {
        for(int x = 0; x < grid.getWidth(); x++)
        {
            for(int y = 1; y < grid.getHeight(); y++)
            {
                if(!grid.isEmpty(x, y))
                {
                    if(!grid.getBlock(x, y).isSwapping() && !grid.getBlock(x, y).isSpawning())
                    {
                        if (grid.isEmpty(x, y - 1))
                        {
                            grid.getBlock(x, y).setFalling(true);
                        }
                        else if(grid.getBlock(x, y - 1).isSwapping())
                        {
                            grid.getBlock(x, y).setFalling(false);
                        }
                        else if (grid.getBlock(x, y - 1).isFalling())
                        {
                            grid.getBlock(x, y).setFalling(true);
                        }
                        else
                        {
                            grid.getBlock(x, y).stopFalling();
                        }
                    }
                }
            }
        }
    }

    Vector2 worldPosToGrid(Vector3 pos)
    {
        return new Vector2(pos.x, pos.y);
    }

    // move the graphics up by a little bit
    void moveGrid(bool manually)
    {
        if(!grid.blockOnTop() && stopTime <= 0)
        {
            if (manually)
            {
                //transform.position += new Vector3(0, 0.025f, 0);
                grid.moveAxisX(0.025f);
                totalMovement += 0.025f;
            }
            else
            {
                //transform.position += new Vector3(0, 0.01f * FindObjectOfType<PointUpdater>().getLevel(), 0);
                grid.moveAxisX(0.01f * FindObjectOfType<PointUpdater>().getLevel());
                totalMovement += 0.01f * FindObjectOfType<PointUpdater>().getLevel();
            }

            if (totalMovement > 1.0f)
            {
                FindObjectOfType<RowSpawner>().spawnNext(grid);
                FindObjectOfType<PointUpdater>().addPoints(1);
                totalMovement = 0.0f;
            }
        }
    }

    bool hasSwappingBlocks()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Block block = transform.GetChild(i).GetComponent<Block>();
            if(block.isSwapping())
            {
                return true;
            }
        }
        return false;
    }

    bool hasRemovingBlocks()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Block block = transform.GetChild(i).GetComponent<Block>();
            if (block.isSetForRemoval())
            {
                return true;
            }
        }
        return false;
    }

    // check for combos
    void checkGrid()
    {
        ArrayList removalList = new ArrayList();
        for (int w = 0; w < grid.getWidth(); w++)
        {
            for(int h = 0; h < grid.getHeight(); h++)
            {
                bool resetCombo = true;
                Block block = grid.getBlock(w, h);
                if(block && block.hasChanged())
                {
                    if (!block.isSpawning() && !block.isSwapping()
                        && !block.isSetForRemoval() && !block.isFalling())
                    {
                        int matchWidthLeft = 0;
                        int matchWidthRight = 0;
                        int matchHeightUp = 0;
                        int matchHeightDown = 0;
                        for (int cw = w; cw >= 0; cw--)
                        {
                            Block checkBlock = grid.getBlock(cw, h);
                            if (!checkBlock || checkBlock.isSpawning() || checkBlock.isFalling()
                                || checkBlock.isSwapping() || checkBlock.isSetForRemoval())
                            {
                                matchWidthLeft = w - cw - 1;
                                break;
                            }
                            if (block.getId() != checkBlock.getId())
                            {
                                matchWidthLeft = w - cw - 1;
                                break;
                            }
                            matchWidthLeft = w - cw;
                        }
                        for (int cw = w; cw < grid.getWidth(); cw++)
                        {
                            Block checkBlock = grid.getBlock(cw, h);
                            if (!checkBlock || checkBlock.isSpawning() || checkBlock.isFalling()
                                || checkBlock.isSwapping() || checkBlock.isSetForRemoval())
                            {
                                matchWidthRight = cw - w - 1;
                                break;
                            }
                            if (block.getId() != checkBlock.getId())
                            {
                                matchWidthRight = cw - w - 1;
                                break;
                            }
                            matchWidthRight = cw - w;
                        }
                        for (int ch = h; ch > 0; ch--)
                        {
                            Block checkBlock = grid.getBlock(w, ch);
                            if (!checkBlock || checkBlock.isSpawning() || checkBlock.isFalling()
                                || checkBlock.isSwapping() || checkBlock.isSetForRemoval())
                            {
                                matchHeightDown = h - ch - 1;
                                break;
                            }
                            if (block.getId() != checkBlock.getId())
                            {
                                matchHeightDown = h - ch - 1;
                                break;
                            }
                            matchHeightDown = h - ch;
                        }
                        for (int ch = h; ch < grid.getHeight() - 1; ch++)
                        {
                            Block checkBlock = grid.getBlock(w, ch);
                            if (!checkBlock || checkBlock.isSpawning() || checkBlock.isFalling()
                                || checkBlock.isSwapping() || checkBlock.isSetForRemoval())
                            {
                                matchHeightUp = ch - h - 1;
                                break;
                            }
                            if (block.getId() != checkBlock.getId())
                            {
                                matchHeightUp = ch - h - 1;
                                break;
                            }
                            matchHeightUp = ch - h;
                        }
                        if (matchWidthLeft + matchWidthRight + 1 >= 3)
                        {
                            for (int i = -matchWidthLeft; i <= matchWidthRight; i++)
                            {
                                Block removalBlock = grid.getBlock(w + i, h);
                                removalBlock.setSavedCombo(block.getSavedCombo());
                                removalList.Add(removalBlock);
                            }
                            resetCombo = false;
                        }
                        if (matchHeightDown + matchHeightUp + 1 >= 3)
                        {
                            for (int i = -matchHeightDown; i <= matchHeightUp; i++)
                            {
                                Block removalBlock = grid.getBlock(w, h + i);
                                removalBlock.setSavedCombo(block.getSavedCombo());
                                removalList.Add(removalBlock);
                            }
                            resetCombo = false;
                        }
                    }
                    block.markAsNotified(resetCombo);
                    if (!resetCombo)
                    {
                        Debug.Log(block.getSavedCombo() + "x Combo");
                        if(block.getSavedCombo() > 1)
                        {
                            stopTime = 6 + (block.getSavedCombo() - 2);
                        }
                        if(removalList.Count > 3)
                        {
                            stopTime = 1 + Mathf.FloorToInt((removalList.Count / 2));
                        }
                    }
                }
                //removalList.Reverse();
                if(removalList.Count > 0)
                {
                    for (int i = 0; i < removalList.Count; i++)
                    {
                        Block removalBlock = (Block)removalList[i];
                        if (block)
                        {
                            removalBlock.setForRemoval(true, i * 0.25f, removalList.Count * 0.25f);
                        }
                    }
                }
                removalList.Clear();
            }
        }
    }

    public Rect getSpawnArea()
    {
        return spawnArea;
    }
}
