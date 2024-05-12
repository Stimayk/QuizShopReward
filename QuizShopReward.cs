using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Newtonsoft.Json;
using QuizApi;
using ShopAPI;

namespace QuizShopReward
{
    public class QuizShopReward : BasePlugin
    {
        public override string ModuleName => "QuizShopReward";
        public override string ModuleAuthor => "E!N";
        public override string ModuleVersion => "v1.0";
        public override string ModuleDescription => "Module that adds rewards for quiz participation";

        private IQuizApi? QUIZ_API;
        private IShopApi? SHOP_API;
        private QuizShopRewardConfig? _config;
        private int _Min;
        private int _Max;

        public override void OnAllPluginsLoaded(bool hotReload)
        {
            string configDirectory = GetConfigDirectory();
            EnsureConfigDirectory(configDirectory);
            string configPath = Path.Combine(configDirectory, "QuizShopRewardConfig.json");
            _config = QuizShopRewardConfig.Load(configPath);

            QUIZ_API = IQuizApi.Capability.Get();
            SHOP_API = IShopApi.Capability.Get();

            if (QUIZ_API == null || SHOP_API == null)
            {
                Console.WriteLine($"{ModuleName} | Error: Quiz or Shop API is not available.");
                return;
            }

            InitializeQuizShopReward();

            QUIZ_API.OnPlayerWin += HandlePlayerWin;
        }

        private static string GetConfigDirectory()
        {
            return Path.Combine(Server.GameDirectory, "csgo/addons/counterstrikesharp/configs/plugins/Quiz/Modules");
        }

        private void EnsureConfigDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
                Console.WriteLine($"{ModuleName} | Created configuration directory at: {directoryPath}");
            }
        }

        private void InitializeQuizShopReward()
        {
            if (_config == null)
            {
                Console.WriteLine($"{ModuleName} | Error: Configuration is not loaded.");
                return;
            }

            _Min = _config.WinMin;
            _Max = _config.WinMax;
            Console.WriteLine($"{ModuleName} | Initialized: Min = {_Min}, Max = {_Max}");
        }

        private void HandlePlayerWin(CCSPlayerController player)
        {
            if (QUIZ_API != null)
            {
                int reward = new Random().Next(_Min, _Max);
                player.PrintToChat($"{Localizer["RewardWin", QUIZ_API.GetTranslatedText("Prefix"), reward]}");
                SHOP_API?.SetClientCredits(player, reward);
            }
        }
    }

    public class QuizShopRewardConfig
    {
        public int WinMin { get; set; } = 1;
        public int WinMax { get; set; } = 50;

        public static QuizShopRewardConfig Load(string configPath)
        {
            if (!File.Exists(configPath))
            {
                QuizShopRewardConfig defaultConfig = new();
                File.WriteAllText(configPath, JsonConvert.SerializeObject(defaultConfig, Formatting.Indented));
                return defaultConfig;
            }

            string json = File.ReadAllText(configPath);
            return JsonConvert.DeserializeObject<QuizShopRewardConfig>(json) ?? new QuizShopRewardConfig();
        }
    }
}
