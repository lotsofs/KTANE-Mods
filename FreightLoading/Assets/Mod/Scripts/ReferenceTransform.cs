using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReferenceTransform : MonoBehaviour {
    [NonSerialized] public Vector3 Position;
    [NonSerialized] public Quaternion Rotation;
    [NonSerialized] public Vector3 Scale;

    /// <summary>
    /// Initialization
    /// </summary>
    void Awake() {
        Position = transform.localPosition;
        Rotation = transform.localRotation;
        Scale = transform.localScale;
    }
}
