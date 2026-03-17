using System.Collections.ObjectModel;
using MapMaker.App.State;
using MapMaker.Core.Sprites;

namespace MapMaker.App.ViewModels;

public class SpritePickerViewModel
{
    public ObservableCollection<SpriteEntry> Sprites => 
        EditorSession.Current.Atlas.Sprites;

    public SpriteEntry? SelectedSprite { get; set; }
}