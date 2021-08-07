using System;
using System.IO;
using System.Collections.Generic;

using Newtonsoft.Json;

using UnityEngine;
using UnityEngine.SceneManagement;

using Articy.Unity;
using Articy.Codename_Mysterybabylon;
using Sirenix.OdinInspector;

[Serializable]
public class SaveData
{
    public string CurrentSceneName;
    public Dictionary<string, float> PlayerPosition;
    public List<PlayableCharacterData> PlayerData;
}

public class SaveManager : MonoBehaviour, IInitializable
{
    public static SaveManager Instance;

    private List<SaveData> _saveSlots = new List<SaveData>();
    public List<SaveData> SaveSlots { get => _saveSlots; }

    public void Init()
    {
        Instance = this;
    }

    [Button("Save")]
    public void Save(int saveSlotIndex = 0)
    {
        var saveData = new SaveData();

        var players = EntityManager.Instance.PlayableCharacters;
        var playersData = new List<PlayableCharacterData>();
        
        foreach(var p in players)
        {
            var playableCharData = PlayableCharacterData.Populate(p);
            playersData.Add(playableCharData);
        }

        var currentScene = SceneManager.GetActiveScene();
        var currentSceneName = currentScene.name;

        var player = GameObject.FindGameObjectWithTag("Main Player");
        var playerPosition = new Dictionary<string, float>
        {
            { "X", player.transform.position.x },
            { "Y", player.transform.position.y },
            { "Z", player.transform.position.z },
        };

        saveData.CurrentSceneName = currentSceneName;
        saveData.PlayerPosition = playerPosition;
        saveData.PlayerData = playersData;

        _saveSlots.Insert(saveSlotIndex, saveData);

        Write(saveData, saveSlotIndex);
    }

    private void Write(SaveData saveData, int saveSlotIndex)
    {
        string jsonPath = $"{Application.dataPath}/SerializedData/Saves";

        if (!Directory.Exists(jsonPath))
            Directory.CreateDirectory(jsonPath);

        string json = JsonConvert.SerializeObject(saveData, Formatting.Indented);

        File.WriteAllText($"{jsonPath}/SaveSlot_{saveSlotIndex}.json", json);
    }
}
