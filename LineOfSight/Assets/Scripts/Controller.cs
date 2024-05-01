using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System;

public class Controller : MonoBehaviour
{
    [SerializeField]
    private GameObject TilePrefab;
    [SerializeField]
    private GameObject PlayerPrefab;

    [SerializeField]
    private Image TileButtonImage;
    [SerializeField]
    private Image PlayerButtonImage;
    [SerializeField]
    private Image ObstacleButtonImage;

    private Color highlightColor = Color.cyan;
    private Color lowlightColor = Color.white;

    private float autoSaveTime = 20f;
    private float autoSaveTimer = 20f;
    private bool autoSaveNeeded;

    private bool preventClick;

    public GridMap<Tile> Grid { get; set; } = new GridMap<Tile>();

    public Player[] Players
    {
        get;
        private set;
    } = new Player[Constants.MaxPlayerCount];

    private TouchUse touchUse = TouchUse.None; //to prevent drawing tiles when using multitouch
    private EditMode _editMode = EditMode.Tile;
    private EditMode EditMode
    {
        get
        {
            return _editMode;
        }
        set
        {
            _editMode = value;
            switch (_editMode)
            {
                case EditMode.Tile:
                    TileButtonImage.color = highlightColor;
                    PlayerButtonImage.color = lowlightColor;
                    ObstacleButtonImage.color = lowlightColor;
                    break;
                case EditMode.Obstacle:
                    TileButtonImage.color = lowlightColor;
                    PlayerButtonImage.color = lowlightColor;
                    ObstacleButtonImage.color = highlightColor;
                    break;
                default:
                    TileButtonImage.color = lowlightColor;
                    PlayerButtonImage.color = highlightColor;
                    ObstacleButtonImage.color = lowlightColor;
                    break;
            }
        }
    }

    private DrawState drawState = DrawState.None;
    private float inputDelayTimer = 0f;
    private float inputDelayTime = 0.03f;

    private Vector2? touchZeroPos = null;
    private Vector2? touchOnePos = null;
    private Vector2? lastMousePos = null;
    private float scrollWheelSpeed = 20f;
    private float minCameraSize = 2f;
    private float maxCameraSize = 16f;
    private float panSpeed = 1.7f;
    private float maxCameraPosition = 55f;

    private float cameraStartSize = 1f;

    private void Awake()
    {
        ReviveGameState();

    }
    private void Start()
    {
        ColorUtility.TryParseHtmlString(Constants.HighlightColor, out highlightColor);
        cameraStartSize = Camera.main.orthographicSize;
    }

    void Update()
    {
        HandleInput();

        autoSaveTimer -= Time.deltaTime;
        if(autoSaveTimer <= 0)
        {
            autoSaveTimer = autoSaveTime;
            PersistGameState();
            
        }
    }

    public void TruncateGrid()
    {
        foreach (var tile in Grid.GetAll().ToList())
        {
            RemoveTile(tile.Value);
        }
    }


    private void ReviveGameState()
    {
        string serializedGrid = PlayerPrefs.GetString(Constants.GameState);
        GridPersistance restoredGrid = JsonUtility.FromJson<GridPersistance>(serializedGrid);
        if(restoredGrid != null && restoredGrid.tiles != null)
        {
            Debug.Log("Restoring gamestate.");
            StartCoroutine(RestoreGrid(restoredGrid));
        }

    }

    private IEnumerator RestoreGrid(GridPersistance restoredGrid)
    {
        foreach (var tile in restoredGrid.tiles)
        {
            SpawnTile(tile.pos);
            yield return null;
        }
        Debug.Log("Game state resored.");
    }

    private void HandleInput()
    {
        //click is registered for the grid regardless if it hits a UI element first.
        // so we have to prevent placing grid tiles while dialogues are open.
        // switch to differetn input system if this workaround gets out of hand.
        if(preventClick)
        {
            return;
        }

        // Draw with Mouse or Touch
        if (Input.GetMouseButton(0) && Input.touchCount <= 1 && (touchUse == TouchUse.None || touchUse == TouchUse.Draw))
        {

            var mousePos = Input.mousePosition;
            if (mousePos.x > Camera.main.pixelWidth - ((180 + 4) * (Camera.main.pixelHeight / 500f)))
            {
                touchUse = TouchUse.UI;
                return;
            }
            Vector3 clickPosition = Camera.main.ScreenToWorldPoint(mousePos);
            //Wait for additional fingers to land
            if (inputDelayTimer < inputDelayTime)
            {
                inputDelayTimer += Time.deltaTime;
                return;
            }

            TileClicked(clickPosition.ToVector2Int());
            return;

        }

        // Pan with Mouse
        if ((Input.GetMouseButton(1) || Input.GetMouseButton(2)) && Input.touchCount == 0)
        {
            Pan();
            return;
        }

        // Scroll with wheel
        var scrollDelta = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scrollDelta) > 0.1)
        {
            Zoom(scrollWheelSpeed * scrollDelta*Time.deltaTime);
            return;
        }

        // pitch zoom or pan with touch
        if (Input.touchCount == 2 && (touchUse == TouchUse.None || touchUse == TouchUse.PanAndZoom))
        {
            touchUse = TouchUse.PanAndZoom;
            Pan();
            PitchZoom();
            return;
        }

        lastMousePos = null;
        touchZeroPos = null;
        touchOnePos = null;

        if (!Input.GetMouseButton(0))
        {
            // reset inputs
            touchUse = TouchUse.None;
            drawState = DrawState.None;
            inputDelayTimer = 0f;
            //Debug.Log("reset");
        }


    }


    private void PitchZoom()
    {
        Touch touchZero = Input.GetTouch(0);
        Touch touchOne = Input.GetTouch(1);

        if(touchZeroPos != null && touchOnePos != null)
        {
            float prevMagnitude = ((Vector2)(touchZeroPos - touchOnePos)).magnitude;
            float currentMagnitude = (touchZero.position - touchOne.position).magnitude;

            float difference = currentMagnitude - prevMagnitude;

            Zoom(difference * Time.deltaTime);
        }
        touchZeroPos = touchZero.position;
        touchOnePos = touchOne.position;
    }

    private void Zoom(float scrollDelta)
    {
        Debug.Log("Zoom");

        float newCameraSize = Mathf.Clamp(Camera.main.orthographicSize - scrollDelta, minCameraSize, maxCameraSize);
        Camera.main.orthographicSize = newCameraSize;
    }

    private void Pan()
    {
        Debug.Log("Pan");


        Vector2 currentMousePos = Input.mousePosition;
        
        if (lastMousePos != null)
        {
            Vector2 mouseDelta = ((Vector2)lastMousePos) - currentMousePos;

            float cameraSizeFactor = Camera.main.orthographicSize / cameraStartSize; //zoomed out camera moves faster
            //Debug.Log(mouseDelta);
            Camera.main.transform.Translate(mouseDelta * panSpeed * Time.deltaTime * cameraSizeFactor); // * new Vector2(100f/(float)Screen.width, 100f/ (float)Screen.height)
            float clampedXPos = Mathf.Clamp(Camera.main.transform.position.x, -maxCameraPosition, maxCameraPosition);
            float clampedYPos = Mathf.Clamp(Camera.main.transform.position.y, -maxCameraPosition, maxCameraPosition);
            Camera.main.transform.position = new Vector3(clampedXPos, clampedYPos, Camera.main.transform.position.z); ;

                lastMousePos = currentMousePos;
        }
        lastMousePos = currentMousePos;


    }

    private void TileClicked(Vector2Int clickPosition)
    {
        Debug.Log("Draw");
        touchUse = TouchUse.Draw;
        switch (EditMode)
        {
            case EditMode.Tile:
                DrawTile(clickPosition);
                break;
            case EditMode.Obstacle:
                DrawObstacle(clickPosition);

                break;
            default:
                DrawPlayer(clickPosition);
                break;
        }

    }

    private void DrawTile(Vector2Int pos)
    {
        Tile existingTile = Grid[pos.x, pos.y];
        if (existingTile == null)
        {
            if (drawState == DrawState.None || drawState == DrawState.Draw)
            {
                drawState = DrawState.Draw;
                SpawnTile(pos);
            }

        }
        else
        {
            if (drawState == DrawState.None || drawState == DrawState.Delete)
            {
                drawState = DrawState.Delete;
                RemoveTile(existingTile);
            }
        }
    }
    private void DrawObstacle(Vector2Int pos)
    {
        if (drawState != DrawState.None)
        {
            return;
        }
        drawState = DrawState.Single;

        Tile tile = Grid[pos.x, pos.y];
        if (tile == null)
        {
            return;
        }
        tile.HasObstacle = !tile.HasObstacle;
    }

    private void DrawPlayer(Vector2Int pos)
    {
        if (drawState != DrawState.None)
        {
            return;            
        }
        drawState = DrawState.Single;

        Tile tile = Grid[pos.x, pos.y];
        if (tile != null)
        {
            Player existingPlayer = Players.FirstOrDefault(x => x != null && x.Pos == pos);
            if (existingPlayer == null)
            {
                int nextPlayerIndex = Array.IndexOf(Players, null);
                if (nextPlayerIndex < 0)
                {
                    Debug.Log("Only 10 players are allowed at a time for performance reasons.");
                    return;
                }

                Debug.Log($"Spawn Player at {pos}");
                GameObject newPlayerObj = Instantiate(PlayerPrefab, pos.ToVector3(), Quaternion.identity);
                Player newPlayer = newPlayerObj.RequireComponent<Player>();
                newPlayer.Init(pos, nextPlayerIndex);
                Players[nextPlayerIndex] = newPlayer;

            }
            else
            {
                Players[existingPlayer.PlayerNumber] = null;
                GameObject.Destroy(existingPlayer.gameObject);
            }
        }

    }

    private void RemoveTile(Tile tile)
    {
        if(tile == null)
        {
            return;
        }
        Grid[tile.Pos.x, tile.Pos.y] = null;
        Player playerToDestroy
        = Players.FirstOrDefault(x => x != null && x.Pos == tile.Pos);
        if (playerToDestroy != null)
        {
            //players[x, y] = null;
            Players[playerToDestroy.PlayerNumber] = null;
            GameObject.Destroy(playerToDestroy.gameObject);
        }
        GameObject.Destroy(tile.gameObject);
    }

    private void SpawnTile(Vector2Int pos)
    {
        if(Grid[pos.x, pos.y] == null)
        {
            Debug.Log($"Spawn Tile at {pos}");
            GameObject newTileObj = Instantiate(TilePrefab, new Vector3(pos.x, pos.y), Quaternion.identity);
            Tile newTile = newTileObj.RequireComponent<Tile>();
            Grid[pos.x, pos.y] = newTile;
            newTile.Init(pos);
        }

    }

    public void PreventClick(bool prevent)
    {
        preventClick = prevent;
    }

    public void ChangeEditMode(int newMode)
    {
        EditMode = (EditMode)newMode;
    }


    public void PersistGameState()
    {
        Debug.Log("Persisting Gamestate");
        GridPersistance gridToPersist = new GridPersistance(Grid) ;
        string serializedGrid = JsonUtility.ToJson(gridToPersist);
        PlayerPrefs.SetString(Constants.GameState, serializedGrid);
        PlayerPrefs.Save();
    }
}
