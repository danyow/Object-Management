using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Game : PersistableObject
{
    public static Game Instance { get; private set; }

    const int saveVersion = 2;
    public float CreationSpeed { get; set; }
    public float DestructionSpeed {get; set; }
    [SerializeField]
    ShapeFactory shapeFactory;
    public PersistentStorage storage;
    public SpawnZone spawnZoneOfLevel {get; set; }
    public int levelCount;
    int loadedLevelBuildIndex;
    float creationProgress, destructionProgress;
    List<Shape> shapes;
    public KeyCode createKey  = KeyCode.C;
    public KeyCode newGameKey = KeyCode.N;
    public KeyCode saveKey    = KeyCode.S;
    public KeyCode loadKey    = KeyCode.L;
    public KeyCode destroyKey = KeyCode.X;

    private void OnEnable() {
        Instance = this;    
    }

    private void Start() {
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
            StartCoroutine(LoadLevel(1 ));
        }
    }

    private void Update() {
        if (Input.GetKey(createKey)) {
            CreateShape();
        } else if (Input.GetKey(newGameKey)) {
            BeginNewGame();
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
        Shape instance = shapeFactory.GetRandom();
        Transform t = instance.transform;
        t.localPosition = spawnZoneOfLevel.SpawnPoint;
        t.localRotation = Random.rotation;
        t.localScale = Vector3.one * Random.Range(0.1f, 1f);
        instance.SetColor(Random.ColorHSV(
            hueMin: 0f, 
            hueMax: 1f,
            saturationMin: 0.5f,
            saturationMax: 1f,
            valueMin: 0.25f,
            valueMax: 1f,
            alphaMin: 1f,
            alphaMax: 1f
        ));
        shapes.Add(instance);
    }

    void BeginNewGame() {
        for (int i = 0; i < shapes.Count; i++)
        {
            shapeFactory.Reclaim(shapes[i]);
        }
        shapes.Clear();
    }

    public override void Save(GameDataWriter writer) {
        writer.Write(shapes.Count);
        writer.Write(loadedLevelBuildIndex);
        for (int i = 0; i < shapes.Count; i++) {
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
        int count = version < 0 ? -version: reader.ReadInt();
        StartCoroutine(LoadLevel(version < 2 ? 1 : reader.ReadInt()));
        for (int i = 0; i < count; i++)
        {
            int shapeId = version > 0 ? reader.ReadInt() : 0;
            int materialId = version > 0 ? reader.ReadInt() : 0;
            Shape instance = shapeFactory.Get(shapeId, materialId);
            instance.Load(reader);
            shapes.Add(instance);
        }
    }

    public void DestroyShape() {
        if (shapes.Count > 0) {
            int index = Random.Range(0, shapes.Count);
            shapeFactory.Reclaim(shapes[index]);
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
