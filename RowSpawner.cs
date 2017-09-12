using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RowSpawner : MonoBehaviour {

    public GameObject blockParent;
    public Block[] blocks;

    public void spawnNext(Grid grid)
    {
        int[] randInts = new int[6];
        for (int i = 0; i < 6; i++)
        {
            randInts[i] = Random.Range(0, 6);
        }

        grid.addBlock(0, 0, Instantiate<Block>(blocks[randInts[0]], transform.position, Quaternion.identity, blockParent.transform));
        grid.addBlock(1, 0, Instantiate<Block>(blocks[randInts[1]], transform.position + new Vector3(1, 0, 0), Quaternion.identity, blockParent.transform));
        grid.addBlock(2, 0, Instantiate<Block>(blocks[randInts[2]], transform.position + new Vector3(2, 0, 0), Quaternion.identity, blockParent.transform));
        grid.addBlock(3, 0, Instantiate<Block>(blocks[randInts[3]], transform.position + new Vector3(3, 0, 0), Quaternion.identity, blockParent.transform));
        grid.addBlock(4, 0, Instantiate<Block>(blocks[randInts[4]], transform.position + new Vector3(4, 0, 0), Quaternion.identity, blockParent.transform));
        grid.addBlock(5, 0, Instantiate<Block>(blocks[randInts[5]], transform.position + new Vector3(5, 0, 0), Quaternion.identity, blockParent.transform));
    }
}
