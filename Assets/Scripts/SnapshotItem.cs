using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnapshotItem : MonoBehaviour
{
    private SnapshotCamera snapshotCamera;
    public GameObject gameObjectToSnapshot;

    void Start()
    {
        snapshotCamera = SnapshotCamera.MakeSnapshotCamera(0);

        Texture2D snapshot = snapshotCamera.TakeObjectSnapshot(Instantiate(gameObjectToSnapshot));
        SnapshotCamera.SavePNG(snapshot);
    }
}