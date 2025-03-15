using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Distance.CustomCar.Data.Car;
using Distance.CustomCar.Data.Errors;
using Events.MainMenu;
using HarmonyLib;
using System;

namespace Distance.CustomCar
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public sealed class Mod : BaseUnityPlugin
    {
        //Mod Details
        private const string modGUID = "Distance.CustomCar";
        private const string modName = "Custom Car";
        private const string modVersion = "1.1.2";

        //Config Entry Strings
        public static string UseTrumpetKey = "Use Trumpet Horn";

        //Config Entries
        public static ConfigEntry<bool> UseTrumpetHorn { get; set; }

        //Public Varibles
        public static int DefaultCarCount { get; private set; }
        public static int ModdedCarCount => TotalCarCount - DefaultCarCount;
        public static int TotalCarCount { get; private set; }
        public ErrorList Errors { get; set; }
        public ProfileCarColors CarColors { get; set; }

        //Other
        private static readonly Harmony harmony = new Harmony(modGUID);
        public static ManualLogSource Log = new ManualLogSource(modName);
        public static Mod Instance;
        private bool displayErrors_ = true;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            Log = BepInEx.Logging.Logger.CreateLogSource(modGUID);
            Logger.LogInfo("Thanks for using Custom Cars!");

            Errors = new ErrorList(Logger);

            CarColors = gameObject.AddComponent<ProfileCarColors>();

            //Config Setup
            UseTrumpetHorn = Config.Bind("General",
                UseTrumpetKey,
                false,
                new ConfigDescription("Custom car models will use the encryptor horn (the \"doot!\" trumpet)."));

            //Apply Patches
            Logger.LogInfo("Loading...");
            harmony.PatchAll();
            Logger.LogInfo("Loaded!");
        }

        public void Start()
        {
            ProfileManager profileManager = G.Sys.ProfileManager_;
            DefaultCarCount = profileManager.CarInfos_.Length;

            CarInfos carInfos = new CarInfos();
            carInfos.CollectInfos();
            CarBuilder carBuilder = new CarBuilder();

            carBuilder.CreateCars(carInfos);
            
            TotalCarCount = profileManager.CarInfos_.Length;
            CarColors.LoadAll();

            Errors.Show();
        }

        private void OnEnable()
        {
            Initialized.Subscribe(OnMainMenuLoaded);
        }

        private void OnDisable()
        {
            Initialized.Unsubscribe(OnMainMenuLoaded);
        }

        private void OnMainMenuLoaded(Initialized.Data _)
        {
            if (displayErrors_)
            {
                Errors.Show();
                displayErrors_ = false;
            }
        }

        private void OnConfigChanged(object sender, EventArgs e)
        {
            SettingChangedEventArgs settingChangedEventArgs = e as SettingChangedEventArgs;

            if (settingChangedEventArgs == null) return;
        }
    }
}
