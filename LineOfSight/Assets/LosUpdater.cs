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
            StartCoroutine(UpdateLos());
        }
    }

    private IEnumerator UpdateLos()
    {
        var allTiles = controller.Grid.GetAll().ToList();
        foreach (var tile in allTiles)
        {
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
            yield return null;
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
        if (!IsSeeThrough((int)cursor.x, (int)cursor.y, tile.Pos))
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
            if (!IsSeeThrough((int)roundedCursor.x, (int)roundedCursor.y, tile.Pos))
            {
                //Debug.Log($"Cell occupied {roundedCursor}");
                return false;
            }
            Vector2 roundedCursor2 = RoundVector(cursor + smallDelta * gradient);
            if (roundedCursor != roundedCursor2)
            {
                //Debug.Log("Secound point is on different tile.");
                //Debug.Log($"Visiting secound cell {roundedCursor2} for cursor {cursor}");
                if (!IsSeeThrough((int)roundedCursor2.x, (int)roundedCursor2.y, tile.Pos))
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

    private bool IsSeeThrough(int x, int y, Vector2Int pos)
    {
        return pos == new Vector2(x, y) || (
            controller.Grid[x, y] != null && !controller.Grid[x, y].HasObstacle);
    }

}
