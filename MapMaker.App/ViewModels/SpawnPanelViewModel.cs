using System.Collections.ObjectModel;
using System.Windows.Input;
using MapMaker.Core.Maps;

namespace MapMaker.App.ViewModels;

public class SpawnPanelViewModel
{
    public ObservableCollection<SpawnArea> Areas { get; } = new();
    public SpawnArea? SelectedArea { get; set; }

    public ObservableCollection<MonsterSpawn> Monsters { get; } = new();
    public MonsterSpawn? SelectedMonster { get; set; }

    public ICommand AddAreaCommand      { get; }
    public ICommand RemoveAreaCommand   { get; }
    public ICommand AddMonsterCommand   { get; }
    public ICommand RemoveMonsterCommand{ get; }

    private SpawnData _data = new();

    public SpawnPanelViewModel()
    {
        AddAreaCommand       = new RelayCommand(AddArea);
        RemoveAreaCommand    = new RelayCommand(RemoveArea);
        AddMonsterCommand    = new RelayCommand(AddMonster);
        RemoveMonsterCommand = new RelayCommand(RemoveMonster);
    }

    public void Initialize(SpawnData data)
    {
        _data = data;
        Areas.Clear();
        foreach (var area in data.Areas)
            Areas.Add(area);
    }

    public SpawnData GetData() => _data;

    private void AddArea()
    {
        var area = new SpawnArea
        {
            CenterX = 0,
            CenterY = 0,
            CenterZ = 0,
            Radius  = 5
        };
        _data.Areas.Add(area);
        Areas.Add(area);
        SelectedArea = area;
        RefreshMonsters();
    }

    private void RemoveArea()
    {
        if (SelectedArea is null) return;
        _data.Areas.Remove(SelectedArea);
        Areas.Remove(SelectedArea);
        SelectedArea = null;
        RefreshMonsters();
    }

    private void AddMonster()
    {
        if (SelectedArea is null) return;
        var monster = new MonsterSpawn
        {
            Name      = "Monster",
            X         = 0,
            Y         = 0,
            Z         = 0,
            SpawnTime = 60
        };
        SelectedArea.Monsters.Add(monster);
        Monsters.Add(monster);
    }

    private void RemoveMonster()
    {
        if (SelectedArea is null || SelectedMonster is null) return;
        SelectedArea.Monsters.Remove(SelectedMonster);
        Monsters.Remove(SelectedMonster);
        SelectedMonster = null;
    }

    public void SelectArea(SpawnArea? area)
    {
        SelectedArea = area;
        RefreshMonsters();
    }

    private void RefreshMonsters()
    {
        Monsters.Clear();
        if (SelectedArea is null) return;
        foreach (var m in SelectedArea.Monsters)
            Monsters.Add(m);
    }
}