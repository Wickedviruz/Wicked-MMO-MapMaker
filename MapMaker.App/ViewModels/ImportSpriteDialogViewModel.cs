using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using MapMaker.App.State;
using MapMaker.Core.Sprites;

namespace MapMaker.App.ViewModels;

public class ImportSpriteDialogViewModel : INotifyPropertyChanged
{
    private readonly Window _window;
    private string _fileName = "Ingen fil vald";
    private bool _isSheet = false;

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    public string? FilePath { get; set; }

    public string FileName
    {
        get => _fileName;
        set { _fileName = value; OnPropertyChanged(); }
    }

    public bool IsSheet
    {
        get => _isSheet;
        set { _isSheet = value; OnPropertyChanged(); }
    }

    public decimal TileWidth  { get; set; } = 32;
    public decimal TileHeight { get; set; } = 32;
    public decimal Columns    { get; set; } = 1;
    public decimal Rows       { get; set; } = 1;
    public decimal Spacing    { get; set; } = 0;
    public decimal Margin     { get; set; } = 0;

    public bool Confirmed { get; private set; }
    public List<SpriteEntry> ImportedSprites { get; private set; } = new();

    public ICommand BrowseCommand  { get; }
    public ICommand ImportCommand  { get; }
    public ICommand CancelCommand  { get; }

    public ImportSpriteDialogViewModel(Window window)
    {
        _window       = window;
        BrowseCommand = new RelayCommand(Browse);
        ImportCommand = new RelayCommand(Import);
        CancelCommand = new RelayCommand(Cancel);
    }

    private async void Browse()
    {
        var files = await _window.StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                Title = "Välj PNG-fil",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("PNG-bilder") { Patterns = new[] { "*.png" } }
                }
            });

        if (files.Count == 0) return;
        FilePath = files[0].Path.LocalPath;
        FileName = System.IO.Path.GetFileName(FilePath);
    }

    private void Import()
    {
        if (FilePath is null) return;

        uint startId = (uint)(EditorSession.Current.Atlas.Sprites.Count + 1);

        ImportResult result;

        if (IsSheet)
        {
            result = SpriteImporter.ImportSheet(
                FilePath,
                startId,
                SpriteCategory.Object,
                new SheetOptions
                {
                    TileWidth  = (int)TileWidth,
                    TileHeight = (int)TileHeight,
                    Columns    = (int)Columns,
                    Rows       = (int)Rows,
                    Spacing    = (int)Spacing,
                    Margin     = (int)Margin
                });
        }
        else
        {
            result = SpriteImporter.ImportSingle(
                FilePath,
                startId,
                System.IO.Path.GetFileNameWithoutExtension(FilePath),
                SpriteCategory.Object);
        }

        if (!result.Success)
        {
            // TODO: visa felmeddelande
            return;
        }

        ImportedSprites = result.ImportedSprites;
        Confirmed       = true;
        _window.Close();
    }

    private void Cancel() => _window.Close();
}