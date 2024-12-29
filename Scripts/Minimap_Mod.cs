using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;
using DaggerfallWorkshop.Game.UserInterfaceWindows;

public class Minimap_Mod : MonoBehaviour
{
    private static Mod mod; //Define mod variable

    //----------------------------------Minimap Variables------------------------------------
  
    public static float minimapSizePercent = 0.23f;
    public static int shortestSide = Mathf.Min(Screen.width, Screen.height);
    public static int textureWidthGUI = (int)(shortestSide * minimapSizePercent); // Render Texture Width
    public static int textureHeightGUI = textureWidthGUI; // Render Texture Height
    public float height = 50f; // Camera Height
    public static bool allowRotationMap; //Allow Rotation
    public static bool allowCompass; //Allow Compass
    public static int actualMinimapShape; //Minimap Shape

    //---------------------------------GUI VARIABLES---------------------------------------
    private Rect minimapRect;
    private Rect arrowRect;
    private Rect compassRect;

    private static Texture2D arrowTexture;
    private static Texture2D compassTexture;
    private float compassRotation;


    //----------------------------------References----------------------------------------------
    private Transform player; // Player reference
    public Camera miniMapCamera; // Minimap Camera
    private PlayerEnterExit playerEnterExit; // PlayerEnterExit component
    private bool playerInside;

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
        };
        mod.LoadSettings(); //Load Settings

        var go = new GameObject(mod.Title);
        go.AddComponent<Minimap_Mod>();//Add component to the GameObject
    }

    private void LoadSettings(ModSettings settings, ModSettingsChange change)
    {
        //-------------------- Check if Settings has changed---------------------------
        if (change.HasChanged("UI Settings") || change.HasChanged("Map View"))
        {
            int minimapPosition = settings.GetValue<int>("UI Settings", "Position");
            bool rotationMap = settings.GetValue<bool>("UI Settings", "Rotation");
            bool compassMap = settings.GetValue<bool>("UI Settings", "Integrated Compass");
            int distanceCovered = settings.GetValue<int>("Map View", "Distance Covered");
            ApplySettings(minimapPosition, rotationMap, compassMap, distanceCovered);
        }
    }

    private void ApplySettings(int minimapPosition, bool rotationMap, bool compassMap, int distanceCovered)
    {
        //-------------------------------Variables References------------------------------------------------------
        
        DaggerfallHUD daggerfallHUD = DaggerfallUI.Instance.DaggerfallHUD; //To modify some HUD
        allowRotationMap = rotationMap; //Load Rotation Map Config
        allowCompass = compassMap; //Load Compass Map Config
        

        //-----------------------------GUI VARIABLES----------------------------------------------------------------------
        Vector2 arrowSize = new Vector2(25, 25);

        //--------------------------------------------MINIMAP POSITIONS--------------------------------------------------
        
        //-------------Variables-----------------------------
        
        if (minimapPosition == 0) // BottomLeft
        {
            
            minimapRect.x = Screen.width * 0.01f;
            minimapRect.y = Screen.height * 0.80f - textureHeightGUI;
        }
        else if (minimapPosition == 1) // BottomCenter
        {

            minimapRect.x = (Screen.width * 0.5f) - (textureWidthGUI * 0.5f);
            minimapRect.y = Screen.height * 0.97f - textureHeightGUI;
        }
        else if (minimapPosition == 2) // BottomRight
        {
            minimapRect.x = Screen.width - textureWidthGUI - 10;
            

            if (allowCompass== true) //Move to the compass position
            {
         
                minimapRect.x = Screen.width * 0.97f - textureWidthGUI;
                minimapRect.y = Screen.height * 0.97f - textureHeightGUI;
            }
            else
            {
                minimapRect.x = Screen.width * 0.97f - textureWidthGUI;
                minimapRect.y = Screen.height * 0.90f - textureHeightGUI;
            }
        }
        else if (minimapPosition == 3) // TopLeft
        {
            minimapRect.x = Screen.width * 0.01f;
            minimapRect.y = Screen.height * 0.42f - textureHeightGUI;
        }
        else if (minimapPosition == 4) // TopCenter
        {
            minimapRect.x = (Screen.width * 0.5f) - (textureWidthGUI * 0.5f);
            minimapRect.y = Screen.height * 0.03f ;
        }
        else if (minimapPosition == 5) // TopRight
        {
            if (allowCompass == true) //Move to the Compass Position
            {
                minimapRect.x = Screen.width * 0.97f - textureWidthGUI;
                minimapRect.y = Screen.height * 0.03f;
            }
            else
            {
                minimapRect.x = Screen.width * 0.97f -textureWidthGUI;
                minimapRect.y = Screen.height * 0.10f;
            }
        }

        compassRect = new Rect(minimapRect.x + minimapRect.width / 2 - 12.5f, minimapRect.y -10, 20, 20 );
        
        if (allowCompass == true)
        {
            
            daggerfallHUD.ShowCompass = false;
        }
        else
        {
            daggerfallHUD.ShowCompass = true;
        }

        //----------------------------CAMERA--------------------------------
        GameObject miniMapCamera = GameObject.Find("MiniMapCamera"); //Get camera GameObject
        Camera miniMapCam = miniMapCamera.GetComponent<Camera>(); //Get camera component
        if (distanceCovered == 0)
        {
            miniMapCam.orthographicSize = 35;
            arrowSize = new Vector2(18, 18);

        }

        else if (distanceCovered == 1)
        {
            miniMapCam.orthographicSize = 50;
            arrowSize = new Vector2(15, 15);
        }

        else if (distanceCovered == 2)
        {
            miniMapCam.orthographicSize = 60;
            arrowSize = new Vector2(10, 10);
        }
        arrowRect = new Rect(minimapRect.x + minimapRect.width / 2 - arrowSize.x / 2, minimapRect.y + minimapRect.height / 2 - arrowSize.y / 2, arrowSize.x, arrowSize.y);

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
        miniMapCamera.cullingMask = LayerMask.GetMask("Default"); // SetMask
        
        miniMapCamera.clearFlags = CameraClearFlags.SolidColor; //Set Solid Background
        miniMapCamera.backgroundColor = Color.black;//Set Black Background

        
        RenderTexture renderTexture = new RenderTexture(textureWidthGUI, textureHeightGUI, 16); //Create Render Texture
        renderTexture.Create();
        miniMapCamera.targetTexture = renderTexture; //Assign to CameraTarget

        //---------------------------GUI START-------------------------------------
        minimapRect = new Rect(10, 10, textureHeightGUI, textureWidthGUI);
        arrowRect = new Rect(minimapRect.x + minimapRect.width / 2 - 12.5f, minimapRect.y + minimapRect.height / 2 - 12.5f, 25, 25); //Half Height and Width Minimap To Center
        compassRect = new Rect(minimapRect.x + minimapRect.width / 2 - 12.5f, minimapRect.y -10, 15, 15 ); //Center compass on top

        // Apply Settings
        ReloadSettings();
    }

    void LateUpdate()
    {
        //----------------------------Player Position----------------------------------------
        Vector3 newPosition = player.position; //Get position
        newPosition.y += height; //Get only Y
        miniMapCamera.transform.position = newPosition; //Set Y position to camera

        //------------------------------Rotation----------------------------------------------------------
        float playerYRotation = player.eulerAngles.y; //Get player Rotation

        if (allowRotationMap == true)
        {
            miniMapCamera.transform.rotation = Quaternion.Euler(90, player.eulerAngles.y, 0); //Set Camera Rotation to Player Rotation

            //-----------------ON GUI--------------------------
            compassRotation = -playerYRotation;
            
        }

        else{
            miniMapCamera.transform.rotation = Quaternion.Euler(90, 0, 0); //Set Map to Initial Position

            //------------------ON GUI-----------------------------
            compassRotation = 0;
        }

        //------------------------------Limitation Area---------------------------------------------

        if (playerEnterExit && playerEnterExit.IsPlayerInside == false)
        {
            miniMapCamera.enabled = true;
            playerInside = false;

        }
        else
        {
            miniMapCamera.enabled = false;
            playerInside = true;
        }
    }

    void OnGUI()
    {
        GUI.depth = 0;

        // Guardar la matriz original
        Matrix4x4 matrixBackup = GUI.matrix;

        if (playerInside == false) { 
                      
           GUI.DrawTexture(minimapRect, miniMapCamera.targetTexture, ScaleMode.StretchToFill);


            if (allowCompass)
            {
                GUIUtility.RotateAroundPivot(compassRotation, minimapRect.center);
                GUI.DrawTexture(compassRect, compassTexture, ScaleMode.StretchToFill);
                GUI.matrix = matrixBackup;
            }

            if (allowRotationMap == false)
            {
                GUIUtility.RotateAroundPivot(player.eulerAngles.y, minimapRect.center);
            }
            GUI.DrawTexture(arrowRect, arrowTexture, ScaleMode.StretchToFill);
            GUI.matrix = matrixBackup;
        }
    }


}
