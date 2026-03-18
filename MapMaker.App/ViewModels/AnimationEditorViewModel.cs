using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MapMaker.App.State;
using MapMaker.Core.Sprites;

namespace MapMaker.App.ViewModels;

public class AnimationEditorViewModel : INotifyPropertyChanged
{
    private SpriteAnimation? _selectedAnimation;
    private uint?            _selectedFrame;
    private string           _editName    = string.Empty;
    private int              _editSpeedMs = 150;
    private AnimationLoopMode _editLoopMode = AnimationLoopMode.Loop;

    public event PropertyChangedEventHandler? PropertyChanged;
    public event Action? AnimationUpdated;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    public ObservableCollection<SpriteAnimation> Animations =>
        EditorSession.Current.Atlas.Animations;

    public ObservableCollection<uint> CurrentFrames { get; } = new();

    public SpriteAnimation? SelectedAnimation
    {
        get => _selectedAnimation;
        set
        {
            _selectedAnimation = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasSelection));
            RefreshFrames();
            if (value is not null)
            {
                EditName     = value.Name;
                EditSpeedMs  = value.SpeedMs;
                EditLoopMode = value.LoopMode;
            }
        }
    }

    public uint? SelectedFrame
    {
        get => _selectedFrame;
        set { _selectedFrame = value; OnPropertyChanged(); }
    }

    public bool HasSelection => SelectedAnimation is not null;

    public string EditName
    {
        get => _editName;
        set { _editName = value; OnPropertyChanged(); }
    }

    public int EditSpeedMs
    {
        get => _editSpeedMs;
        set { _editSpeedMs = value; OnPropertyChanged(); }
    }

    public AnimationLoopMode EditLoopMode
    {
        get => _editLoopMode;
        set { _editLoopMode = value; OnPropertyChanged(); }
    }

    public Array LoopModes => Enum.GetValues(typeof(AnimationLoopMode));

    // Det valda sprite-ID:t i sprite-listan (för att lägga till som frame)
    public uint? SpriteToAdd { get; set; }

    public ICommand AddAnimationCommand    { get; }
    public ICommand RemoveAnimationCommand { get; }
    public ICommand AddFrameCommand        { get; }
    public ICommand RemoveFrameCommand     { get; }
    public ICommand MoveFrameUpCommand     { get; }
    public ICommand MoveFrameDownCommand   { get; }
    public ICommand ApplyEditCommand       { get; }

    public AnimationEditorViewModel()
    {
        AddAnimationCommand    = new RelayCommand(AddAnimation);
        RemoveAnimationCommand = new RelayCommand(RemoveAnimation);
        AddFrameCommand        = new RelayCommand(AddFrame);
        RemoveFrameCommand     = new RelayCommand(RemoveFrame);
        MoveFrameUpCommand     = new RelayCommand(MoveFrameUp);
        MoveFrameDownCommand   = new RelayCommand(MoveFrameDown);
        ApplyEditCommand       = new RelayCommand(ApplyEdit);
    }

    private void AddAnimation()
    {
        var anim = new SpriteAnimation
        {
            Id   = NextAnimationId(),
            Name = $"Animation {Animations.Count + 1}"
        };
        Animations.Add(anim);
        SelectedAnimation = anim;
        EditorSession.Current.MarkDirty();
    }

    private void RemoveAnimation()
    {
        if (SelectedAnimation is null) return;
        Animations.Remove(SelectedAnimation);
        SelectedAnimation = null;
        EditorSession.Current.MarkDirty();
    }

    private void AddFrame()
    {
        if (SelectedAnimation is null || SpriteToAdd is null) return;
        SelectedAnimation.Frames.Add(SpriteToAdd.Value);
        RefreshFrames();
        EditorSession.Current.MarkDirty();
        AnimationUpdated?.Invoke();
    }

    private void RemoveFrame()
    {
        if (SelectedAnimation is null || SelectedFrame is null) return;
        var idx = CurrentFrames.IndexOf(SelectedFrame.Value);
        if (idx < 0) return;
        SelectedAnimation.Frames.RemoveAt(idx);
        RefreshFrames();
        EditorSession.Current.MarkDirty();
        AnimationUpdated?.Invoke();
    }

    private void MoveFrameUp()
    {
        if (SelectedAnimation is null || SelectedFrame is null) return;
        var idx = CurrentFrames.IndexOf(SelectedFrame.Value);
        if (idx <= 0) return;
        (SelectedAnimation.Frames[idx], SelectedAnimation.Frames[idx - 1]) =
            (SelectedAnimation.Frames[idx - 1], SelectedAnimation.Frames[idx]);
        RefreshFrames();
        SelectedFrame = CurrentFrames[idx - 1];
        AnimationUpdated?.Invoke();
    }

    private void MoveFrameDown()
    {
        if (SelectedAnimation is null || SelectedFrame is null) return;
        var idx = CurrentFrames.IndexOf(SelectedFrame.Value);
        if (idx < 0 || idx >= CurrentFrames.Count - 1) return;
        (SelectedAnimation.Frames[idx], SelectedAnimation.Frames[idx + 1]) =
            (SelectedAnimation.Frames[idx + 1], SelectedAnimation.Frames[idx]);
        RefreshFrames();
        SelectedFrame = CurrentFrames[idx + 1];
        AnimationUpdated?.Invoke();
    }

    private void ApplyEdit()
    {
        if (SelectedAnimation is null) return;
        SelectedAnimation.Name     = EditName;
        SelectedAnimation.SpeedMs  = EditSpeedMs;
        SelectedAnimation.LoopMode = EditLoopMode;

        var idx = Animations.IndexOf(SelectedAnimation);
        Animations.RemoveAt(idx);
        Animations.Insert(idx, SelectedAnimation);
        SelectedAnimation = Animations[idx];
        EditorSession.Current.MarkDirty();
    }

    private void RefreshFrames()
    {
        CurrentFrames.Clear();
        if (SelectedAnimation is null) return;
        foreach (var f in SelectedAnimation.Frames)
            CurrentFrames.Add(f);
    }

    public void RefreshFromSession()
    {
        // Tvinga UI att läsa om från den nya atlansen
        OnPropertyChanged(nameof(Animations));
        SelectedAnimation = null;
        CurrentFrames.Clear();
    }

    private uint NextAnimationId()
    {
        if (Animations.Count == 0) return 1;
        return Animations.Max(a => a.Id) + 1;
    }
}