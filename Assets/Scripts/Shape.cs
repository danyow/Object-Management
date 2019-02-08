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

    public int MaterialId { get; private set; }

    public void SetMaterial(Material material, int materialId) {
        GetComponent<MeshRenderer>().material = material;
        MaterialId = materialId;
    }

}
