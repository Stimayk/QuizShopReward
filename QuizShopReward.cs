using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using QuizApi;
using ShopAPI;

namespace QuizShopReward
{
    public class QuizShopReward : BasePlugin
    {
        public override string ModuleName => "QuizShopReward";
        public override string ModuleAuthor => "E!N";
        public override string ModuleVersion => "v1.1";
        public override string ModuleDescription => "Module that adds rewards for quiz participation";

        private IQuizApi? QUIZ_API;
        private IShopApi? SHOP_API;
        private QuizShopRewardConfig? _config;
        private int _Min;
        private int _Max;
        private int _Winning;

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
                Logger.LogError($"Quiz or Shop API is not available.");
                return;
            }

            InitializeQuizShopReward();

            QUIZ_API.OnQuizStart += HandleQuizStart;
            QUIZ_API.OnQuizEnd += HandleQuizEnd;
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
                Logger.LogInformation($"Created configuration directory at: {directoryPath}");
            }
        }

        private void InitializeQuizShopReward()
        {
            if (_config == null)
            {
                Logger.LogError($"Configuration is not loaded.");
                return;
            }

            _Min = _config.WinMin;
            _Max = _config.WinMax;
            Logger.LogInformation($"Initialized: Min = {_Min}, Max = {_Max}");
        }

        private void HandleQuizStart()
        {
            if (QUIZ_API != null)
            {
                int reward = new Random().Next(_Min, _Max);
                _Winning = reward;
                Server.PrintToChatAll($"{Localizer["Reward", QUIZ_API.GetTranslatedText("Prefix"), reward]}");
            }
        }

        private void HandlePlayerWin(CCSPlayerController player)
        {
            if (QUIZ_API != null && SHOP_API != null)
            {
                player.PrintToChat($"{Localizer["RewardWin", QUIZ_API.GetTranslatedText("Prefix"), _Winning]}");
                int credits = (SHOP_API.GetClientCredits(player));
                SHOP_API.SetClientCredits(player, credits + _Winning);
            }
        }

        private void HandleQuizEnd()
        {
            if (QUIZ_API != null)
            {
                Server.PrintToChatAll($"{Localizer["RewardLose", QUIZ_API.GetTranslatedText("Prefix"), _Winning]}");
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
