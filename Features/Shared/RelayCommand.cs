namespace tetris_fight.Features.Shared;

using System.Windows.Input;

public sealed class RelayCommand<T> : ICommand
{
    private readonly Action<T?> _execute;

    public RelayCommand(Action<T?> execute) => _execute = execute;

    public event EventHandler? CanExecuteChanged { add { } remove { } }
    public bool CanExecute(object? parameter) => true;
    public void Execute(object? parameter) => _execute((T?)parameter);
}
