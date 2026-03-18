using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using MapMaker.App.State;
using MapMaker.Core.Assets;
using MapMaker.Core.Sprites;

namespace MapMaker.App.ViewModels;

public class SpriteWorkspaceViewModel : INotifyPropertyChanged
{
    private SpriteEntry? _selectedSprite;
    private string _editName = string.Empty;
    private SpriteCategory _editCategory;
    private SpriteEntry? _previewSprite;
    private SpriteAnimation? _previewAnimation;
    private Avalonia.Threading.DispatcherTimer? _animTimer;
    private int _frameIndex = 0;

    public AnimationEditorViewModel AnimationEditor { get; } = new();

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    public ObservableCollection<SpriteEntry> Sprites =>
        EditorSession.Current.Atlas.Sprites;

    public SpriteEntry? PreviewSprite
    {
        get => _previewSprite;
        set { _previewSprite = value; OnPropertyChanged(); }
    }

    public SpriteEntry? SelectedSprite
    {
        get => _selectedSprite;
        set
        {
            _selectedSprite = value;
            OnPropertyChanged();

            // Rensa animations-val
            _previewAnimation = null;
            AnimationEditor.SelectedAnimation = null;

            OnPropertyChanged(nameof(HasSelection));
            if (value is not null)
            {
                EditName     = value.Name;
                EditCategory = value.Category;
                AnimationEditor.SpriteToAdd = value.Id;
            }
            UpdatePreview();
        }
    }

    public SpriteAnimation? PreviewAnimation
    {
        get => _previewAnimation;
        set
        {
            _previewAnimation = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasSelection));
            StartAnimationPreview(value);
        }
    }

    private void StartAnimationPreview(SpriteAnimation? anim)
    {
        _animTimer?.Stop();
        _animTimer = null;
        _frameIndex = 0;

        if (anim is null || anim.Frames.Count == 0)
        {
            PreviewSprite = SelectedSprite;
            return;
        }

        // Sätt första frame direkt
        PreviewSprite = EditorSession.Current.Atlas.GetById(anim.Frames[0]);

        if (anim.Frames.Count == 1) return;

        _animTimer = new Avalonia.Threading.DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(anim.SpeedMs)
        };

        _animTimer.Tick += (_, _) =>
        {
            _frameIndex = (_frameIndex + 1) % anim.Frames.Count;
            PreviewSprite = EditorSession.Current.Atlas.GetById(anim.Frames[_frameIndex]);
        };

        _animTimer.Start();
    }

    public bool HasSelection => SelectedSprite is not null || _previewAnimation is not null;

    public string EditName
    {
        get => _editName;
        set { _editName = value; OnPropertyChanged(); }
    }

    public SpriteCategory EditCategory
    {
        get => _editCategory;
        set { _editCategory = value; OnPropertyChanged(); }
    }

    public Array Categories => Enum.GetValues(typeof(SpriteCategory));

    public ICommand ImportCommand    { get; }
    public ICommand ApplyEditCommand { get; }
    public ICommand SaveAtlasCommand { get; }
    public ICommand LoadAtlasCommand { get; }

    private readonly Window _window;

    public SpriteWorkspaceViewModel(Window window)
    {
        _window          = window;
        ImportCommand    = new RelayCommand(Import);
        ApplyEditCommand = new RelayCommand(ApplyEdit);
        SaveAtlasCommand = new RelayCommand(SaveAtlas);
        LoadAtlasCommand = new RelayCommand(LoadAtlas);

        AnimationEditor.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(AnimationEditorViewModel.SelectedAnimation))
            {
                _previewAnimation = null; // Tvinga reset
                PreviewAnimation = AnimationEditor.SelectedAnimation;
            }
        };

        AnimationEditor.AnimationUpdated += () =>
        {
            var anim = AnimationEditor.SelectedAnimation;
            if (anim is null) return;

            var idx = AnimationEditor.Animations.IndexOf(anim);
            if (idx < 0) return;

            AnimationEditor.Animations.RemoveAt(idx);
            AnimationEditor.Animations.Insert(idx, anim);
            AnimationEditor.SelectedAnimation = AnimationEditor.Animations[idx];
        };
    }

    private async void Import()
    {
        var dialog = new Views.Dialogs.ImportSpriteDialog();
        await dialog.ShowDialog(_window);

        var vm = (ImportSpriteDialogViewModel)dialog.DataContext!;
        if (!vm.Confirmed) return;

        foreach (var sprite in vm.ImportedSprites)
            EditorSession.Current.Atlas.Sprites.Add(sprite);

        EditorSession.Current.MarkDirty();
    }

    private void ApplyEdit()
    {
        if (SelectedSprite is null) return;

        SelectedSprite.Name     = EditName;
        SelectedSprite.Category = EditCategory;
        EditorSession.Current.MarkDirty();

        // Trigga refresh i listan
        var idx = Sprites.IndexOf(SelectedSprite);
        Sprites.RemoveAt(idx);
        Sprites.Insert(idx, SelectedSprite);
        SelectedSprite = Sprites[idx];
    }

    private void UpdatePreview()
    {
        // Stoppa gammal timer
        _animTimer?.Stop();
        _animTimer = null;
        _frameIndex = 0;

        PreviewSprite = SelectedSprite;
    }

    private async void SaveAtlas()
    {
        var folders = await _window.StorageProvider.OpenFolderPickerAsync(
            new FolderPickerOpenOptions
            {
                Title = "Välj mapp att spara atlas i"
            });

        if (folders.Count == 0) return;

        WPackSerializer.Save(folders[0].Path.LocalPath, EditorSession.Current.Atlas);
        EditorSession.Current.HasUnsavedChanges = false;
    }

    private async void LoadAtlas()
    {
        var folders = await _window.StorageProvider.OpenFolderPickerAsync(
            new FolderPickerOpenOptions { Title = "Välj atlasmapp att ladda" });

        if (folders.Count == 0) return;

        var atlas = WPackSerializer.Load(folders[0].Path.LocalPath);
        EditorSession.Current.Atlas = atlas;

        // Trigga refresh på både sprites och animationer
        OnPropertyChanged(nameof(Sprites));
        AnimationEditor.RefreshFromSession();
    }
}