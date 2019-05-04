using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Blocks
{
    public class MenuManager : MonoBehaviour
    {
        public Canvas pauseMenu;
        public Canvas mainMenu;
        public Canvas loadWorldMenu;
        public Canvas generateWorldMenu;
        public Canvas optionsMenu;







        public UnityEngine.UI.Button creativeModeButton;
        public UnityEngine.UI.Button survivalModeButton;
        public void EnableCreativeMode()
        {
            World.creativeMode = true;
            survivalModeButton.interactable = true;
            creativeModeButton.interactable = false;
        }

        public void EnableSurvivalMode()
        {
            World.creativeMode = false;
            survivalModeButton.interactable = false;
            creativeModeButton.interactable = true;
        }


        void OnLevelWasLoaded(int level)
        {
            Awake();
        }

        void OnEnable()
        {
            //Tell our 'OnLevelFinishedLoading' function to start listening for a scene change as soon as this script is enabled.
            SceneManager.sceneLoaded += OnLevelFinishedLoading;
        }

        void OnLevelFinishedLoading(Scene arg0, LoadSceneMode arg1)
        {
            Awake();
        }

        void OnDisable()
        {
            //Tell our 'OnLevelFinishedLoading' function to stop listening for a scene change as soon as this script is disabled. Remember to always have an unsubscription for every delegate you subscribe to!
            SceneManager.sceneLoaded -= OnLevelFinishedLoading;
        }

        void Awake()
        {
            smoothMouseLook = FindObjectOfType<SmoothMouseLook>();
            blocksWorld = FindObjectOfType<BlocksWorld>();

            // add the pause menu to us if we don't have it already
            if (GetComponent<PauseMenu>() == null)
            {
                gameObject.AddComponent<PauseMenu>();
            }

            if (blocksWorld != null && smoothMouseLook != null)
            {
                curMenu = MenuStatus.Hotbar;
                paused = false;
                showingInventory = false;
                showingBlockInventory = false;

                HideAllMenus();
                Debug.Log("had awake with world");
            }
            else
            {
                HideAllMenus();
                curMenu = MenuStatus.MainMenu;
                mainMenu.enabled = true;
                Debug.Log("had awake with no world");
            }
        }

        void HideAllMenus()
        {
            pauseMenu.enabled = false;
            mainMenu.enabled = false;
            loadWorldMenu.enabled = false;
            generateWorldMenu.enabled = false;
            optionsMenu.enabled = false;
        }

        bool allowedToCapture = false;
        public SmoothMouseLook smoothMouseLook;
        bool showingInventory = false;
        bool showingBlockInventory = false;
        public BlocksWorld blocksWorld;

        public void CloseMenu()
        {
            HideAllMenus();
            paused = false;
            curMenu = MenuStatus.Hotbar;
        }

        public void OpenMainMenu()
        {
            if (blocksWorld != null)
            {
                Debug.LogWarning("warning: opened menu: why u do this?");
            }
            HideAllMenus();
            mainMenu.enabled = true;
            curMenu = MenuStatus.MainMenu;
        }

        public void CloseOptionsMenu()
        {
            if (blocksWorld != null)
            {
                curMenu = MenuStatus.Paused;
                HideAllMenus();
                pauseMenu.enabled = true;
            }
        }

        public void OpenOptions()
        {
            if (blocksWorld != null)
            {
                survivalModeButton.interactable = World.creativeMode;
                creativeModeButton.interactable = !World.creativeMode;
                curMenu = MenuStatus.Options;
                HideAllMenus();
                optionsMenu.enabled = true;
            }
            //curMenu = MenuStatus.Options;
        }




        public class WorldMaybeLoad
        {
            public string rootPath;
            public string worldName;
            public WorldGenOptions worldGenOptions;

            public WorldMaybeLoad(string rootPath)
            {
                this.rootPath = rootPath;
                DirectoryInfo curDirInfo = new DirectoryInfo(rootPath);
                this.worldName = curDirInfo.Name;
                if (curDirInfo.Exists)
                {
                    isValid = true;
                }
                try
                {
                    worldGenOptions = LitJson.JsonMapper.ToObject<WorldGenOptions>(System.IO.File.ReadAllText(System.IO.Path.Combine(curDirInfo.FullName, "worldGenOptions.json")));
                }
                catch (System.Exception e)
                {
                    isValid = false;
                    Debug.LogWarning("exception when loading world from path " + rootPath + " of: " + e);
                }
            }

            public bool isValid = false;
        }

        WorldMaybeLoad[] GetWorlds(string baseDirectory)
        {
            List<WorldMaybeLoad> result = new List<WorldMaybeLoad>();
            DirectoryInfo baseDirInfo = new DirectoryInfo(baseDirectory);
            if (baseDirInfo.Exists)
            {
                foreach (DirectoryInfo dir in baseDirInfo.EnumerateDirectories())
                {
                    WorldMaybeLoad dirWorld = new WorldMaybeLoad(dir.FullName);
                    if (dirWorld.isValid)
                    {
                        result.Add(dirWorld);
                    }
                }
            }
            return result.ToArray();
        }


        public RectTransform loadWorldList;
        public GameObject loadWorldButtonPrefab;

        public void LoadWorld()
        {
            // for some reason we need to call this multiple times to get it to actually show up
            LoadWorldHelper();
            HideAllMenus();
            loadWorldMenu.enabled = true;
            LoadWorldHelper();
            LoadWorldHelper();
        }
        public void LoadWorldHelper()
        {
            WorldMaybeLoad[] worlds = GetWorlds(World.ROOT_SAVE_DIR);


            // Destroy previous elements in the list
            List<RectTransform> children = new List<RectTransform>();
            foreach (RectTransform child in loadWorldList)
            {
                children.Add(child);
            }

            for(int i = 0; i < children.Count; i++)
            {
                GameObject.Destroy(children[i].gameObject);
            }
            
            // Add one button for each world in our save folder
            foreach (WorldMaybeLoad world in worlds)
            {
                UnityEngine.UI.Button loadWorldButton = GameObject.Instantiate(loadWorldButtonPrefab).GetComponent<UnityEngine.UI.Button>();
                loadWorldButton.GetComponentInChildren<UnityEngine.UI.Text>().text = world.worldName;
                loadWorldButton.onClick.AddListener(() =>
                {
                    LoadSpecificWorld(world);
                });
                loadWorldButton.GetComponent<RectTransform>().SetParent(loadWorldList);
            }
            // force reload layout so they get pushed accordingly
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(loadWorldList);

            HideAllMenus();
            loadWorldMenu.enabled = true;

            curMenu = MenuStatus.LoadWorld;
        }


        public static string WORLD_SCENE_NAME = "Blocks/Scenes/MainWorld";
        public static string MAIN_MENU_SCENE_NAME = "Blocks/Scenes/MainMenu";

        public void LoadSpecificWorld(WorldMaybeLoad world)
        {
            World.currentWorldDir = world.rootPath;
            World.worldGenOptions = world.worldGenOptions;
            World.needToGenerate = false;
            curMenu = MenuStatus.Hotbar;
            UnityEngine.SceneManagement.SceneManager.LoadScene(WORLD_SCENE_NAME);
        }


        public void NewWorld()
        {
            HideAllMenus();
            generateWorldMenu.enabled = true;
            curMenu = MenuStatus.GenerateWorld;
        }


        public UnityEngine.UI.InputField generateSeedInput;
        public UnityEngine.UI.InputField generateWorldNameInput;

        public void GenerateNewWorld()
        {
            string seed = generateSeedInput.text;
            string worldName = generateWorldNameInput.text;

            long seedVal = 0;
            if (long.TryParse(seed, out seedVal))
            {
                // got seed, we are good
            }
            else
            {
                // convert numbers into ascii values, mod 32, then just interpret as base 32 number because reasons
                long curDigit = 1;
                long baseUsing = 32;

                for (int i = 0; i < seed.Length; i++)
                {
                    seedVal += ((long)seed[i] % baseUsing) * curDigit;
                    curDigit *= baseUsing;
                }
            }

            WorldGenOptions options = new WorldGenOptions((int)seedVal, World.VERSION_STRING);
            CreateWorld(worldName, options);
        }



        public void SaveAndQuit()
        {
            World.mainWorld.Save(World.currentWorldDir);
            UnityEngine.SceneManagement.SceneManager.LoadScene(MAIN_MENU_SCENE_NAME);
        }

        public void CreateWorld(string worldName, WorldGenOptions worldGenOptions)
        {
            World.currentWorldDir = Path.Combine(World.ROOT_SAVE_DIR, worldName);
            World.worldGenOptions = worldGenOptions;
            World.needToGenerate = true;
            UnityEngine.SceneManagement.SceneManager.LoadScene(WORLD_SCENE_NAME);
        }


        // Start is called before the first frame update
        void Start()
        {
            paused = false;
        }

        LVector3 blockShowing;
        Inventory blockShowingInventory;
        BlocksPlayer playerInventoryShowing;
        BlocksPlayer playerHotbarShowing;

        public BlocksPlayer activeGuiPlayer;

        public void ShowBlockInventory(BlocksPlayer playerUsing, LVector3 block)
        {

            Inventory blockInventory;
            BlockOrItem customBlock;
            if (blocksWorld.world.BlockHasInventory(block, out blockInventory) && blocksWorld.blocksPack.customBlocks.ContainsKey(block.BlockV, out customBlock))
            {
                ShowPlayerInventory(playerUsing);
                blockShowing = block;
                blockShowingInventory = blockInventory;
                blocksWorld.otherObjectInventoryGui.displaying = true;
                blocksWorld.otherObjectInventoryGui.playerUsing = playerUsing;
                blocksWorld.otherObjectInventoryGui.inventory = blockInventory;
                blocksWorld.otherObjectInventoryGui.inventory = blockInventory;
                blocksWorld.otherObjectInventoryGui.screenOffset = new Vector2(0, 300);
                blocksWorld.otherObjectInventoryGui.customBlockOwner = customBlock;
                blocksWorld.otherObjectInventoryGui.customBlockOwnerPosition = block;
                blocksWorld.otherObjectInventoryGui.numRows = customBlock.NumInventoryRows();
                showingInventory = true;
                showingBlockInventory = true;
            }
            else
            {
                HidePlayerInventory();
                blockShowing = LVector3.Invalid;
                blockShowingInventory = null;
                showingBlockInventory = false;
                showingInventory = false;
            }
        }

        public void HidePlayerInventory()
        {
            showingInventory = false;
            showingBlockInventory = false;

            blocksWorld.otherObjectInventoryGui.displaying = false;
            blocksWorld.otherObjectInventoryGui.playerUsing = null;
            blocksWorld.otherObjectInventoryGui.inventory = null;
            blockShowingInventory = null;
            blockShowing = LVector3.Invalid;
        }

        public void ShowPauseMenu()
        {
            HideAllMenus();
            pauseMenu.enabled = true;
        }

        public void ShowPlayerInventory(BlocksPlayer player)
        {
            activeGuiPlayer = player;
            showingInventory = true;
            showingBlockInventory = false;
            playerInventoryShowing = player;
            playerInventoryShowing.inventoryGui.maxItems = -1;
            playerInventoryShowing.inventoryGui.numRows = 4;
            playerInventoryShowing.inventoryGui.screenOffset.y = -Screen.height / 2.0f + 300.0f;
            playerInventoryShowing.inventoryGui.inventory = player.inventory;
            playerInventoryShowing.inventoryGui.playerUsing = playerInventoryShowing;
            playerInventoryShowing.inventoryGui.enabled = true;
        }

        public MenuStatus curMenuStatus;
        [HideInInspector]
        public bool paused = false;
        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                paused = true;
                ShowPauseMenu();
            }
            if (paused)
            {
                allowedToCapture = false;
                if (smoothMouseLook != null)
                {
                    smoothMouseLook.allowedToCapture = allowedToCapture;
                }
                return;
            }

            if (!showingInventory)
            {
                showingBlockInventory = false;
            }

            if (showingInventory)
            {
                allowedToCapture = false;
            }
            else if(!paused)
            {
                allowedToCapture = true;
            }

            if (smoothMouseLook != null)
            {
                smoothMouseLook.allowedToCapture = allowedToCapture;
            }


















            curMenuStatus = CurrentMenu;

            // in world
            if (blocksWorld != null && smoothMouseLook != null)
            {

                // if the current gui player is disabled, we need to search for which one we are actually using
                if (activeGuiPlayer == null || !activeGuiPlayer.enabled)
                {
                    activeGuiPlayer = null;
                    foreach (BlocksPlayer player in FindObjectsOfType<BlocksPlayer>())
                    {
                        if (player.enabled)
                        {
                            activeGuiPlayer = player;
                            break;
                        }
                    }
                    if (activeGuiPlayer != null)
                    {
                        smoothMouseLook = activeGuiPlayer.GetComponent<SmoothMouseLook>();
                    }
                }

                if (curMenu == MenuStatus.LoadWorld || curMenu == MenuStatus.GenerateWorld || curMenu == MenuStatus.MainMenu)
                {
                    curMenu = MenuStatus.Hotbar;
                    showingInventory = false;
                    showingBlockInventory = false;
                    paused = false;
                }
                if (curMenuStatus != MenuStatus.BlockInventory && curMenuStatus != MenuStatus.Inventory)
                {
                    if (playerInventoryShowing != null)
                    {
                        playerInventoryShowing.inventoryGui.displaying = false;
                        playerInventoryShowing = null;
                    }
                    if (blockShowing != LVector3.Invalid || blockShowingInventory != null)
                    {
                        blocksWorld.otherObjectInventoryGui.displaying = false;
                        blocksWorld.otherObjectInventoryGui.playerUsing = null;
                        blocksWorld.otherObjectInventoryGui.inventory = null;
                        blockShowingInventory = null;
                        blockShowing = LVector3.Invalid;
                    }
                }

                if (curMenuStatus == MenuStatus.Inventory)
                {
                    if (playerInventoryShowing == null)
                    {
                        blocksWorld.otherObjectInventoryGui.displaying = false;
                        blocksWorld.otherObjectInventoryGui.playerUsing = null;
                    }
                }


                if (curMenuStatus == MenuStatus.Hotbar)
                {
                    playerHotbarShowing = activeGuiPlayer;
                    playerHotbarShowing.inventoryGui.numRows = 1;
                    playerHotbarShowing.inventoryGui.inventory = playerHotbarShowing.inventory;
                    playerHotbarShowing.inventoryGui.maxItems = playerHotbarShowing.hotbarSize;
                    playerHotbarShowing.inventoryGui.screenOffset.y = -Screen.height / 2.0f + 100.0f;
                    playerHotbarShowing.inventoryGui.displaying = true;
                }
                else
                {
                    if (playerHotbarShowing != null)
                    {
                        //playerHotbarShowing.inventoryGui.displaying = false;
                        //playerHotbarShowing = null;
                    }
                }

            }
            // in main menu
            else
            {
                if (curMenuStatus == MenuStatus.Hotbar || curMenuStatus == MenuStatus.Inventory || curMenuStatus == MenuStatus.BlockInventory)
                {
                    curMenu = MenuStatus.MainMenu;
                }
            }

        }

        public enum MenuStatus
        {
            Hotbar,
            Inventory,
            BlockInventory,
            Paused,
            GenerateWorld,
            LoadWorld,
            Options,
            MainMenu
        }

        MenuStatus curMenu = MenuStatus.Hotbar;


        public MenuStatus CurrentMenu
        {
            get
            {
                // in world
                if (smoothMouseLook != null)
                {
                    if (paused)
                    {
                        if (curMenu == MenuStatus.Options)
                        {
                            return MenuStatus.Options;
                        }
                        else
                        {
                            return MenuStatus.Paused;
                        }
                    }
                    else
                    {
                        if (showingInventory)
                        {
                            if (showingBlockInventory)
                            {
                                return MenuStatus.BlockInventory;
                            }
                            else
                            {
                                return MenuStatus.Inventory;
                            }
                        }
                        else
                        {
                            return MenuStatus.Hotbar;
                        }
                    }
                }
                // in main menu
                else
                {
                    return curMenu;
                }
            }
            private set
            {

            }
        }
    }


}