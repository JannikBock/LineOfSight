using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Tile : MonoBehaviour
{

    [SerializeField]
    GameObject[] InSightHighlight;


    private bool _isInSight = false;
    public bool IsInSight
    {
        get { return _isInSight; }
        set
        {
            _isInSight = value;
        }
    }

    public Vector2Int Pos
    {
        get;
        private set;
    }

    public void Init(Controller inController, Vector2Int inPos)
    {
        controller = inController;
        Pos = inPos;
    }

    public bool CanSeeTile(Vector2Int endCell)
    {
        //Debug.Log($"Input start cell {pos}");
        //Debug.Log($"Input end cell {endCell}");

        Vector2 gradient = endCell - Pos;
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

        Vector2 cursor = Pos;
        //Debug.Log($"Visiting Start cell {cursor}");
        if (!controller.IsSeeThrough((int)cursor.x, (int)cursor.y))
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
            if (!controller.IsSeeThrough((int)roundedCursor.x, (int)roundedCursor.y))
            {
                //Debug.Log($"Cell occupied {roundedCursor}");
                return false;
            }
            Vector2 roundedCursor2 = RoundVector(cursor + smallDelta * gradient);
            if (roundedCursor != roundedCursor2)
            {
                //Debug.Log("Secound point is on different tile.");
                //Debug.Log($"Visiting secound cell {roundedCursor2} for cursor {cursor}");
                if (!controller.IsSeeThrough((int)roundedCursor2.x, (int)roundedCursor2.y))
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


    private Controller controller;

    private bool coroutineRunning = false;
    public void Update()
    {
        if(!coroutineRunning)
        {
            coroutineRunning = true;
            StartCoroutine(SetTilesVisible());
        }
    }

    private IEnumerator SetTilesVisible()
    {
        bool playerInSight = false;

        for (int i = 0; i < Constants.MaxPlayerCount; i++)
        {
            var player = controller.Players[i];
            if(player== null)
            {
                InSightHighlight[i].SetActive(false);
                continue;
            }
            if (CanSeeTile(player.Pos))
            {
                playerInSight = true;
                InSightHighlight[i].SetActive(true);
            }
            else
            {
                InSightHighlight[i].SetActive(false);
            }

            yield return null;
        }

        IsInSight = playerInSight;
        yield return new WaitForSeconds(1);
        coroutineRunning = false;
    }

}
