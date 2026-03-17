using System;
using System.Windows.Input;

namespace MapMaker.App.ViewModels;

public class RelayCommand(Action execute) : ICommand
{
    public event EventHandler? CanExecuteChanged;
    public bool CanExecute(object? parameter) => true;
    public void Execute(object? parameter) => execute();
}