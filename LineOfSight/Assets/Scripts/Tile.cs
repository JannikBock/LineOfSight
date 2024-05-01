using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Tile : MonoBehaviour
{

    [SerializeField]
    public GameObject[] InSightHighlight;
    [SerializeField]
    GameObject Obstacle;


    private bool _isInSight = false;
    public bool IsInSight
    {
        get { return _isInSight; }
        set
        {
            _isInSight = value;
        }
    }

    private bool _hasObstacle = false;
    public bool HasObstacle
    {
        get { return _hasObstacle; }
        set
        {
            _hasObstacle = value;
            Obstacle.SetActive(_hasObstacle);
        }
    }

    public Vector2Int Pos
    {
        get;
        private set;
    }

    public void Init(Vector2Int inPos)
    {
        Pos = inPos;
    }

}
