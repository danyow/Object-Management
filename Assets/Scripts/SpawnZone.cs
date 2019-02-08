using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SpawnZone : MonoBehaviour
{

    // [SerializeField]
    // bool surfaceOnly;
    public abstract Vector3 SpawnPoint { get; }

    // private void OnDrawGizmos() {
    //     Gizmos.color = Color.cyan;
    //     Gizmos.matrix = transform.localToWorldMatrix;
    //     Gizmos.DrawWireSphere(Vector3.zero, 1f);
    // }

}
