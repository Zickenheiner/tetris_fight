namespace tetris_fight.Features.Settings.Application;

using System.Text.Json;
using System.Text.Json.Serialization;

public static class GameSettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private static readonly string SettingsDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "tetris_fight");

    private static readonly string SettingsPath = Path.Combine(SettingsDirectory, "settings.json");

    static GameSettingsService()
    {
        Current = Load();
    }

    public static GameSettings Current { get; private set; }

    public static event Action<GameSettings>? SettingsChanged;

    public static void Save(GameSettings settings)
    {
        settings.MusicVolume = Clamp01(settings.MusicVolume);
        settings.LocalPlayerControls ??= PlayerControlsSettings.Default();

        Directory.CreateDirectory(SettingsDirectory);
        File.WriteAllText(SettingsPath, JsonSerializer.Serialize(settings, JsonOptions));
        Current = settings;
        SettingsChanged?.Invoke(Current);
    }

    public static void Reset()
    {
        Save(new GameSettings());
    }

    private static GameSettings Load()
    {
        try
        {
            if (!File.Exists(SettingsPath))
                return new GameSettings();

            var settings = JsonSerializer.Deserialize<GameSettings>(
                File.ReadAllText(SettingsPath),
                JsonOptions) ?? new GameSettings();

            settings.MusicVolume = Clamp01(settings.MusicVolume);
            settings.LocalPlayerControls ??= PlayerControlsSettings.Default();
            return settings;
        }
        catch
        {
            return new GameSettings();
        }
    }

    private static double Clamp01(double value) => Math.Clamp(value, 0.0, 1.0);
}
