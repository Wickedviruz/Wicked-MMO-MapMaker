using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using MapMaker.App.State;
using MapMaker.Core.Items;
using MapMaker.Core.Sprites;

namespace MapMaker.App.ViewModels;

public class ItemWorkspaceViewModel : INotifyPropertyChanged
{
    private ItemDefinition? _selectedItem;
    private SpriteEntry?    _selectedSprite;

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    // Item-lista
    public ObservableCollection<ItemDefinition> Items =>
        EditorSession.Current.Items;

    public ItemDefinition? SelectedItem
    {
        get => _selectedItem;
        set
        {
            _selectedItem = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasSelection));
            OnPropertyChanged(nameof(SpritePreview));
            RefreshFlags();
        }
    }

    private void RefreshFlags()
    {
        OnPropertyChanged(nameof(FlagWalkable));
        OnPropertyChanged(nameof(FlagBlockSolid));
        OnPropertyChanged(nameof(FlagBlockMissiles));
        OnPropertyChanged(nameof(FlagBlockPathfinder));
        OnPropertyChanged(nameof(FlagPickupable));
        OnPropertyChanged(nameof(FlagStackable));
        OnPropertyChanged(nameof(FlagMovable));
        OnPropertyChanged(nameof(FlagRotatable));
        OnPropertyChanged(nameof(FlagHangable));
        OnPropertyChanged(nameof(FlagReadable));
        OnPropertyChanged(nameof(FlagMultiUse));
        OnPropertyChanged(nameof(FlagFullGround));
        OnPropertyChanged(nameof(FlagHasElevation));
        OnPropertyChanged(nameof(FlagIgnoreLook));
    }

    public bool HasSelection => SelectedItem is not null;

    // Sprite-picker
    public ObservableCollection<SpriteEntry> Sprites =>
        EditorSession.Current.Atlas.Sprites;

    public SpriteEntry? SelectedSprite
    {
        get => _selectedSprite;
        set
        {
            _selectedSprite = value;
            OnPropertyChanged();
            if (value is not null && SelectedItem is not null)
            {
                SelectedItem.SpriteId = value.Id;
                OnPropertyChanged(nameof(SpritePreview));
            }
        }
    }

    // Preview av vald sprites för item
    public SpriteEntry? SpritePreview =>
        SelectedItem is not null
            ? EditorSession.Current.Atlas.GetById(SelectedItem.SpriteId)
            : null;

    // Flaggor — wrapper properties för binding
    public bool FlagWalkable
    {
        get => SelectedItem?.HasFlag(ItemFlags.Walkable) ?? false;
        set { SelectedItem?.SetFlag(ItemFlags.Walkable, value); OnPropertyChanged(); EditorSession.Current.MarkDirty(); }
    }
    public bool FlagBlockSolid
    {
        get => SelectedItem?.HasFlag(ItemFlags.BlockSolid) ?? false;
        set { SelectedItem?.SetFlag(ItemFlags.BlockSolid, value); OnPropertyChanged(); EditorSession.Current.MarkDirty(); }
    }
    public bool FlagBlockMissiles
    {
        get => SelectedItem?.HasFlag(ItemFlags.BlockMissiles) ?? false;
        set { SelectedItem?.SetFlag(ItemFlags.BlockMissiles, value); OnPropertyChanged(); EditorSession.Current.MarkDirty(); }
    }
    public bool FlagBlockPathfinder
    {
        get => SelectedItem?.HasFlag(ItemFlags.BlockPathfinder) ?? false;
        set { SelectedItem?.SetFlag(ItemFlags.BlockPathfinder, value); OnPropertyChanged(); EditorSession.Current.MarkDirty(); }
    }
    public bool FlagPickupable
    {
        get => SelectedItem?.HasFlag(ItemFlags.Pickupable) ?? false;
        set { SelectedItem?.SetFlag(ItemFlags.Pickupable, value); OnPropertyChanged(); EditorSession.Current.MarkDirty(); }
    }
    public bool FlagStackable
    {
        get => SelectedItem?.HasFlag(ItemFlags.Stackable) ?? false;
        set { SelectedItem?.SetFlag(ItemFlags.Stackable, value); OnPropertyChanged(); EditorSession.Current.MarkDirty(); }
    }
    public bool FlagMovable
    {
        get => SelectedItem?.HasFlag(ItemFlags.Movable) ?? false;
        set { SelectedItem?.SetFlag(ItemFlags.Movable, value); OnPropertyChanged(); EditorSession.Current.MarkDirty(); }
    }
    public bool FlagRotatable
    {
        get => SelectedItem?.HasFlag(ItemFlags.Rotatable) ?? false;
        set { SelectedItem?.SetFlag(ItemFlags.Rotatable, value); OnPropertyChanged(); EditorSession.Current.MarkDirty(); }
    }
    public bool FlagHangable
    {
        get => SelectedItem?.HasFlag(ItemFlags.Hangable) ?? false;
        set { SelectedItem?.SetFlag(ItemFlags.Hangable, value); OnPropertyChanged(); EditorSession.Current.MarkDirty(); }
    }
    public bool FlagReadable
    {
        get => SelectedItem?.HasFlag(ItemFlags.Readable) ?? false;
        set { SelectedItem?.SetFlag(ItemFlags.Readable, value); OnPropertyChanged(); EditorSession.Current.MarkDirty(); }
    }
    public bool FlagMultiUse
    {
        get => SelectedItem?.HasFlag(ItemFlags.MultiUse) ?? false;
        set { SelectedItem?.SetFlag(ItemFlags.MultiUse, value); OnPropertyChanged(); EditorSession.Current.MarkDirty(); }
    }
    public bool FlagFullGround
    {
        get => SelectedItem?.HasFlag(ItemFlags.FullGround) ?? false;
        set { SelectedItem?.SetFlag(ItemFlags.FullGround, value); OnPropertyChanged(); EditorSession.Current.MarkDirty(); }
    }
    public bool FlagHasElevation
    {
        get => SelectedItem?.HasFlag(ItemFlags.HasElevation) ?? false;
        set { SelectedItem?.SetFlag(ItemFlags.HasElevation, value); OnPropertyChanged(); EditorSession.Current.MarkDirty(); }
    }
    public bool FlagIgnoreLook
    {
        get => SelectedItem?.HasFlag(ItemFlags.IgnoreLook) ?? false;
        set { SelectedItem?.SetFlag(ItemFlags.IgnoreLook, value); OnPropertyChanged(); EditorSession.Current.MarkDirty(); }
    }

    // Enums
    public Array ItemTypes   => Enum.GetValues(typeof(ItemType));
    public Array StackOrders => Enum.GetValues(typeof(StackOrder));

    // Commands
    public ICommand AddItemCommand    { get; }
    public ICommand RemoveItemCommand { get; }
    public ICommand SaveItemsCommand  { get; }
    public ICommand LoadItemsCommand  { get; }

    private readonly Window _window;

    public ItemWorkspaceViewModel(Window window)
    {
        _window           = window;
        AddItemCommand    = new RelayCommand(AddItem);
        RemoveItemCommand = new RelayCommand(RemoveItem);
        SaveItemsCommand  = new RelayCommand(SaveItems);
        LoadItemsCommand  = new RelayCommand(LoadItems);

        EditorSession.Current.AtlasChanged += () =>
        OnPropertyChanged(nameof(Sprites));
    }

    private void AddItem()
    {
        var item = new ItemDefinition
        {
            Id   = NextItemId(),
            Name = $"Item {Items.Count + 1}"
        };
        Items.Add(item);
        SelectedItem = item;
        EditorSession.Current.MarkDirty();
    }

    private void RemoveItem()
    {
        if (SelectedItem is null) return;
        Items.Remove(SelectedItem);
        SelectedItem = null;
        EditorSession.Current.MarkDirty();
    }

    private async void SaveItems()
    {
        var folders = await _window.StorageProvider.OpenFolderPickerAsync(
            new FolderPickerOpenOptions { Title = "Välj mapp att spara items i" });

        if (folders.Count == 0) return;

        var path = System.IO.Path.Combine(folders[0].Path.LocalPath, "items.wdef");
        MapMaker.Core.Items.ItemDefSerializer.Save(Items, path);
        EditorSession.Current.HasUnsavedChanges = false;
    }

    private async void LoadItems()
    {
        var files = await _window.StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                Title = "Ladda items.wdef",
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("Wicked Item Def") { Patterns = new[] { "*.wdef" } }
                }
            });

        if (files.Count == 0) return;

        var loaded = MapMaker.Core.Items.ItemDefSerializer.Load(files[0].Path.LocalPath);
        EditorSession.Current.Items.Clear();
        foreach (var item in loaded)
            EditorSession.Current.Items.Add(item);

        OnPropertyChanged(nameof(Items));
    }

    private uint NextItemId()
    {
        if (Items.Count == 0) return 100;
        return Items.Max(i => i.Id) + 1;
    }
}