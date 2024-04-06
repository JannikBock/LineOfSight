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

    private Color highlightColor = Color.cyan;
    private Color lowlightColor = Color.white;

    private float autoSaveTime = 20f;
    private float autoSaveTimer = 20f;
    private bool autoSaveNeeded;

    private GridMap<Tile> grid = new GridMap<Tile>();

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
                    break;
                case EditMode.Player:
                    TileButtonImage.color = lowlightColor;
                    PlayerButtonImage.color = highlightColor;
                    break;
                default:
                    break;
            }
        }
    }

    private DrawState drawState = DrawState.None;
    private float inputDelayTimer = 0f;
    private float inputDelayTime = 0.03f;

    private Vector2? lastMousePos = null;
    private float scrollWheelSpeed = 0.2f;
    private float minCameraSize = 1f;
    private float maxCameraSize = 16f;
    private float mouseMoveSpeed = 2f;

    private float cameraStartSize = 1f;

    private void Awake()
    {
        ReviveGameState();

    }
    private void Start()
    {
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

    private void ReviveGameState()
    {
        string serializedGrid = PlayerPrefs.GetString("GameState");
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
        // Draw with Mouse or Touch
        if (Input.GetMouseButton(0) && Input.touchCount <= 1 && (touchUse == TouchUse.None || touchUse == TouchUse.Draw))
        {

            var mousePos = Input.mousePosition;
            if (mousePos.x > Camera.main.pixelWidth - (60 * (Camera.main.pixelWidth / 800f)))
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
            Zoom(scrollWheelSpeed * scrollDelta);
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


        if (!Input.GetMouseButton(0))
        {
            // reset inputs
            lastMousePos = null;
            touchUse = TouchUse.None;
            drawState = DrawState.None;
            inputDelayTimer = 0f;
        }


    }


    private void PitchZoom()
    {
        Touch touchZero = Input.GetTouch(0);
        Touch touchOne = Input.GetTouch(1);

        Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
        Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

        float prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
        float currentMagnitude = (touchZero.position - touchOne.position).magnitude;

        float difference = currentMagnitude - prevMagnitude;

        Zoom(difference * 0.01f);
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

        if (lastMousePos == null)
        {
            lastMousePos = Input.mousePosition;
        }
        else
        {
            Vector2 currentMousePos = Input.mousePosition;
            Vector2 mouseDelta = (Vector2)lastMousePos - currentMousePos;

            float cameraSizeFactor = Camera.main.orthographicSize / cameraStartSize; //zoomed out camera moves faster
            Camera.main.transform.Translate(mouseDelta * mouseMoveSpeed * Time.deltaTime * cameraSizeFactor);
            lastMousePos = currentMousePos;
        }

    }

    private void TileClicked(Vector2Int clickPosition)
    {
        Debug.Log("Draw");
        touchUse = TouchUse.Draw;
        int x = clickPosition.x;
        int y = clickPosition.y;
        switch (EditMode)
        {
            case EditMode.Tile:
                Tile existingTile = grid[x, y];
                if (existingTile == null)
                {
                    if (drawState == DrawState.None || drawState == DrawState.Draw)
                    {
                        drawState = DrawState.Draw;
                        SpawnTile(clickPosition);
                        //Debug.Log($"Spawn Tile at {clickPosition}");
                        //GameObject newTileObj = Instantiate(TilePrefab, new Vector3(x, y), Quaternion.identity);
                        //Tile newTile = newTileObj.RequireComponent<Tile>();
                        //grid[x, y] = newTile;
                        //newTile.Init(this, clickPosition);
                    }

                }
                else
                {
                    if (drawState == DrawState.None || drawState == DrawState.Delete)
                    {
                        drawState = DrawState.Delete;
                        grid[x, y] = null;
                        Player playerToDestroy
                        = Players.FirstOrDefault(x => x != null && x.Pos == clickPosition);
                        if (playerToDestroy != null)
                        {
                            //players[x, y] = null;
                            Players[playerToDestroy.PlayerNumber]= null;
                            GameObject.Destroy(playerToDestroy.gameObject);
                        }
                        GameObject.Destroy(existingTile.gameObject);
                    }
                }
                break;
            case EditMode.Player:
                Tile tile = grid[x, y];
                if (tile != null)
                {
                    Player existingPlayer= Players.FirstOrDefault(x => x != null && x.Pos == clickPosition);
                    if (existingPlayer == null)
                    {
                        int nextPlayerIndex = Array.IndexOf(Players, null);
                        if (nextPlayerIndex < 0)
                        {
                            Debug.Log("Only 10 players are allowed at a time for performance reasons.");
                            return;
                        }
                        if (drawState == DrawState.None)
                        {
                            
                            drawState = DrawState.Single;
                            Debug.Log($"Spawn Player at {clickPosition}");
                            GameObject newPlayerObj = Instantiate(PlayerPrefab, new Vector3(x, y), Quaternion.identity);
                            Player newPlayer = newPlayerObj.RequireComponent<Player>();
                            newPlayer.Init(clickPosition, nextPlayerIndex);
                            Players[nextPlayerIndex] = newPlayer;
                        }
                    }
                    else
                    {
                        if (drawState == DrawState.None)
                        {
                            drawState = DrawState.Single;
                            Players[existingPlayer.PlayerNumber] = null;
                            GameObject.Destroy(existingPlayer.gameObject);
                        }
                    }
                }

                break;
            default:
                break;
        }

    }

    private void SpawnTile(Vector2Int pos)
    {
        if(grid[pos.x, pos.y] == null)
        {
            Debug.Log($"Spawn Tile at {pos}");
            GameObject newTileObj = Instantiate(TilePrefab, new Vector3(pos.x, pos.y), Quaternion.identity);
            Tile newTile = newTileObj.RequireComponent<Tile>();
            grid[pos.x, pos.y] = newTile;
            newTile.Init(this, pos);
        }

    }

    public void ChangeEditMode(int newMode)
    {
        EditMode = (EditMode)newMode;
    }

    public bool IsSeeThrough(int x, int y)
    {
        return grid[x, y] != null;
    }


    public void PersistGameState()
    {
        Debug.Log("Persisting Gamestate");
        GridPersistance gridToPersist = new GridPersistance(grid) ;
        string serializedGrid = JsonUtility.ToJson(gridToPersist);
        PlayerPrefs.SetString("GameState", serializedGrid);
        PlayerPrefs.Save();
    }
}
