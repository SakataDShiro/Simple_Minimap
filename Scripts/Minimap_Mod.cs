using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using DaggerfallWorkshop.Game.Serialization;
using MinimapLocatorMod;

public class Minimap_Mod : MonoBehaviour
{
    private static Mod mod; //Define mod variable

    //----------------------------------Minimap Variables------------------------------------
  
    public static float minimapSizePercent = 0.23f; //Minimap Size
    public static int shortestSide = Mathf.Min(Screen.width, Screen.height);
    public static int textureWidthGUI = (int)(shortestSide * minimapSizePercent); // Render Texture Width
    public static int textureHeightGUI = textureWidthGUI; // Render Texture Height
    public static int xAdded = 0;
    public static int yAdded = 0;
    public float height = 50f; // Camera Height
    public float interiorHeight = 1f; //Interior camera height
    public static bool allowRotationMap; //Allow Rotation
    public static bool allowCompass; //Allow Compass
    public static bool allowIndoorMinimap; //Allow Indoor
    public static int actualMinimapShape; //Minimap Shape
    public static int distanceCoveredInsideOption;//Selected Option
    public static int distanceCoveredOutsideOption;//Selected Option
    public static float transparency = 1.0f; // Transparency
    public static float updateFrequency = 0.08f; //Frequency
    public static KeyCode toggleKey;
    public float farClipPlane = 5f; //Far clip

    private RenderTexture previousRenderTexture; //Render Texture of Main Camera
    private Camera mainCamera; //Main Camera

    //---------------------------------GUI VARIABLES---------------------------------------
    public static int depthModifier; //GUI Modifier to other interfaces

    private Rect minimapRect; // Size and position
    private Rect arrowRect;// Size and position
    private Rect markerRect;// Size and position
    private Rect compassRect;// Size and position

    private static Texture2D arrowTexture;
    private static Texture2D compassTexture;
    private float compassRotation;

    private Dictionary<string, bool> markerVisibility = new Dictionary<string, bool>() //To Show Nearest Markers
        {
            { "Alchemist", false },
            { "Armorer", false },
            { "HouseForSale", false },
            { "Bank", false },
            { "Bookseller", false },
            { "ClothingStore", false },
            { "GemStore", false },
            { "GeneralStore", false },
            { "GuildHall", false },
            { "Library", false },
            { "Palace", false },
            { "PawnShop", false },
            { "Tavern", false },
            { "Temple", false },
            { "WeaponSmith", false }
        };


    private Dictionary<string, Vector3> closestMarkerPositions = new Dictionary<string, Vector3>(); //Vector to get closest marker of each on dictionary
    private Dictionary<string, Texture2D> closestMarkerTextures = new Dictionary<string, Texture2D>(); //to get the textures of each
    private Dictionary<string, float> closestMarkerRotations = new Dictionary<string, float>(); //to define rotation of each
    private float minDistanceToHideMarker; // Distance to hide marker
    private float fadeDistance; // Distance to start hiding with fade
    private Vector3 closestMarkerPosition; //The position of each
    private Texture2D closestMarkerTexture; //The texture of each


    //----------------------------------References----------------------------------------------
    private Transform player; // Player reference
    public Camera miniMapCamera; // Minimap Camera
    private PlayerEnterExit playerEnterExit; // PlayerEnterExit component
    private bool renderMap;
    private bool renderMapToggle = true;

    //---------------------------------------INVOKE-------------------------------------------------
    [Invoke(StateManager.StateTypes.Game, 0)]
    public static void Init(InitParams initParams)
    {
        mod = initParams.Mod;
        mod.LoadSettingsCallback = (settings, change) =>
        {
            var instance = FindObjectOfType<Minimap_Mod>();
            if (instance != null)
            {
                instance.LoadSettings(settings, change);
            }

            var locatorMod = FindObjectOfType<MinimapLocator>();
            if (locatorMod != null)
            {
                locatorMod.LoadSettings(settings, change);
            }

        };
        mod.LoadSettings(); //Load Settings

        var go = new GameObject(mod.Title);
        go.AddComponent<Minimap_Mod>();//Add component to the GameObject
        MinimapLocator minimapLocator = go.AddComponent<MinimapLocator>();
    }

    private void LoadSettings(ModSettings settings, ModSettingsChange change)
    {
        //-------------------- Check if Settings has changed---------------------------
        if (change.HasChanged("UI Settings") || change.HasChanged("Map View") || change.HasChanged("Building Markers"))
        {
            
            int minimapPosition = settings.GetValue<int>("UI Settings", "Position");
            int minimapXOffset = settings.GetValue<int>("UI Settings", "X Offset");
            int minimapYOffset = settings.GetValue<int>("UI Settings", "Y Offset");
            float minimapTransparency = settings.GetValue<float>("UI Settings", "Transparency");
            int depthSelected = settings.GetValue<int>("UI Settings", "GUI Depth Modifier");
            int refreshUpdateSelected = settings.GetValue<int>("UI Settings", "Update Frequency");
            bool rotationMap = settings.GetValue<bool>("UI Settings", "Rotation");
            bool compassMap = settings.GetValue<bool>("UI Settings", "Integrated Compass");
            string keyInputString = settings.GetValue<string>("UI Settings", "KeyInput");

            int distanceCoveredOutside = settings.GetValue<int>("Map View", "Distance Covered Outside");
            int distanceCoveredInside = settings.GetValue<int>("Map View", "Distance Covered Inside");
            bool indoorMinimap = settings.GetValue<bool>("Map View", "Indoor Minimap");

            bool showNearAlchemist = settings.GetValue<bool>("Building Markers", "Show Nearest Alchemist Building");
            bool showNearArmorer = settings.GetValue<bool>("Building Markers", "Show Nearest Armorer Building");
            bool showNearHouseForSale = settings.GetValue<bool>("Building Markers", "Show Nearest House For Sale Building");
            bool showNearBank = settings.GetValue<bool>("Building Markers", "Show Nearest Bank Building");
            bool showNearBookseller = settings.GetValue<bool>("Building Markers", "Show Nearest Bookseller Building");
            bool showNearClothingStore = settings.GetValue<bool>("Building Markers", "Show Nearest Clothing Store Building");
            bool showNearGemStore = settings.GetValue<bool>("Building Markers", "Show Nearest Gem Store Building");
            bool showNearGeneralStore = settings.GetValue<bool>("Building Markers", "Show Nearest General Store Building");
            bool showNearGuildHall = settings.GetValue<bool>("Building Markers", "Show Nearest Guild Hall Building");
            bool showNearLibrary = settings.GetValue<bool>("Building Markers", "Show Nearest Library Building");
            bool showNearPalace = settings.GetValue<bool>("Building Markers", "Show Nearest Palace Building");
            bool showNearPawnShop = settings.GetValue<bool>("Building Markers", "Show Nearest Pawn Shop Building");
            bool showNearTavern = settings.GetValue<bool>("Building Markers", "Show Nearest Tavern Building");
            bool showNearTemple = settings.GetValue<bool>("Building Markers", "Show Nearest Temple Building");
            bool showNearWeaponSmith = settings.GetValue<bool>("Building Markers", "Show Nearest Weapon Smith Building");



            ApplySettings(minimapPosition, minimapXOffset, minimapYOffset, minimapTransparency, depthSelected, refreshUpdateSelected, rotationMap, compassMap, keyInputString, distanceCoveredOutside, distanceCoveredInside, indoorMinimap,
              showNearAlchemist, showNearArmorer, showNearHouseForSale, showNearBank, showNearBookseller, showNearClothingStore,
              showNearGemStore, showNearGeneralStore, showNearGuildHall, showNearLibrary, showNearPalace, showNearPawnShop,
              showNearTavern, showNearTemple, showNearWeaponSmith);

        }
    }

    private void ApplySettings(int minimapPosition, int minimapXOffset, int minimapYOffset, float minimapTransparency, int depthSelected, int refreshUpdateSelected, bool rotationMap, bool compassMap, string keyInputString,
                            int distanceCoveredOutside, int distanceCoveredInside, bool indoorMinimap,
                           bool showNearAlchemist, bool showNearArmorer, bool showNearHouseForSale, bool showNearBank, bool showNearBookseller, bool showNearClothingStore,
                           bool showNearGemStore, bool showNearGeneralStore, bool showNearGuildHall, bool showNearLibrary, bool showNearPalace, bool showNearPawnShop,
                           bool showNearTavern, bool showNearTemple, bool showNearWeaponSmith)
    {
        //-------------------------------Variables References------------------------------------------------------

        
        allowRotationMap = rotationMap; //Load Rotation Map Config
        allowCompass = compassMap; //Load Compass Map Config
        allowIndoorMinimap = indoorMinimap;

        //---------------------Camera------------------------
        distanceCoveredInsideOption = distanceCoveredInside;
        distanceCoveredOutsideOption = distanceCoveredOutside;

        //---------------------Offset---------------------------
        xAdded = minimapXOffset;
        yAdded = minimapYOffset;
        //---------------------------------Transparency-----------------------------
        transparency = minimapTransparency;

        //----------------------------GUI DEPTH-------------------------------------
        depthModifier = depthSelected;

        //------------------------------Refresh Frequency-----------------------------
        if(refreshUpdateSelected == 0)
        {
            updateFrequency = 0.2f;
        }else if(refreshUpdateSelected == 1)
        {
            updateFrequency = 0.1f;
        }else if(refreshUpdateSelected == 2){
            updateFrequency = 0.06f;
        }
        else if (refreshUpdateSelected == 3)
        {
            updateFrequency = 0.04f;
        }
        else if (refreshUpdateSelected == 4)
        {
            updateFrequency = 0.03f;
        }

        AdjustUpdateInterval(updateFrequency);

        //----------------------------------------MARKER VISIBILITY------------------------------
        markerVisibility = new Dictionary<string, bool>()
            {
                { "Alchemist", showNearAlchemist },
                { "Armorer", showNearArmorer },
                { "HouseForSale", showNearHouseForSale },
                { "Bank", showNearBank },
                { "Bookseller", showNearBookseller },
                { "ClothingStore", showNearClothingStore },
                { "GemStore", showNearGemStore },
                { "GeneralStore", showNearGeneralStore },
                { "GuildHall", showNearGuildHall },
                { "Library", showNearLibrary },
                { "Palace", showNearPalace },
                { "PawnShop", showNearPawnShop },
                { "Tavern", showNearTavern },
                { "Temple", showNearTemple },
                { "WeaponSmith", showNearWeaponSmith }
            };

        //---------------------------------------TOGGLE KEY-------------------------------
        toggleKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), keyInputString);

        //--------------------------------------------MINIMAP POSITIONS--------------------------------------------------

        if (minimapPosition == 0) // BottomLeft
        {
            
            minimapRect.x = Screen.width * 0.01f + xAdded;
            minimapRect.y = Screen.height * 0.80f - textureHeightGUI +yAdded;
        }
        else if (minimapPosition == 1) // BottomCenter
        {

            minimapRect.x = (Screen.width * 0.5f) - (textureWidthGUI * 0.5f) + xAdded;
            minimapRect.y = Screen.height * 0.97f - textureHeightGUI + yAdded;
        }
        else if (minimapPosition == 2) // BottomRight
        {
            minimapRect.x = Screen.width - textureWidthGUI - 10 + xAdded;
            minimapRect.y = minimapRect.y + yAdded; 

            if (allowCompass== true) //Move to the compass position
            {
         
                minimapRect.x = Screen.width * 0.97f - textureWidthGUI + xAdded;
                minimapRect.y = Screen.height * 0.97f - textureHeightGUI + yAdded;
            }
            else
            {
                minimapRect.x = Screen.width * 0.97f - textureWidthGUI + xAdded;
                minimapRect.y = Screen.height * 0.90f - textureHeightGUI + yAdded;
            }
        }
        else if (minimapPosition == 3) // TopLeft
        {
            minimapRect.x = Screen.width * 0.01f + xAdded;
            minimapRect.y = Screen.height * 0.42f - textureHeightGUI + yAdded;
        }
        else if (minimapPosition == 4) // TopCenter
        {
            minimapRect.x = (Screen.width * 0.5f) - (textureWidthGUI * 0.5f) + xAdded;
            minimapRect.y = Screen.height * 0.03f + yAdded;
        }
        else if (minimapPosition == 5) // TopRight
        {
            if (allowCompass == true) //Move to the Compass Position
            {
                minimapRect.x = Screen.width * 0.97f - textureWidthGUI + xAdded;
                minimapRect.y = Screen.height * 0.03f + yAdded;
            }
            else
            {
                minimapRect.x = Screen.width * 0.97f -textureWidthGUI + xAdded;
                minimapRect.y = Screen.height * 0.10f + yAdded;
            }
        }

        markerRect = new Rect(minimapRect.x + minimapRect.width / 2 - 12.5f, minimapRect.y -10, 7, 7 ); //Define rect of all markers
        compassRect = new Rect(minimapRect.x + minimapRect.width / 2 - 12.5f, minimapRect.y - 10, 20, 20); //Define rect of compass

    }

    void AdjustUpdateInterval(float newInterval)
    {
        CancelInvoke("UpdateMinimap"); // Stop Actual Update
        InvokeRepeating("UpdateMinimap", 0f, newInterval); // New Update Rate
    }


    private static void LoadAssets() 
    {
        arrowTexture = mod.GetAsset<Texture2D>("PlayerArrowGUI");
        compassTexture = mod.GetAsset<Texture2D>("compassSpriteGUI");

        if (arrowTexture == null)
        {
            Debug.LogError("Failed to load PlayerArrow texture.");
        }
        if (compassTexture == null)
        {
            Debug.LogError("Failed to load compass texture.");
        }
    }

    public static void ReloadSettings()
    {
        var instance = FindObjectOfType<Minimap_Mod>();
        if (instance != null)
        {
            var settings = mod.GetSettings();
            var changes = new ModSettingsChange();
            instance.LoadSettings(settings, changes);

            var locatorMod = FindObjectOfType<MinimapLocator>();
            if (locatorMod != null)
            {
                locatorMod.LoadSettings(settings, changes);
            }
        }
    }

    public void setDistanceCovered(int distanceCoveredOutsideOption, int distanceCoveredInsideOption)
    {
        GameObject miniMapCamera = GameObject.Find("MiniMapCamera"); //Get camera GameObject

        if (miniMapCamera == null)
        {
            Debug.LogError("Not found 'MiniMapCamera'.");
            return;
        }

        Camera miniMapCam = miniMapCamera.GetComponent<Camera>(); //Get camera component

        if (miniMapCam == null)
        {
            Debug.LogError("GameObject 'MiniMapCamera' does not have 'Camera'.");
            return;
        }

        //-----------------------------GUI VARIABLES----------------------------------------------------------------------
        Vector2 arrowSize = new Vector2(25, 25);

        if (playerEnterExit != null && playerEnterExit.IsPlayerInside == false)
        {
            height = 50f;
            if (distanceCoveredOutsideOption == 0)
            {
                miniMapCam.orthographicSize = 25;
                arrowSize = new Vector2(20, 20);
                minDistanceToHideMarker = 45f;
                fadeDistance = minDistanceToHideMarker * 1.15f;
            }
            else if (distanceCoveredOutsideOption == 1)
            {
                miniMapCam.orthographicSize = 35;
                arrowSize = new Vector2(18, 18);
                minDistanceToHideMarker = 55f;
                fadeDistance = minDistanceToHideMarker * 1.15f;
            }
            else if (distanceCoveredOutsideOption == 2)
            {
                miniMapCam.orthographicSize = 50;
                arrowSize = new Vector2(15, 15);
                minDistanceToHideMarker = 70f;
                fadeDistance = minDistanceToHideMarker * 1.15f;
            }
            else if (distanceCoveredOutsideOption == 3)
            {
                miniMapCam.orthographicSize = 60;
                arrowSize = new Vector2(8, 8);
                minDistanceToHideMarker = 80f;
                fadeDistance = minDistanceToHideMarker * 1.15f;
            }
            else if (distanceCoveredOutsideOption == 4)
            {
                miniMapCam.orthographicSize = 80;
                arrowSize = new Vector2(5, 5);
                minDistanceToHideMarker = 100f;
                fadeDistance = minDistanceToHideMarker * 1.15f;
            }
        }
        else if (playerEnterExit != null && playerEnterExit.IsPlayerInside == true)
        {
            height = interiorHeight;
            if (distanceCoveredInsideOption == 0)
            {
                miniMapCam.orthographicSize = 20;
                arrowSize = new Vector2(10, 10);
            }
            else if (distanceCoveredInsideOption == 1)
            {
                miniMapCam.orthographicSize = 25;
                arrowSize = new Vector2(8, 8);
            }
            else if (distanceCoveredInsideOption == 2)
            {
                miniMapCam.orthographicSize = 30;
                arrowSize = new Vector2(6, 6);
            }
            else if (distanceCoveredInsideOption == 3)
            {
                miniMapCam.orthographicSize = 35;
                arrowSize = new Vector2(4, 4);
            }
            else if (distanceCoveredInsideOption == 4)
            {
                miniMapCam.orthographicSize = 40;
                arrowSize = new Vector2(2, 2);
            }
        }
        else
        {
            Debug.LogError("playerEnterExit not initializated");
        }

        //Correct arrowSize
        if (minimapRect != null)
        {
            arrowRect = new Rect(minimapRect.x + minimapRect.width / 2 - arrowSize.x / 2, minimapRect.y + minimapRect.height / 2 - arrowSize.y / 2, arrowSize.x, arrowSize.y);
        }
        else
        {
            Debug.LogError("minimapRect not initializated.");
        }
    }


    //----------------------------------------MARKERS---------------------------------------------------------

    //To make rotationMarkers as compass
    public void rotationMarker(Rect rect, Texture2D markerTexture, float markerRotation, Matrix4x4 matrixBackup)
    {
        GUIUtility.RotateAroundPivot(markerRotation, minimapRect.center); //Rotate around a virtual pivot, on this case MinimapRect.center
        GUI.DrawTexture(rect, markerTexture, ScaleMode.StretchToFill); //Draw texture with rect, a texture and the scalemode
        GUI.matrix = matrixBackup; //Get de matrix backup
    }

    public void FindNearMarker()
    {
        if (player == null) return;
        closestMarkerTextures.Clear(); //Clean the texture
        closestMarkerPositions.Clear(); //Clean the position
                                        
        GameObject markersContainer = GameObject.Find("MinimapMarkersContainer"); //Search container
        if (markersContainer == null) return;
        foreach (var markerType in markerVisibility.Keys) //For each on dictionary read visibility
        {
            if (!markerVisibility[markerType])
                continue;

            Transform closestMarker = null;
            float closestDistanceSqr = Mathf.Infinity; //A infinite float distance
            Vector3 playerPosition = player.position; 

            foreach (Transform marker in markersContainer.transform) //when find all gameObjects inside
            {
                if (marker.name.Contains(markerType)) 
                {
                    Vector3 directionToMarker = marker.position - playerPosition; //If is right, check his diference with player
                    float dSqrToMarker = directionToMarker.sqrMagnitude; //magnitude of distance, the size of the vector

                    if (dSqrToMarker < closestDistanceSqr) //Check if distance is lower than the lower distance registered
                    {
                        closestDistanceSqr = dSqrToMarker; //The initial distance registered is the lower
                        closestMarker = marker; //When check all, the lower distance registered on all markers is the closest to player
                    }
                }
            }

            if (closestMarker != null)
            {
              //  Debug.Log($"{markerType} found at: {closestMarker.transform.position}");

                Renderer markerRenderer = closestMarker.GetComponentInChildren<Renderer>();
                if (markerRenderer != null && markerRenderer.material != null)
                {
                    Texture2D markerTexture = markerRenderer.material.mainTexture as Texture2D;
                    if (markerTexture != null)
                    {
                        closestMarkerTextures[markerType] = markerTexture;
                  //      Debug.Log($"Texture of {markerType} marker: {markerTexture.name}");
                    }
                    else
                    {
                       Debug.Log($"No Texture2D found on the marker's material.");
                    }
                }
                else
                {
                    Debug.Log($"No Renderer or Material found on the marker.");
                }

                closestMarkerPositions[markerType] = closestMarker.transform.position;
            }
            else
            {
               // Debug.Log($"No markers of type {markerType} found.");
            }
        }
    }

    void Start()
    {
        LoadAssets(); //LOAD ALL ASSETS
        //-----------------------REFERENCES---------------------------
        player = GameObject.FindGameObjectWithTag("Player").transform; //Define Player
        playerEnterExit = player.GetComponent<PlayerEnterExit>(); //Define playerEnterExit Component

        //----------------CAMERA-----------------------------------------
        GameObject cameraGameObject = new GameObject("MiniMapCamera");
        miniMapCamera = cameraGameObject.AddComponent<Camera>();

        miniMapCamera.orthographic = true;
        miniMapCamera.orthographicSize = 50; // Default ortographicSize
        miniMapCamera.cullingMask = -1; // Everything to detect the skylayer mask (locators)
        
        miniMapCamera.clearFlags = CameraClearFlags.SolidColor; //Set Solid Background
        miniMapCamera.backgroundColor = Color.black;//Set Black Background

        
        RenderTexture renderTexture = new RenderTexture(textureWidthGUI, textureHeightGUI, 16); //Create Render Texture
        renderTexture.Create();
        miniMapCamera.targetTexture = renderTexture; //Assign to CameraTarget

        mainCamera = Camera.main;
        previousRenderTexture = mainCamera.targetTexture;

        //---------------------------GUI START-------------------------------------
        minimapRect = new Rect(10, 10, textureHeightGUI, textureWidthGUI);

        // Apply Settings
        ReloadSettings();

        StartCoroutine(CheckInput());
        AdjustUpdateInterval(updateFrequency);
    }

    IEnumerator CheckInput()
    {
        while (true)
        {
            if (!GameManager.IsGamePaused && !SaveLoadManager.Instance.LoadInProgress)
            {
                if (InputManager.Instance.GetKeyDown(toggleKey, true))
                {
                    renderMapToggle = !renderMapToggle; // Toggle renderMap
                }
            }
            yield return null;
        }
    }


    void UpdateMinimap()
    {
        if (player == null || playerEnterExit == null || miniMapCamera == null)
            return;
        Vector3 newPosition = player.position;
        newPosition.y += height;
        miniMapCamera.transform.position = newPosition;
        DaggerfallHUD daggerfallHUD = DaggerfallUI.Instance.DaggerfallHUD; //To modify some HUD

        float playerYRotation = player.eulerAngles.y;

        if (allowRotationMap == true)
        {
            miniMapCamera.transform.rotation = Quaternion.Euler(90, player.eulerAngles.y, 0);
            compassRotation = -playerYRotation;
        }
        else
        {
            miniMapCamera.transform.rotation = Quaternion.Euler(90, 0, 0);
            compassRotation = 0;
        }

        if (playerEnterExit.IsPlayerInside == true)
        {
            if (allowIndoorMinimap == true)
            {
                miniMapCamera.enabled = true;
                renderMap = true;
                setDistanceCovered(distanceCoveredOutsideOption, distanceCoveredInsideOption);
                miniMapCamera.farClipPlane = farClipPlane;
            }
            else
            {
                miniMapCamera.enabled = false;
                renderMap = false;
            }
        }
        else
        {
            miniMapCamera.enabled = true;
            renderMap = true;
            setDistanceCovered(distanceCoveredOutsideOption, distanceCoveredInsideOption);
            miniMapCamera.farClipPlane = 1000;
        }

        if (allowCompass == true)
        {
            if (playerEnterExit.IsPlayerInside == true)
                {
                if (allowIndoorMinimap == false)
                {
                    daggerfallHUD.ShowCompass = true;
                }
                else
                {
                    daggerfallHUD.ShowCompass = false;
                }
            }
            else
            {
                daggerfallHUD.ShowCompass = false;
            }
                
        }
        else
        {
            daggerfallHUD.ShowCompass = true;
        }

        //--------------------------------------Marker---------------------------------------------------
        FindNearMarker(); //Call all nearer markers

        closestMarkerRotations.Clear(); //Clear marker rotation to continue refreshing it
        foreach (var markerType in closestMarkerPositions.Keys) //For each markerType registered on the function
        {
            Vector3 directionToMarker = closestMarkerPositions[markerType] - player.position; //check his position diference
            float angleToMarker = Mathf.Atan2(directionToMarker.x, directionToMarker.z) * Mathf.Rad2Deg; //check his angle
            float markerRotation = angleToMarker - playerYRotation; //rest difference
            closestMarkerRotations[markerType] = markerRotation; //define marker rotation of each marker type
        }
    }

    void Update()
    {
        if (mainCamera.targetTexture != previousRenderTexture)
        {
            previousRenderTexture = mainCamera.targetTexture;

            GameObject minimapCamera = GameObject.Find("MiniMapCamera");

            if (minimapCamera != null)
            {
                minimapCamera.SetActive(false);

                // Reactivar después de un corto período de tiempo
                StartCoroutine(ReactivarMinimapCamera(minimapCamera, 0.1f)); // 1.0f es el tiempo en segundos
            }
            else
            {
                Debug.LogWarning("Not found GameObject 'minimapCamera'.");
            }
        }
    } //To check if the mode on effect has changed

    IEnumerator ReactivarMinimapCamera(GameObject minimapCamera, float delay)
    {
        yield return new WaitForSeconds(delay);
        minimapCamera.SetActive(true);
    } //To check if the mode on effect has changed



    void OnGUI()
    {
        if (miniMapCamera == null ||
       miniMapCamera.targetTexture == null ||
       arrowTexture == null ||
       compassTexture == null)
            return;
        GUI.depth = 0 + depthModifier; //To initial Markers

        Matrix4x4 matrixBackup = GUI.matrix;
        Color originalColor = GUI.color;
        GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, transparency);

        if (renderMapToggle == true)
        {
            if (renderMap == true)
            {
                GUI.DrawTexture(minimapRect, miniMapCamera.targetTexture, ScaleMode.StretchToFill);

                // Draw Arrow
                if (allowRotationMap == false)
                {
                    GUIUtility.RotateAroundPivot(player.eulerAngles.y, minimapRect.center); //Not rotate map
                }
                GUI.DrawTexture(arrowRect, arrowTexture, ScaleMode.StretchToFill); //Rotate arrow

                // Restor matrix
                GUI.matrix = matrixBackup;

                // Draw Markers
                foreach (var markerType in closestMarkerTextures.Keys) //For each marker registered by update
                {
                    if (markerVisibility[markerType]) //if is valid
                    {
                        Vector3 markerPosition = closestMarkerPositions[markerType]; //define position
                        float distanceToPlayer = Vector3.Distance(player.position, markerPosition); //diference to player position

                        if (distanceToPlayer > minDistanceToHideMarker) //if is to close, do not render
                        {
                            // Calculate to vanish gradually
                            float alpha = 1.0f;
                            if (distanceToPlayer < fadeDistance) //Distance to fade
                            {
                                float t = (distanceToPlayer - minDistanceToHideMarker) / (fadeDistance - minDistanceToHideMarker); //an equation to calculate gradually the vanish when the player is closer
                                alpha = Mathf.Pow(t, 3); // Cubic interpolation
                            }


                            GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, alpha);// Move the alpha with the formula

                            if (allowRotationMap == true)
                            {
                                rotationMarker(markerRect, closestMarkerTextures[markerType], closestMarkerRotations[markerType], matrixBackup); //Move markers
                            }
                            else
                            {

                                Vector3 directionToMarker = markerPosition - player.position; // Check direction of each marker
                                float angleToMarker = Mathf.Atan2(directionToMarker.x, directionToMarker.z) * Mathf.Rad2Deg;// Check angle of each marker

                                // Rotate the markers
                                GUIUtility.RotateAroundPivot(angleToMarker, minimapRect.center);
                                GUI.DrawTexture(markerRect, closestMarkerTextures[markerType], ScaleMode.StretchToFill);

                                // Restore Matrix
                                GUI.matrix = matrixBackup;
                            }
                        }
                    }
                }

                GUI.depth = -1 + depthModifier; //Draw compass over

                GUI.color = originalColor; //Restore color

                if (allowCompass)
                {
                    rotationMarker(compassRect, compassTexture, compassRotation, matrixBackup); //Draw compass
                }

                //To draw map under
                GUI.depth = 0 + depthModifier;
                // Restore matrix
                GUI.matrix = matrixBackup;
            }
        }
    }


}
