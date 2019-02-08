using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : PersistableObject
{
    const int saveVersion = 1;
    public float CreationSpeed { get; set; }
    public float DestructionSpeed {get; set; }
    float creationProgress, destructionProgress;

    public ShapeFactory shapeFactory;
    List<Shape> shapes;
    public PersistentStorage storage;
    public KeyCode createKey  = KeyCode.C;
    public KeyCode newGameKey = KeyCode.N;
    public KeyCode saveKey    = KeyCode.S;
    public KeyCode loadKey    = KeyCode.L;
    public KeyCode destroyKey = KeyCode.X;

    private void Awake() {
        shapes = new List<Shape>();
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
        t.localPosition = Random.insideUnitSphere * 5f;
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
            Destroy(shapes[i].gameObject);
        }
        shapes.Clear();
    }

    public override void Save(GameDataWriter writer) {
        writer.Write(shapes.Count);
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
            Destroy(shapes[index].gameObject);
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

}
