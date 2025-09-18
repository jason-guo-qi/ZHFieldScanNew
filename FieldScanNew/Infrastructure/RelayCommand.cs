// Infrastructure/RelayCommand.cs
using System;
using System.Windows.Input;

namespace FieldScanNew.Infrastructure // <-- 修正命名空间
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Predicate<object?>? _canExecute;

        public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        // 为参数添加 '?'
        public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

        // 为参数添加 '?'
        public void Execute(object? parameter) => _execute(parameter);

        // 为事件添加 '?'
        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}