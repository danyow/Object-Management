using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    public Transform prefab;

    List<Transform> objects;

    public KeyCode createKey = KeyCode.C;
    public KeyCode newGameKey = KeyCode.N;


    private void Awake() {
        objects = new List<Transform>();
    }

    private void Update() {
        if (Input.GetKey(createKey)) {
            CreateObject();
        } else if (Input.GetKey(newGameKey)) {
            BeginNewGame();
        }
    }

    void CreateObject() {
        Transform t = Instantiate(prefab);
        t.localPosition = Random.insideUnitSphere * 5f;
        t.localRotation = Random.rotation;
        t.localScale = Vector3.one * Random.Range(0.1f, 1f);
        objects.Add(t);
    }

    void BeginNewGame() {
        for (int i = 0; i < objects.Count; i++)
        {
            Destroy(objects[i].gameObject);
        }
        objects.Clear();
    }

}
