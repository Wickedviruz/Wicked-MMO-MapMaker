using System;
using System.Collections.Generic;
using MapMaker.Core.Items;
using MapMaker.Core.Map;
using MapMaker.Core.Maps;
using MapMaker.Core.Sprites;

namespace MapMaker.App.State;

public class EditorSession
{
    public static EditorSession Current { get; } = new();

    // Karta
    public RMap? CurrentMap { get; set; }
    public SpawnData SpawnData { get; set; } = new();

    // Sprites
    public SpriteAtlas Atlas { get; set; } = new();

    // Items
    public List<ItemDefinition> Items { get; set; } = new();

    // Projekt-sökväg — null om ej sparat än
    public string? ProjectPath { get; set; }

    public bool HasUnsavedChanges { get; set; }

    public bool IsProjectOpen => CurrentMap is not null;

    public void Reset()
    {
        CurrentMap       = null;
        SpawnData        = new SpawnData();
        Atlas            = new SpriteAtlas();
        Items            = new();
        ProjectPath      = null;
        HasUnsavedChanges = false;
    }

    public void MarkDirty() => HasUnsavedChanges = true;
}