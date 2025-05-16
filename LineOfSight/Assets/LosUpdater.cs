using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class LosUpdater : MonoBehaviour
{
    private bool coroutineRunning = false;
    private Controller controller;
    void Start()
    {
        controller = FindObjectOfType<Controller>();
    }

    
    void Update()
    {
        if (!coroutineRunning)
        {
            coroutineRunning = true;
            StartCoroutine(UpdateLos());
        }
    }

    private IEnumerator UpdateLos()
    {
        int index = 0;
        while(true)
        {
            
            KeyValuePair<Vector2Int, Tile> tile = controller.Grid.GetByIndex(index++);
            if(tile.Equals(default(KeyValuePair<Vector2Int, Tile>)) || tile.Value==null)
            {
                break;
            }

            bool playerInSight = false;

            for (int i = 0; i < Constants.MaxPlayerCount; i++)
            {
                var player = controller.Players[i];
                if (player == null)
                {
                    tile.Value.InSightHighlight[i].SetActive(false);
                    continue;
                }
                if (CanSeeTile(tile.Value, player.Pos))
                {
                    playerInSight = true;
                    tile.Value.InSightHighlight[i].SetActive(true);
                }
                else
                {
                    tile.Value.InSightHighlight[i].SetActive(false);
                }
            }

            tile.Value.IsInSight = playerInSight;
            if(index%5 == 0)
            {
                //yield return new WaitForSeconds(0.1f);
                yield return null;
            }

        }
        
        coroutineRunning = false;
    }

    public bool CanSeeTile(Tile tile, Vector2Int endCell)
    {
        //Debug.Log($"Input start cell {pos}");
        //Debug.Log($"Input end cell {endCell}");

        Vector2 gradient = endCell - tile.Pos;
        //Debug.Log($"Gradient {gradient}");

        int maxCursorCount;
        if (Mathf.Abs(gradient.x) > Mathf.Abs(gradient.y))
        {
            maxCursorCount = (int)(Mathf.Abs(gradient.x) * 2);
            gradient = gradient / (Mathf.Abs(gradient.x) * 2); //times two to also check quater steps
        }
        else
        {
            maxCursorCount = (int)(Mathf.Abs(gradient.y) * 2);
            gradient = gradient / (Mathf.Abs(gradient.y) * 2);
        }
        //Debug.Log($"Gradient devided {gradient}");
        //Debug.Log($"maxCursorCount {maxCursorCount}");

        Vector2 cursor = tile.Pos;
        //Debug.Log($"Visiting Start cell {cursor}");
        if (!IsSeeThrough(controller.Grid[(int)cursor.x, (int)cursor.y] , tile.Pos, endCell))
        {
            //Debug.Log($"Start Cell occupied {cursor}");
            return false;
        }


        Vector2 smallDelta = new Vector2(0.0001f, 0.0001f);
        for (int i = 0; i < maxCursorCount; i++)
        {
            cursor += gradient;

            Vector2 roundedCursor = RoundVector(cursor - smallDelta * gradient);
            //Debug.Log($"Visiting cell {roundedCursor} for cursor {cursor}");
            if (!IsSeeThrough(controller.Grid[(int)roundedCursor.x, (int)roundedCursor.y], tile.Pos, endCell))
            {
                //Debug.Log($"Cell occupied {roundedCursor}");
                return false;
            }
            Vector2 roundedCursor2 = RoundVector(cursor + smallDelta * gradient);
            if (roundedCursor != roundedCursor2)
            {
                //Debug.Log("Secound point is on different tile.");
                //Debug.Log($"Visiting secound cell {roundedCursor2} for cursor {cursor}");
                if (!IsSeeThrough(controller.Grid[(int)roundedCursor2.x, (int)roundedCursor2.y], tile.Pos, endCell))
                {
                    //Debug.Log($"Cell occupied {roundedCursor2}");
                    return false;
                }
            }
        }
        //Debug.Log($"Cell visible");
        return true;
    }

    private Vector2 RoundVector(Vector2 input)
    {
        return new Vector2((int)Mathf.Round(input.x), (int)Mathf.Round(input.y)); // no midpoint away from zero option :(
    }

    private bool IsSeeThrough(Tile t, Vector2Int startPos, Vector2Int endPos)
    {
        if(t == null)
        {
            return false;
        }
        return startPos == t.Pos ||
            endPos == t.Pos
            || !t.HasObstacle;
    }

}
