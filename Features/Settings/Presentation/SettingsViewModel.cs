namespace tetris_fight.Features.Settings.Presentation;

using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Input;
using tetris_fight.Features.Settings.Application;
using tetris_fight.Features.Shared;

public sealed class SettingsViewModel : INotifyPropertyChanged
{
    private readonly GameSettings _settings;

    public SettingsViewModel()
    {
        _settings = GameSettingsService.Current;
        StartCaptureCommand = new RelayCommand<string>(StartCapture);
        ResetCommand = new RelayCommand<object>(_ => Reset());
        CloseDuplicateKeyDialogCommand = new RelayCommand<object>(_ => CloseDuplicateKeyDialog());
    }

    public RelayCommand<string> StartCaptureCommand { get; }
    public RelayCommand<object> ResetCommand { get; }
    public RelayCommand<object> CloseDuplicateKeyDialogCommand { get; }

    private ControlAction? _capturingAction;
    public ControlAction? CapturingAction
    {
        get => _capturingAction;
        private set
        {
            _capturingAction = value;
            OnPropertyChanged();
            NotifyControlLabels();
        }
    }

    public double MusicVolumePercent
    {
        get => Math.Round(_settings.MusicVolume * 100);
        set
        {
            _settings.MusicVolume = Math.Clamp(value / 100.0, 0.0, 1.0);
            Save();
            OnPropertyChanged();
            OnPropertyChanged(nameof(MusicVolumeText));
        }
    }

    public string MusicVolumeText => $"{MusicVolumePercent:0}%";

    public bool ShowGhostPiece
    {
        get => _settings.ShowGhostPiece;
        set
        {
            _settings.ShowGhostPiece = value;
            Save();
            OnPropertyChanged();
        }
    }

    public string MoveLeftButtonText => GetButtonText(ControlAction.MoveLeft);
    public string MoveRightButtonText => GetButtonText(ControlAction.MoveRight);
    public string MoveDownButtonText => GetButtonText(ControlAction.MoveDown);
    public string RotateClockwiseButtonText => GetButtonText(ControlAction.RotateClockwise);
    public string RotateCounterClockwiseButtonText => GetButtonText(ControlAction.RotateCounterClockwise);
    public string HardDropButtonText => GetButtonText(ControlAction.HardDrop);

    public string StatusText { get; private set; } = "Chaque joueur configure ses touches sur sa machine.";

    private bool _isDuplicateKeyDialogVisible;
    public bool IsDuplicateKeyDialogVisible
    {
        get => _isDuplicateKeyDialogVisible;
        private set { _isDuplicateKeyDialogVisible = value; OnPropertyChanged(); }
    }

    public string DuplicateKeyDialogText { get; private set; } = "";

    public void AssignCapturedKey(Key key)
    {
        if (CapturingAction is not { } action)
            return;

        if (key is Key.Escape or Key.Enter)
        {
            CapturingAction = null;
            StatusText = "Capture annulée.";
            OnPropertyChanged(nameof(StatusText));
            return;
        }

        if (key == Key.None)
            return;

        if (_settings.LocalPlayerControls.FindActionForKey(key, action) is { } assignedAction)
        {
            CapturingAction = null;
            ShowDuplicateKeyDialog(key, assignedAction);
            return;
        }

        _settings.LocalPlayerControls.SetKey(action, key);
        CapturingAction = null;
        StatusText = "Touches sauvegardées.";
        Save();
        OnPropertyChanged(nameof(StatusText));
        NotifyControlLabels();
    }

    private void StartCapture(string? actionName)
    {
        if (!Enum.TryParse<ControlAction>(actionName, out var action))
            return;

        CapturingAction = action;
        StatusText = "Appuyez sur une touche, Échap pour annuler.";
        OnPropertyChanged(nameof(StatusText));
    }

    private string GetButtonText(ControlAction action) =>
        CapturingAction == action
            ? "..."
            : FormatKey(_settings.LocalPlayerControls.GetKey(action));

    private static string FormatKey(Key key) => key switch
    {
        Key.Left => "←",
        Key.Right => "→",
        Key.Up => "↑",
        Key.Down => "↓",
        Key.Space => "ESPACE",
        _ => key.ToString().ToUpperInvariant()
    };

    private static string FormatAction(ControlAction action) => action switch
    {
        ControlAction.MoveLeft => "Déplacer à gauche",
        ControlAction.MoveRight => "Déplacer à droite",
        ControlAction.MoveDown => "Descendre",
        ControlAction.RotateClockwise => "Rotation horaire",
        ControlAction.RotateCounterClockwise => "Rotation antihoraire",
        ControlAction.HardDrop => "Hard drop",
        _ => action.ToString()
    };

    private void ShowDuplicateKeyDialog(Key key, ControlAction assignedAction)
    {
        DuplicateKeyDialogText = $"La touche {FormatKey(key)} est déjà attribuée à : {FormatAction(assignedAction)}.";
        IsDuplicateKeyDialogVisible = true;
        StatusText = "Choisissez une autre touche.";
        OnPropertyChanged(nameof(DuplicateKeyDialogText));
        OnPropertyChanged(nameof(StatusText));
    }

    public void CloseDuplicateKeyDialog()
    {
        IsDuplicateKeyDialogVisible = false;
    }

    private void Reset()
    {
        GameSettingsService.Reset();
        _settings.MusicVolume = GameSettingsService.Current.MusicVolume;
        _settings.ShowGhostPiece = GameSettingsService.Current.ShowGhostPiece;
        _settings.LocalPlayerControls = GameSettingsService.Current.LocalPlayerControls;
        CapturingAction = null;
        StatusText = "Paramètres réinitialisés.";
        OnPropertyChanged(nameof(StatusText));
        OnPropertyChanged(nameof(MusicVolumePercent));
        OnPropertyChanged(nameof(MusicVolumeText));
        OnPropertyChanged(nameof(ShowGhostPiece));
        NotifyControlLabels();
    }

    private void Save() => GameSettingsService.Save(_settings);

    private void NotifyControlLabels()
    {
        OnPropertyChanged(nameof(MoveLeftButtonText));
        OnPropertyChanged(nameof(MoveRightButtonText));
        OnPropertyChanged(nameof(MoveDownButtonText));
        OnPropertyChanged(nameof(RotateClockwiseButtonText));
        OnPropertyChanged(nameof(RotateCounterClockwiseButtonText));
        OnPropertyChanged(nameof(HardDropButtonText));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
