using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Game : PersistableObject
{
    const int saveVersion = 6;
    public float CreationSpeed { get; set; }
    public float DestructionSpeed {get; set; }
    [SerializeField]
    Slider creationSpeedSlider, destructionSpeedSlider;
    [SerializeField]
    // 是否重置种子在加载的时候
    bool reseedOnLoad;
    [SerializeField]
    ShapeFactory[] shapeFactories;
    public PersistentStorage storage;
    // public SpawnZone spawnZoneOfLevel {get; set; }
    public int levelCount;
    int loadedLevelBuildIndex;
    float creationProgress, destructionProgress;
    List<Shape> shapes;
    Random.State mainRandomState;
    public KeyCode createKey  = KeyCode.C;
    public KeyCode newGameKey = KeyCode.N;
    public KeyCode saveKey    = KeyCode.S;
    public KeyCode loadKey    = KeyCode.L;
    public KeyCode destroyKey = KeyCode.X;

    private void OnEnable() {
        if (shapeFactories[0].FactoryId != 0)
        {
            for (int i = 0; i < shapeFactories.Length; i++) {
                shapeFactories[i].FactoryId = i;
            }
        }
    }

    private void Start() {
        mainRandomState = Random.state;
        shapes = new List<Shape>();
        if (Application.isEditor) {
            for (int i = 0; i < SceneManager.sceneCount; i++) {
                Scene loadedScene = SceneManager.GetSceneAt(i);
                if (loadedScene.name.Contains("Level")) {
                    SceneManager.SetActiveScene(loadedScene);
                    loadedLevelBuildIndex = loadedScene.buildIndex;
                    return;
                }
            }
            BeginNewGame();
            StartCoroutine(LoadLevel(1));
        }
    }

    private void Update() {
        if (Input.GetKey(createKey)) {
            CreateShape();
        } else if (Input.GetKey(newGameKey)) {
            BeginNewGame();
            StartCoroutine(LoadLevel(loadedLevelBuildIndex));
        } else if (Input.GetKeyDown(saveKey)) {
            storage.Save(this, saveVersion);
        } else if (Input.GetKeyDown(loadKey)) {
            BeginNewGame();
            storage.Load(this);
        } else if (Input.GetKeyDown(destroyKey)) {
            DestroyShape();
        } else {
            for (int i = 1; i <= levelCount; i++) {
                if (Input.GetKeyDown(KeyCode.Alpha0 + i)) {
                    BeginNewGame();
                    StartCoroutine(LoadLevel(i));
                    return;
                }
            }
        }
    }

    private void FixedUpdate() {
        for (int i = 0; i < shapes.Count; i++) {
            shapes[i].GameUpdate();
        }
        
        creationProgress += Time.deltaTime * CreationSpeed;
        while (creationProgress >= 1f) {
            creationProgress -= 1f;
            CreateShape();
        }

        destructionProgress += Time.deltaTime * DestructionSpeed;
        while (destructionProgress >= 1f) {
            destructionProgress -= 1f;
            DestroyShape();
        }
    }


    void CreateShape() {
        shapes.Add(GameLevel.Current.SpawnShape());
    }

    void BeginNewGame() {
        Random.state = mainRandomState;
        int seed = Random.Range(0, int.MaxValue) ^ (int)Time.unscaledTime;
        mainRandomState = Random.state;
        Random.InitState(seed);
        creationSpeedSlider.value    = CreationSpeed    = 0;
        destructionSpeedSlider.value = DestructionSpeed = 0;
        for (int i = 0; i < shapes.Count; i++) {
            shapes[i].Recycle();
        }
        shapes.Clear();
    }

    public override void Save(GameDataWriter writer) {
        writer.Write(shapes.Count);
        writer.Write(Random.state);
        writer.Write(CreationSpeed);
        writer.Write(creationProgress);
        writer.Write(DestructionSpeed);
        writer.Write(destructionProgress);
        writer.Write(loadedLevelBuildIndex);
        GameLevel.Current.Save(writer);
        for (int i = 0; i < shapes.Count; i++) {
            writer.Write(shapes[i].OriginFactory.FactoryId);
            writer.Write(shapes[i].ShapeId);
            writer.Write(shapes[i].MaterialId);
            shapes[i].Save(writer);
        }
    }

    public override void Load(GameDataReader reader) {
        int version = reader.Version;
        if (version > saveVersion)
        {
            Debug.LogError("不支持超前版本" + version);
            return;
        }
        StartCoroutine(LoadGame(reader));
    }

    IEnumerator LoadGame(GameDataReader reader) {
        int version = reader.Version;
        int count = version < 0 ? -version: reader.ReadInt();
        if (version >= 3) {
            Random.State state = reader.ReadRandomState();
            if (!reseedOnLoad) {
                Random.state = state;
            }
            creationSpeedSlider.value    = CreationSpeed    = reader.ReadFloat();
            creationProgress                                = reader.ReadFloat();
            destructionSpeedSlider.value = DestructionSpeed = reader.ReadFloat();
            destructionProgress                             = reader.ReadFloat();
        }
        yield return LoadLevel(version < 2 ? 1 : reader.ReadInt());
        if (version >= 3) {
            GameLevel.Current.Load(reader);
        }
        for (int i = 0; i < count; i++)
        {
            int factoryId = version >= 5 ? reader.ReadInt() : 0;
            int shapeId = version > 0 ? reader.ReadInt() : 0;
            int materialId = version > 0 ? reader.ReadInt() : 0;
            Shape instance = shapeFactories[factoryId].Get(shapeId, materialId);
            instance.Load(reader);
            shapes.Add(instance);
        }
    }

    public void DestroyShape() {
        if (shapes.Count > 0) {
            int index = Random.Range(0, shapes.Count);
            shapes[index].Recycle();
            /**
            这里删除有一个细节 就是删除的时候把当前值移到最后面再删除 
            但是如果是一个一个移动 保证顺序的话 就会耗费很多时间 
            如果直接和最后一个交换的话 就会打乱顺序 但符合我们的需求
             */
            int lastIndex = shapes.Count - 1;
            shapes[index] = shapes[lastIndex];
            shapes.RemoveAt(lastIndex);
        }
    }

    IEnumerator LoadLevel(int levelBuildIndex) {
        enabled = false;
        if (loadedLevelBuildIndex > 0) {
            yield return SceneManager.UnloadSceneAsync(loadedLevelBuildIndex);
        }
        yield return SceneManager.LoadSceneAsync(levelBuildIndex, LoadSceneMode.Additive);
        SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(levelBuildIndex));
        loadedLevelBuildIndex = levelBuildIndex;
        enabled = true;
    }

}
