using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shape : PersistableObject
{
    private int shapeId = int.MinValue;
    public int ShapeId
    {
        get { return shapeId;}
        set { 
            if (shapeId == int.MinValue && value != int.MinValue) {
                shapeId = value;
            } else {
                Debug.Log("Can't Change ShapeId");
            }
        }
    }
    private ShapeFactory originFactory;
    public ShapeFactory OriginFactory
    {
        get { return originFactory;}
        set { 
            if (originFactory == null) {
                originFactory = value;
            } else {
                Debug.Log("Can't Change OriginFactory");
            }
        }
    }    

    public int MaterialId { get; private set; }

    public void SetMaterial(Material material, int materialId) {
        for (int i = 0; i < meshRenderers.Length; i++) {
            meshRenderers[i].material = material;
        }
        MaterialId = materialId;
    }

    static int colorPropertyId = Shader.PropertyToID("_Color");
    static MaterialPropertyBlock sharedPropertyBlock;

    Color[] colors;

    public int ColorCount {
        get {
            return colors.Length;
        }
    }

    public void SetColor(Color color) {
        // meshRenderer.material.color = color;
        if (sharedPropertyBlock == null) {
            sharedPropertyBlock = new MaterialPropertyBlock();
        }
        sharedPropertyBlock.SetColor(colorPropertyId, color);
        for (int i = 0; i < meshRenderers.Length; i++) {
            colors[i] = color;
            meshRenderers[i].SetPropertyBlock(sharedPropertyBlock);
        }
    }

    public void SetColor(Color color, int index) {
        if (sharedPropertyBlock == null) {
            sharedPropertyBlock = new MaterialPropertyBlock();
        }
        sharedPropertyBlock.SetColor(colorPropertyId, color);
        colors[index] = color;
        meshRenderers[index].SetPropertyBlock(sharedPropertyBlock);
    }

    public override void Save(GameDataWriter writer) {
        base.Save(writer);
        writer.Write(colors.Length);
        for (int i = 0; i < colors.Length; i++) {
            writer.Write(colors[i]);
        }
        writer.Write(AngularVelocity);
        writer.Write(Velocity);
    }

    public override void Load(GameDataReader reader) {
        base.Load(reader);
        if (reader.Version >= 5) {
            // for (int i = 0; i < colors.Length; i++) {
            //     SetColor(reader.ReadColor(), i);
            // }
            LoadColors(reader);
        } else {
            SetColor(reader.Version > 0 ? reader.ReadColor() : Color.white);
        }
        AngularVelocity = reader.Version > 4 ? reader.ReadVector3() : Vector3.zero;
        Velocity        = reader.Version > 4 ? reader.ReadVector3() : Vector3.zero;
    }

    void LoadColors(GameDataReader reader) {
        int count = reader.ReadInt();
        int max = count <= colors.Length ? count : colors.Length;
        int i = 0;
        for ( ; i < max; i++) {
            SetColor(reader.ReadColor(), i);
        }
        if (count > max) {
            for ( ; i < count; i++) {
                reader.ReadColor();
            } 
        } else if (count < max) {
            for ( ; i < max; i++) {
                SetColor(Color.white, i);
            }
        }
    }

    public Vector3 AngularVelocity { get; set; }
    public void GameUpdate() {
        // 这里原本是FixedUpdate方法  自己调用的 但后面优化到Game里面的FixedUpdate手动调用 因为Unity在调用FixedUpdate的时候还做了些自己要做的事情
        transform.Rotate(AngularVelocity * Time.deltaTime);
        transform.localPosition += Velocity * Time.deltaTime;
    }

    public Vector3 Velocity { get; set; }


    [SerializeField]
    MeshRenderer[] meshRenderers;

    private void Awake() {
        colors = new Color[meshRenderers.Length];
    }

    public void Recycle() {
        OriginFactory.Reclaim(this);   
    }
}
