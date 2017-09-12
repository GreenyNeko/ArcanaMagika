using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    public int id;
    private Sprite[] sprites;
    private Sprite spawnSprite;
    private Sprite defaultSprite;
    private Sprite removalSprite;
    private Sprite endangeredSprite;
    private bool combineAnimationDone = false;
    private byte state = 0;
    /*
     * 1st bit - change flag (1)
     * 2nd bit - spawn flag (2)
     * 3rd bit - swap flag (4)
     * 4th bit - falling flag (8)
     * 5th bit - removal flag (16)
     * 6th bit - endangered flag (32)
     * 7th bit - pressured flag (64)
     * 8th bit - gameover flag (128)
     */
    private Vector3 swapGoal;
    private float deathDelay = 0;
    private float deathTimer = 0;
    private float maxDelay = 0;
    private float fallTime = 0;
    private int savedCombo = 1;

    // Use this for initialization
	void Start ()
    {
        sprites = Resources.LoadAll<Sprite>("blockSheetRetro");
        spawnSprite = sprites[4 + id * 8];
        defaultSprite = sprites[0 + id * 8];
        removalSprite = sprites[6 + id * 8];
        endangeredSprite = sprites[1 + id * 8];
        state |= (1 << 1); // set spawn flag
        fallTime = 0;
    }

    private void FixedUpdate()
    {
        Vector2 currentVelocity = GetComponent<Rigidbody2D>().velocity;
        if(currentVelocity.y <= 0f)
        {
            return;
        }
        currentVelocity.y = 0f;
        GetComponent<Rigidbody2D>().velocity = currentVelocity;
    }

    // Update is called once per frame
    void Update ()
    {
        // Set respective animation
        if ((state & 128) == 128)
        {
            GetComponent<Animator>().Play("GameOver");
        }
        else if ((state & 32) == 32)
        {
            GetComponent<Animator>().Play("Endangered");
        }
        else if ((state & 64) == 64)
        {
            GetComponent<Animator>().Play("Pressured");
        }
        else if ((state & 16) == 16)
        {
            GetComponent<Animator>().Play("Combined");
        }
        else if ((state & 2) == 2)
        {
            GetComponent<Animator>().Play("Spawning");
        }
        else
        {
            GetComponent<Animator>().Play("Idle");
        }

        if (isSpawning())
        {
            // spawning can't fall
            setFalling(false); 
        }
        else if (isSetForRemoval())
        {
            // set for removal can't fall
            setFalling(false); 
            if(combineAnimationDone)
            {
                deathTimer += Time.deltaTime;
                if (deathTimer > 0.25f + deathDelay)
                {
                    if (GetComponent<SpriteRenderer>().enabled)
                    {
                        FindObjectOfType<PointUpdater>().addPoints(10 * savedCombo);
                        GetComponent<SpriteRenderer>().enabled = false;
                    }
                }
                if (deathTimer > maxDelay)
                {
                    Destroy(gameObject);
                }
            }
        }
        else
        {
            if ((state & 4) == 4) // if swapping
            {
                // can't fall if swapping
                setFalling(false);
                Vector3 diff = swapGoal - transform.position;
                if (diff.x < 0.3f && diff.x > -0.3f)
                {
                    transform.position = new Vector3(swapGoal.x, transform.position.y); // fix position
                    state &= 255 - (1 << 2);//swapping = false;
                    state |= 1;//changed = true;
                }
                else
                {
                    transform.Translate(new Vector3(diff.x, 0, 0) * 0.3f);
                }
            }
            if (isFalling())
            {
                fallTime += Time.deltaTime;
                if(fallTime > 3)
                {
                    fallTime = 3;
                }
                transform.Translate(new Vector2(0, -0.06f)); //0.03f = easy; 0.06f = normal; 0.09f = hard
            }
        }
	}

    // This method is called when combined animation is done to remove the block
    public void OnCombineAnimationDone()
    {
        combineAnimationDone = true;
    }

    // If there's a block directly below stop falling
    public void stopFalling()
    {
        // check left
        if (!Physics2D.Linecast(new Vector2(transform.position.x - 0.45f, transform.position.y - 0.51f), new Vector2(transform.position.x - 0.45f, transform.position.y - 0.7f)))
        {
            return;
        }
        // check middle
        if (!Physics2D.Linecast(new Vector2(transform.position.x, transform.position.y - 0.51f), new Vector2(transform.position.x, transform.position.y - 0.7f)))
        {
            return;
        }
        // check right
        if (!Physics2D.Linecast(new Vector2(transform.position.x + 0.45f, transform.position.y - 0.51f), new Vector2(transform.position.x + 0.45f, transform.position.y - 0.7f)))
        {
            return;
        }
        if (fallTime > 0.01)
        {
            GetComponent<Animator>().Play("Landed");
        }
        fallTime = 0;
        setFalling(false);
        state |= 1;// changed = true;
    }

    /*public void setGridPosition(int posX, int posY)
    {
        gridPosX = posX;
        gridPosY = posY;
    }*/

    public void setSwapGoal(Vector3 pos)
    {
        swapGoal = pos;
        state |= (1 << 2);//swapping = true;
    }

    public void setSpawned()
    {
        state &= 255 - (1 << 1);//spawning = false;
        state |= 1;//changed = true;
    }

    public void setPressured(bool newState)
    {
        if(newState)
        {
            state |= (1 << 6);//pressured = true
            // pressured blocks can't be endangered or gameover
            state &= 255 - (1 << 1);
            setEndangered(false);
            setGameOver(false);
        }
        else
        {
            state &= 255 - (1 << 6);//pressured = false
        }
    }

    public void setEndangered(bool newState)
    {
        if (newState)
        {
            state |= (1 << 5);//endangered = true
            state &= 255 - (1 << 1);
            setPressured(false);
            setGameOver(false);
        }
        else
        {
            state &= 255 - (1 << 5);//endangered = false
        }
    }

    public void setFalling(bool newState)
    {
        if (newState)
        {
            state |= (1 << 3);//falling = true
            state &= 255 - (1 << 1);
            state &= 255 - (1 << 2);
            state &= 255 - (1 << 4);
            setGameOver(false);
        }
        else
        {
            state &= 255 - (1 << 3);//falling = false
        }
    }

    public void setGameOver(bool newState)
    {
        if (newState)
        {
            state |= (1 << 7);//gameover = true
            state &= 255 - 1;
            state &= 255 - (1 << 1);
            state &= 255 - (1 << 2);
            setFalling(false);
            state &= 255 - (1 << 4);
            setEndangered(false);
            setPressured(false);
        }
        else
        {
            state &= 255 - (1 << 7);//gameover = false
        }
    }

    public bool isSwapping()
    {
        return (state & 4) == 4;
    }

    public bool isSpawning()
    {
        return (state & 2) == 2;
    }

    public int getId()
    {
        return id;
    }

    public Vector3 getSwapGoal()
    {
        return swapGoal;
    }

    public bool hasChanged()
    {
        return (state & 1) == 1;
    }

    public bool canSwap()
    {
        return !isFalling() && !isSpawning() && !isSetForRemoval();
    }

    public void markAsNotified(bool resetCombo)
    {
        state &= 255 - 1;//changed = false;
        if (resetCombo)
        {
            savedCombo = 1;
        }
    }

    public void setForRemoval(bool set, float delay, float maxDelay)
    {
        //removal = set;
        if(set)
        {
            state |= (1 << 4);
        }
        else
        {
            state &= 255 - (1 << 4);
        }
        deathDelay = delay;
        this.maxDelay = maxDelay;
    }

    public bool isSetForRemoval()
    {
        return (state & 16) == 16;
    }

    public bool isFalling()
    {
        return (state & 8) == 8;
    }

    public int getSavedCombo()
    {
        return savedCombo;
    }

    public void setSavedCombo(int combo)
    {
        savedCombo = combo;
    }
}
