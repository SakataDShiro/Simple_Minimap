using UnityEngine;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using System.Collections.Generic;
using System.Collections;

namespace MinimapLocatorMod
{
    public class MinimapLocator : MonoBehaviour
    {
        private static Mod mod;

        //----------------------------------UPDATE---------------------------------
        public float updateFrequency = 0.1f;

        //-------------------------BUILDING VARIABLES---------------------------------------
        public DaggerfallRMBBlock[] blockArray; //Obtain city blocks
        public BuildingDirectory buildingDirectory;//Info buildings of the city
        public List<Vector3> buildingLocations = new List<Vector3>();//Array of static buildings in block

        //------------------------------MARKER VARIABLES---------------------------------------
        public int textureSize = 128; // Base Size of marker
        public int markerScale; //Variable to move the size of the marker
        public float markerBorder; //Variable to move the border size

        private List<GameObject> createdMarkers = new List<GameObject>(); // References of created markers


        //---------------------------BUILDING COLOR VARIABLES---------------------------------
        public Color32 alchemistColor;
        public Color32 armorerColor;
        public Color32 houseForSaleColor;
        public Color32 bankColor;
        public Color32 booksellerColor;
        public Color32 clothingStoreColor;
        public Color32 gemStoreColor;
        public Color32 generalStoreColor;
        public Color32 guildHallColor;
        public Color32 libraryColor;
        public Color32 palaceColor;
        public Color32 pawnShopColor;
        public Color32 tavernColor;
        public Color32 templeColor;
        public Color32 weaponSmithColor;

        //---------------------------BUILDING BOOL VARIABLES---------------------------------
        private bool allowAlchemist;
        private bool allowArmorer;
        private bool allowHouseForSale;
        private bool allowBank;
        private bool allowBookseller;
        private bool allowClothingStore;
        private bool allowGemStore;
        private bool allowGeneralStore;
        private bool allowGuildHall;
        private bool allowLibrary;
        private bool allowPalace;
        private bool allowPawnShop;
        private bool allowTavern;
        private bool allowTemple;
        private bool allowWeaponSmith;


        //-----------------------------Player control variables--------------------------------------
        private bool wasInsideBuilding = false; //Check if player is inside
        private bool wasResting = false; // check if the player is resting
        private bool isInLocationRect = false; //check if is in a locationRect 
        private bool needsUpdateAfterTransition = false; //if is in a new location rect, but does not have detected markers
        private DaggerfallLocation lastPlayerLocation = null;
 
     public void LoadSettings(ModSettings settings, ModSettingsChange change)
        {
            //-------------------- Check if Settings has changed---------------------------
            if (change.HasChanged("Markers") || change.HasChanged("Building Markers"))
            {
                int markerSizeSetting = settings.GetValue<int>("Markers", "Marker Size");
                float markerBorderSetting = settings.GetValue<float>("Markers", "Border Size");
                

                bool alchemist = settings.GetValue<bool>("Building Markers", "Alchemist");
                bool armorer = settings.GetValue<bool>("Building Markers", "Armorer");
                bool houseForSale = settings.GetValue<bool>("Building Markers", "House For Sale");
                bool bank = settings.GetValue<bool>("Building Markers", "Bank");
                bool bookseller = settings.GetValue<bool>("Building Markers", "Bookseller");
                bool clothingStore = settings.GetValue<bool>("Building Markers", "Clothing Store");
                bool gemStore = settings.GetValue<bool>("Building Markers", "Gem Store");
                bool generalStore = settings.GetValue<bool>("Building Markers", "General Store");
                bool guildHall = settings.GetValue<bool>("Building Markers", "Guild Hall");
                bool library = settings.GetValue<bool>("Building Markers", "Library");
                bool palace = settings.GetValue<bool>("Building Markers", "Palace");
                bool pawnShop = settings.GetValue<bool>("Building Markers", "Pawn Shop");
                bool tavern = settings.GetValue<bool>("Building Markers", "Tavern");
                bool temple = settings.GetValue<bool>("Building Markers", "Temple");
                bool weaponSmith = settings.GetValue<bool>("Building Markers", "Weapon Smith");



                Color32 alchemistSelectedColor = settings.GetValue<Color32>("Building Markers", "Alchemist Marker Color");
                Color32 armorerSelectedColor = settings.GetValue<Color32>("Building Markers", "Armorer Marker Color");
                Color32 houseForSaleSelectedColor = settings.GetValue<Color32>("Building Markers", "House For Sale Marker Color");
                Color32 bankSelectedColor = settings.GetValue<Color32>("Building Markers", "Bank Marker Color");
                Color32 booksellerSelectedColor = settings.GetValue<Color32>("Building Markers", "Bookseller Marker Color");
                Color32 clothingStoreSelectedColor = settings.GetValue<Color32>("Building Markers", "Clothing Store Marker Color");
                Color32 gemStoreSelectedColor = settings.GetValue<Color32>("Building Markers", "Gem Store Marker Color");
                Color32 generalStoreSelectedColor = settings.GetValue<Color32>("Building Markers", "General Store Marker Color");
                Color32 guildHallSelectedColor = settings.GetValue<Color32>("Building Markers", "Guild Hall Marker Color");
                Color32 librarySelectedColor = settings.GetValue<Color32>("Building Markers", "Library Marker Color");
                Color32 palaceSelectedColor = settings.GetValue<Color32>("Building Markers", "Palace Marker Color");
                Color32 pawnShopSelectedColor = settings.GetValue<Color32>("Building Markers", "Pawn Shop Marker Color");
                Color32 tavernSelectedColor = settings.GetValue<Color32>("Building Markers", "Tavern Marker Color");
                Color32 templeSelectedColor = settings.GetValue<Color32>("Building Markers", "Temple Marker Color");
                Color32 weaponSmithSelectedColor = settings.GetValue<Color32>("Building Markers", "Weapon Smith Marker Color");



                ApplySettings(
                    markerSizeSetting,
                    markerBorderSetting,
                    alchemist, alchemistSelectedColor,
                    armorer, armorerSelectedColor,
                    houseForSale, houseForSaleSelectedColor,
                    bank, bankSelectedColor,
                    bookseller, booksellerSelectedColor,
                    clothingStore, clothingStoreSelectedColor,
                    gemStore, gemStoreSelectedColor,
                    generalStore, generalStoreSelectedColor,
                    guildHall, guildHallSelectedColor,
                    library, librarySelectedColor,
                    palace, palaceSelectedColor,
                    pawnShop, pawnShopSelectedColor,
                    tavern, tavernSelectedColor,
                    temple, templeSelectedColor,
                    weaponSmith, weaponSmithSelectedColor
                );
            }
        }

            private void ApplySettings(
            int markerSizeSetting,
            float markerBorderSetting,
            bool alchemist, Color32 alchemistSelectedColor,
            bool armorer, Color32 armorerSelectedColor,
            bool houseForSale, Color32 houseForSaleSelectedColor,
            bool bank, Color32 bankSelectedColor,
            bool bookseller, Color32 booksellerSelectedColor,
            bool clothingStore, Color32 clothingStoreSelectedColor,
            bool gemStore, Color32 gemStoreSelectedColor,
            bool generalStore, Color32 generalStoreSelectedColor,
            bool guildHall, Color32 guildHallSelectedColor,
            bool library, Color32 librarySelectedColor,
            bool palace, Color32 palaceSelectedColor,
            bool pawnShop, Color32 pawnShopSelectedColor,
            bool tavern, Color32 tavernSelectedColor,
            bool temple, Color32 templeSelectedColor,
            bool weaponSmith, Color32 weaponSmithSelectedColor
        )
        
        {
            markerBorder = markerBorderSetting;
            markerScale = 9 + markerSizeSetting;

            allowAlchemist = alchemist;
            allowArmorer = armorer;
            allowHouseForSale = houseForSale;
            allowBank = bank;
            allowBookseller = bookseller;
            allowClothingStore = clothingStore;
            allowGemStore = gemStore;
            allowGeneralStore = generalStore;
            allowGuildHall = guildHall;
            allowLibrary = library;
            allowPalace = palace;
            allowPawnShop = pawnShop;
            allowTavern = tavern;
            allowTemple = temple;
            allowWeaponSmith = weaponSmith;

            alchemistColor = alchemistSelectedColor;
            armorerColor = armorerSelectedColor;
            houseForSaleColor = houseForSaleSelectedColor;
            bankColor = bankSelectedColor;
            booksellerColor = booksellerSelectedColor;
            clothingStoreColor = clothingStoreSelectedColor;
            gemStoreColor = gemStoreSelectedColor;
            generalStoreColor = generalStoreSelectedColor;
            guildHallColor = guildHallSelectedColor;
            libraryColor = librarySelectedColor;
            palaceColor = palaceSelectedColor;
            pawnShopColor = pawnShopSelectedColor;
            tavernColor = tavernSelectedColor;
            templeColor = templeSelectedColor;
            weaponSmithColor = weaponSmithSelectedColor;

            UpdateMarkers();
    }

        private void Start()
        {
            AdjustUpdateInterval(updateFrequency);
            UpdateMarkers(); //Update the markers
        }

        void UpdateMarkers()
        {
            ClearMarkers();            // Eliminate the previous markers
            ReinitializeCityData();    // Recollect the data

            if (blockArray == null)
            {
                Debug.LogError("blockArray is still null after reinitializing city data.");
                return;
            }
            if (buildingDirectory == null)
            {
                Debug.LogError("buildingDirectory is still null after reinitializing city data.");
                return;
            }

            // Locate and render the buildings allowed
            if (allowAlchemist)
            {
                LocateBuildings(DFLocation.BuildingTypes.Alchemist, alchemistColor);
            }
            if (allowArmorer)
            {
                LocateBuildings(DFLocation.BuildingTypes.Armorer, armorerColor);
            }
            if (allowHouseForSale)
            {
                LocateBuildings(DFLocation.BuildingTypes.HouseForSale, houseForSaleColor);
            }
            if (allowBank)
            {
                LocateBuildings(DFLocation.BuildingTypes.Bank, bankColor);
            }
            if (allowBookseller)
            {
                LocateBuildings(DFLocation.BuildingTypes.Bookseller, booksellerColor);
            }
            if (allowClothingStore)
            {
                LocateBuildings(DFLocation.BuildingTypes.ClothingStore, clothingStoreColor);
            }
            if (allowGemStore)
            {
                LocateBuildings(DFLocation.BuildingTypes.GemStore, gemStoreColor);
            }
            if (allowGeneralStore)
            {
                LocateBuildings(DFLocation.BuildingTypes.GeneralStore, generalStoreColor);
            }
            if (allowGuildHall)
            {
                LocateBuildings(DFLocation.BuildingTypes.GuildHall, guildHallColor);
            }
            if (allowLibrary)
            {
                LocateBuildings(DFLocation.BuildingTypes.Library, libraryColor);
            }
            if (allowPalace)
            {
                LocateBuildings(DFLocation.BuildingTypes.Palace, palaceColor);
            }
            if (allowPawnShop)
            {
                LocateBuildings(DFLocation.BuildingTypes.PawnShop, pawnShopColor);
            }
            if (allowTavern)
            {
                LocateBuildings(DFLocation.BuildingTypes.Tavern, tavernColor);
            }
            if (allowTemple)
            {
                LocateBuildings(DFLocation.BuildingTypes.Temple, templeColor);
            }
            if (allowWeaponSmith)
            {
                LocateBuildings(DFLocation.BuildingTypes.WeaponSmith, weaponSmithColor);
            }
        }


        void ReinitializeCityData()
        {
            buildingLocations.Clear(); // Clear previous data
            if (GameManager.Instance.StreamingWorld.CurrentPlayerLocationObject != null) //check the current location
            {
                blockArray = GameManager.Instance.StreamingWorld.CurrentPlayerLocationObject.GetComponentsInChildren<DaggerfallRMBBlock>(); //New block
                buildingDirectory = GameManager.Instance.StreamingWorld.CurrentPlayerLocationObject.GetComponentInChildren<BuildingDirectory>(); //new buildings
                Debug.Log("DATA LOADED");
            }
            else
            {
                blockArray = null;
                buildingDirectory = null;
                Debug.Log("NO DATA");
            }
        }


        void LocateBuildings(DFLocation.BuildingTypes buildingType, Color buildingColor)
        {
            if (blockArray != null && buildingDirectory != null) //if is valid the info
            {
                foreach (DaggerfallRMBBlock block in blockArray) //for each block
                {
                    if (block == null)
                    {
                        Debug.LogError("Block is null in blockArray");
                        continue;
                    }

                    var staticBuildingsComponent = block.GetComponentInChildren<DaggerfallStaticBuildings>();
                    if (staticBuildingsComponent == null)
                    {
                        Debug.LogWarning("DaggerfallStaticBuildings component is null in block. Skipping this block.");
                        continue;
                    }

                    StaticBuilding[] staticBuildings = staticBuildingsComponent.Buildings;
                    if (staticBuildings == null)
                    {
                        Debug.Log("Static Buildings Is Null");
                    }
                    else
                    {
                        LocateStaticBuildings(staticBuildings, block, buildingType, buildingColor);
                    }
                }
                RenderBuildingMarkers(buildingType, buildingColor);
                buildingLocations.Clear(); // Clear previous data
            }
            else
            {
                if (blockArray == null)
                {
                    Debug.Log("Block Array is Null");
                }

                if (buildingDirectory == null)
                {
                    Debug.Log("Building Directory is Null");
                }
            }
        }



        void LocateStaticBuildings(StaticBuilding[] staticBuildings, DaggerfallRMBBlock block, DFLocation.BuildingTypes buildingType, Color buildingColor)
        {
            foreach (StaticBuilding building in staticBuildings) //check the buildings
            {
                BuildingSummary buildingSummary;
                buildingDirectory.GetBuildingSummary(building.buildingKey, out buildingSummary);

                if (buildingSummary.BuildingType == buildingType) //filter per building type
                {
                    Vector3 buildingPosition = block.transform.position + buildingSummary.Position;
                    buildingLocations.Add(buildingPosition); //add to the list of buildings checked
                    Debug.Log(buildingType + " found at: " + buildingPosition);
                }
            }
        }

        void RenderBuildingMarkers(DFLocation.BuildingTypes buildingType, Color buildingColor)
        {
            Texture2D circleTexture = CreateCircleTexture(textureSize, buildingColor, Color.clear); //create the texture

            foreach (Vector3 position in buildingLocations) //for each building finded
            {
                float markerHeight = position.y + 40;

                GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Quad); //create a marker
                marker.transform.position = new Vector3(position.x, markerHeight, position.z); // Locate the marker over the building
                marker.transform.localScale = new Vector3(markerScale, markerScale, markerScale); // Adjust the size of the marker
                marker.transform.rotation = Quaternion.Euler(90, 0, 0); // The marker always facing up
                marker.GetComponent<Renderer>().material.mainTexture = circleTexture;
                marker.GetComponent<Renderer>().material.shader = Shader.Find("Unlit/Transparent"); //put shader

                marker.name = buildingType.ToString() + "_Marker";// Name the marker
                createdMarkers.Add(marker); //add marker to the list of marker created
            }
        }

        void ClearMarkers()
        {
            foreach (GameObject marker in createdMarkers) //Eliminate marker created
            {
                Destroy(marker);
            }
            createdMarkers.Clear(); // Clean list of markers
        }

        Texture2D CreateCircleTexture(int size, Color circleColor, Color backgroundColor)
        {
            Texture2D texture = new Texture2D(size, size);
            float radius = size / 2f;
            float squaredRadius = radius * radius;
            float squaredInnerRadius = (radius - markerBorder) * (radius - markerBorder);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - radius;
                    float dy = y - radius;
                    float distanceSquared = dx * dx + dy * dy;

                    if (distanceSquared <= squaredRadius)
                    {
                        if (distanceSquared >= squaredInnerRadius)
                        {
                            texture.SetPixel(x, y, Color.black); // Color del borde
                        }
                        else
                        {
                            texture.SetPixel(x, y, circleColor); // Color del círculo
                        }
                    }
                    else
                    {
                        texture.SetPixel(x, y, backgroundColor); // Color del fondo
                    }
                }
            }

            texture.Apply();
            return texture;
        }

        void AdjustUpdateInterval(float newInterval)
        {
            CancelInvoke("UpdateLocator"); // Stop Actual Update
            InvokeRepeating("UpdateLocator", 0f, newInterval); // New Update Rate
        }

        void UpdateLocator()
        {
            PlayerGPS playerGPS = GameManager.Instance.PlayerGPS;
            bool isInsideBuilding = GameManager.Instance.IsPlayerInside;
            bool isResting = GameManager.Instance.PlayerEntity.IsResting;

            

            // Update if when player is resting
            if (isResting != wasResting)
            {
                wasResting = isResting;
                if (!isResting)
                {
                    UpdateMarkers();
                }
            }

            // Update when enter-exit a building
            if (isInsideBuilding != wasInsideBuilding)
            {
                wasInsideBuilding = isInsideBuilding;
                if (isInsideBuilding)
                {
                    ClearMarkers(); // Clean markers when enter
                }
                else if (isInLocationRect)
                {
                    UpdateMarkers(); // Update if is on location when exit
                }
            }

            // Detect if player is inside a location (Logic of Trancisions)
            if (playerGPS.IsPlayerInLocationRect != isInLocationRect)
            {
                isInLocationRect = playerGPS.IsPlayerInLocationRect;
                if (isInLocationRect)
                {
                    if (GameManager.Instance.StreamingWorld.CurrentPlayerLocationObject == null) //Check if CurrentPlaterLocationObject is null
                    {
                        needsUpdateAfterTransition = true; // If is null, needsUpdate
                    }
                    else
                    {
                        UpdateMarkers(); // If not, update.
                    }
                }
                else
                {
                    ClearMarkers(); // Clean after leaving location
                    
                }
            }

            if (needsUpdateAfterTransition && GameManager.Instance.StreamingWorld.CurrentPlayerLocationObject != null && isInLocationRect)
            {
                UpdateMarkers();
                needsUpdateAfterTransition = false;
            }

            //Logic of fast travel, check if the last CurrentPlayerLocationObject is the same
            if (GameManager.Instance.StreamingWorld.CurrentPlayerLocationObject != lastPlayerLocation)
            {
                lastPlayerLocation = GameManager.Instance.StreamingWorld.CurrentPlayerLocationObject;
                ClearMarkers();
                StartCoroutine(WaitForCoordinatesAndUpdateMarkers());
            }

        }

        private IEnumerator WaitForCoordinatesAndUpdateMarkers()
        {
            yield return new WaitForSeconds(1f); // Wait for 1 second to allow coordinates to stabilize
            UpdateMarkers();
        }

        private IEnumerator WaitForBuildingsUpdate()
        {
            yield return new WaitForSeconds(1f); // Wait for 1 second to allow coordinates to stabilize
            UpdateMarkers();
        }

    }

}
