using System.IO;
using System.Reflection;

using MelonLoader;
using NEP.DOOMBBQ.Data;
using UnityEngine;

using NEP.DOOMBBQ.WAD;
using NEP.DOOMBBQ.Game;
using NEP.DOOMBBQ.Entities;
using NEP.DOOMBBQ.Rendering;
using NEP.DOOMBBQ.Sound;
using JoelG.ENA4;

namespace NEP.DOOMBBQ
{
    public static class BuildInfo
    {
        public const string Name = "DOOMBBQ"; // Name of the Mod.  (MUST BE SET)
        public const string Description = "What the... Demons... in MY BBQ? GET THE HELL OUT"; // Description for the Mod.  (Set as null if none)
        public const string Author = "Not Enough Photons, adamdev"; // Author of the Mod.  (MUST BE SET)
        public const string Company = "Not Enough Photons"; // Company that made the Mod.  (Set as null if none)
        public const string Version = "0.1.0"; // Version of the Mod.  (MUST BE SET)
        public const string DownloadLink = null; // Download Link for the Mod.  (Set as null if none)
    }

    public class Main : MelonMod
    {
        public AssetBundle bundle;
        public static GameObject mobjTemplate;
        public static Material unlitMaterial;
        public static Mobj player;

        public static readonly string UserDataDirectory = MelonUtils.UserDataDirectory;
        public static readonly string TeamDirectory = Path.Combine(UserDataDirectory, "Not Enough Photons");
        public static readonly string ModDirectory = Path.Combine(TeamDirectory, "DOOMBBQ");
        public static readonly string IWADDirectory = Path.Combine(ModDirectory, "IWADS");
        public static readonly string PWADDirectory = Path.Combine(ModDirectory, "PWADS");

        public static GameObject mainPlayerObject;
        
        public static Texture2D MissingSprite;

        public static MelonLogger.Instance Logger;
        
        internal static T LoadPersistentAsset<T>(AssetBundle assetBundle, string name) where T : UnityEngine.Object
        {
            UnityEngine.Object asset = assetBundle.LoadAsset(name);

            if (asset != null)
            {
                asset.hideFlags = HideFlags.DontUnloadUnusedAsset;
                return (T)asset;
            }

            return null;
        }
        
        public override void OnInitializeMelon()
        {
            Logger = this.LoggerInstance;

            UnityExplorer.ExplorerStandalone.CreateInstance();

            Directory.CreateDirectory(ModDirectory);
            Directory.CreateDirectory(IWADDirectory);
            Directory.CreateDirectory(PWADDirectory);

            var assembly = Assembly.GetExecutingAssembly();
            string fileName = "doomlab.pack";
            Logger.Msg(Path.Combine(ModDirectory, "doombbq.pack"));
            bundle = AssetBundle.LoadFromFile(Path.Combine(ModDirectory, "doombbq.pack"));
            mobjTemplate = LoadPersistentAsset<GameObject>(bundle, "[MOBJ] - Null");
            MissingSprite = LoadPersistentAsset<Texture2D>(bundle, "faila0");
            unlitMaterial = LoadPersistentAsset<Material>(bundle, "mat_unlit");

            Logger.Msg(mobjTemplate.name);

            new WADManager();
            WADManager.Instance.LoadWAD(WADManager.Instance.GetIWAD());
            FrameBuilder.GenerateTable();

            DoomGame game = new DoomGame();
        }

        public override void OnUpdate()
        {
            if (DoomGame.Instance == null)
            {
                return;
            }

            DoomGame.Instance.Update();

            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                MobjManager.Instance.SpawnMobj(player.transform.position + player.transform.forward, MobjType.MT_PLAYER, 0f);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                MobjManager.Instance.SpawnMobj(player.transform.position + player.transform.forward, MobjType.MT_TROOP, 0f);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                MobjManager.Instance.SpawnMobj(player.transform.position + player.transform.forward, MobjType.MT_BOSSBRAIN, 0f);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                MobjManager.Instance.SpawnMobj(player.transform.position + player.transform.forward, MobjType.MT_CYBORG, 0f);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                MobjManager.Instance.SpawnMobj(player.transform.position + player.transform.forward, MobjType.MT_SERGEANT, 0f);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                MobjManager.Instance.SpawnMobj(player.transform.position + player.transform.forward, MobjType.MT_HEAD, 0f);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                MobjManager.Instance.SpawnMobj(player.transform.position + player.transform.forward, MobjType.MT_SHOTGUY, 0f);
            }
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            Logger.Msg("OnSceneWasLoaded");

            mainPlayerObject = Camera.main.gameObject;

            if (mainPlayerObject != null)
            {
                Logger.Msg("Found player!");
            }
            else
            {
                Logger.Msg("Unable to find player!");
                return;
            }

            mainPlayerObject.AddComponent<AudioListener>();

            DoomGame.Instance.gameTic = 0;

            new SoundManager();
            new MobjManager();

            if(player != null)
            {
                Mobj.ComponentCache.RemoveInstance(player.gameObject.GetInstanceID());
            }

            player = mainPlayerObject.AddComponent<Mobj>();
            player.gameObject.AddComponent<DoomPlayer>();
            player.flags ^= MobjFlags.MF_SOLID;
            player.flags ^= MobjFlags.MF_SHOOTABLE;
            Mobj.ComponentCache.AddInstance(player.gameObject.GetInstanceID(), player);
        }
    }
}
