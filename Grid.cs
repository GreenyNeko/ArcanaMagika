using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid
{
    private Block[] gridPositions;
    private int sizeX, sizeY;
    private float axisXHeight = 0;

    // Initialize a grid area
    public Grid(int sizeX, int sizeY)
    {
        this.sizeX = sizeX;
        this.sizeY = sizeY;
        gridPositions = new Block[sizeX * sizeY + 1]; // 6 x 11
    }

    public int getWidth()
    {
        return sizeX;
    }

    public int getHeight()
    {
        return sizeY;
    }

    // Move the alignment axis upwards
    public void moveAxisX(float y)
    {
        axisXHeight += y;
        // If the alignment axis reaches the top push blocks upwards (in the array) and restart from the start
        if(axisXHeight >= 1)
        {
            axisXHeight = 0;
            pushUp();
        }
    }

    public int getSize()
    {
        return sizeX * sizeY;
    }

    // Add a block to the grid
    public void addBlock(int x, int y, Block block)
    {
        gridPositions[x + y * sizeX] = block;
    }

    // Get a block at a position in the grid
    public Block getBlock(int x, int y)
    {
        return gridPositions[x + y * sizeX];
    }

    // Get a list of blocks in a row
    public Block[] getRow(int y)
    {
        Block[] blocks = new Block[sizeX];
        for(int x = 0; x < sizeX; x++)
        {
            blocks[x] = gridPositions[x + y * sizeX];
        }
        return blocks;
    }

    // Get a list of blocks in a column
    public Block[] getColumn(int x)
    {
        Block[] blocks = new Block[sizeY];
        for (int y = 0; y < sizeY; y++)
        {
            blocks[y] = gridPositions[x + y * sizeX];
        }
        return blocks;
    }

    public Vector3 convertPositionToGrid(Vector3 pos)
    {
        return new Vector3(Mathf.FloorToInt(pos.x), Mathf.RoundToInt(pos.y - axisXHeight));
    }

    // Check if a spot in the grid is empty
    public bool isEmpty(int x, int y)
    {
        if (gridPositions[x + y * sizeX])
        {
            return false;
        }
        return true;
    }

    // Push all blocks in the grid to a higher level
    public void pushUp()
    {
        for(int x = 0; x < sizeX; x++)
        {
            for(int y = sizeY - 2; y >= 0; y--)
            {
                gridPositions[x + (y + 1) * sizeX] = gridPositions[x + y * sizeX];
            }
        }
    }

    // Realign all or all stationary blocks
    public void realignBlocksToGrid()
    {
        for(int x = 0; x < sizeX; x++)
        {
            for(int y = 0; y < sizeY; y++)
            {
                Block unalignedBlock = gridPositions[x + y * sizeX];
                if(unalignedBlock)
                {
                    if (!unalignedBlock.isFalling() && !unalignedBlock.isSwapping())
                    {
                        unalignedBlock.transform.SetPositionAndRotation(new Vector3(x, y + axisXHeight), unalignedBlock.transform.rotation);
                    }
                    else if(unalignedBlock.isFalling()) //unalignedBlock.isSwapping())
                    {
                        if (isEmpty(Mathf.FloorToInt(unalignedBlock.transform.position.x), Mathf.FloorToInt(unalignedBlock.transform.position.y)))
                        {
                            gridPositions[x + y * sizeX] = null;
                            gridPositions[Mathf.FloorToInt(unalignedBlock.transform.position.x) + Mathf.FloorToInt(unalignedBlock.transform.position.y) * sizeX] = unalignedBlock;
                        }
                    }
                }
            }
        }
    }

    // realign 
    public void realignBlockToBlock(Block block, Block anotherBlock)
    {
        if(block && anotherBlock)
        {
            if (!block.isFalling() && !anotherBlock.isFalling()
            || block.isFalling() && anotherBlock.isFalling())
            {
                block.transform.SetPositionAndRotation(new Vector3(block.transform.position.x, anotherBlock.transform.position.y), block.transform.rotation);
                anotherBlock.transform.SetPositionAndRotation(new Vector3(anotherBlock.transform.position.x, block.transform.position.y), block.transform.rotation);
            }
            else if (block.isFalling() && !anotherBlock.isFalling())
            {
                block.transform.SetPositionAndRotation(new Vector3(block.transform.position.x, anotherBlock.transform.position.y), block.transform.rotation);
            }
            else if (!block.isFalling() && anotherBlock.isFalling())
            {
                anotherBlock.transform.SetPositionAndRotation(new Vector3(anotherBlock.transform.position.x, block.transform.position.y), block.transform.rotation);
            }
        }
    }

    public void swap(int blockLeftX, int blockRightX, int y)
    {
        realignBlockToBlock(gridPositions[blockLeftX + y * sizeX], gridPositions[blockRightX + y * sizeX]);
        if (!isEmpty(blockLeftX, y) && !isEmpty(blockRightX, y))
        {
            gridPositions[blockLeftX + y * sizeX].setSwapGoal(gridPositions[blockRightX + y * sizeX].transform.position);
            gridPositions[blockRightX + y * sizeX].setSwapGoal(gridPositions[blockLeftX + y * sizeX].transform.position);
        }
        else if(isEmpty(blockLeftX, y))
        {
            gridPositions[blockRightX + y * sizeX].setSwapGoal(new Vector3(gridPositions[blockRightX + y * sizeX].transform.position.x - 1, y));
        }
        else if(isEmpty(blockRightX, y))
        {
            gridPositions[blockLeftX + y * sizeX].setSwapGoal(new Vector3(gridPositions[blockLeftX + y * sizeX].transform.position.x + 1, y));
        }
        Block tempBlock = gridPositions[blockLeftX + y * sizeX];
        gridPositions[blockLeftX + y * sizeX] = gridPositions[blockRightX + y * sizeX];
        gridPositions[blockRightX + y * sizeX] = tempBlock; 
    }

    public bool canSwap(int x1, int x2, int y)
    {
        if (gridPositions[x1 + y * sizeX].isFalling() && !isEmpty(x2, y))
        {
            return !gridPositions[x1 + y * sizeX].isSpawning() && !gridPositions[x1 + y * sizeX].isSetForRemoval() && !gridPositions[x2 + y * sizeX].isSpawning() && !gridPositions[x2 + y * sizeX].isSetForRemoval();
        }
        if (!gridPositions[x1 + y * sizeX].isFalling() && !isEmpty(x2, y))
        {
            return !gridPositions[x1 + y * sizeX].isSpawning() && !gridPositions[x1 + y * sizeX].isSetForRemoval() && !gridPositions[x2 + y * sizeX].isSpawning() && !gridPositions[x2 + y * sizeX].isSetForRemoval();
        }
        if (gridPositions[x1 + y * sizeX].isFalling() && isEmpty(x2, y))
        {
            return !gridPositions[x1 + y * sizeX].isSpawning() && !gridPositions[x1 + y * sizeX].isSetForRemoval();
        }
        if (!gridPositions[x1 + y * sizeX].isFalling() && isEmpty(x2, y))
        {
            return !gridPositions[x1 + y * sizeX].isSpawning() && !gridPositions[x1 + y * sizeX].isSetForRemoval();
        }
        return false;
    }

    public int getHighestColumn()
    {
        int max = 0;
        int columnMax = 0;
        for(int x = 0; x < sizeX; x++)
        {
            if(max < getColumnHeight(x))
            {
                max = getColumnHeight(x);
                columnMax = x;
            }
        }
        return columnMax;
    }

    public int getColumnHeight(int x)
    {
        for(int y = 0; y < sizeY; y++)
        {
            if(isEmpty(x, y))
            {
                return y;
            }
        }
        return sizeY;
    }

    public bool blockOnTop()
    {
        for(int x = 0; x < sizeX; x++)
        {
            if (!isEmpty(x, sizeY - 1))
            {
                return true;
            }
        }
        return false;
    }
}
