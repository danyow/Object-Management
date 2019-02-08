using System.IO;
using UnityEngine;

public class PersistentStorage : MonoBehaviour
{
    string savePath;
    private void Awake() {
        savePath = Path.Combine(Application.persistentDataPath, "saveFile");
    }

    public void Save(PersistableObject o, int version) {
        using (
            var writer = new BinaryWriter(File.Open(savePath, FileMode.Create))
        ) {
            /**
                这种直接创建的 会有警告 因为这里没有被实例化到一个物体上
             */
            writer.Write(-version);
            o.Save(new GameDataWriter(writer));
            // o.Save(new GameObject().AddComponent<GameDataWriter>().Init(writer));
        }
    }

    public void Load(PersistableObject o) {
        using (
            var reader = new BinaryReader(File.Open(savePath, FileMode.Open))
        ) {
            o.Load(new GameDataReader(reader, -reader.ReadInt32()));
            // o.Load(new GameObject().AddComponent<GameDataReader>().Init(reader));
        }
    }
}
