using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerData : MonoBehaviour
{
    public List<GameObject> spawnedObjects;
    public float spawnerTimer;
    public float spawnerCount;

    public SpawnerData(List<GameObject> _spawnedObjects, float _spawnerTimer, float _spawnerCount)
    {
        spawnedObjects = _spawnedObjects;
        spawnerTimer = _spawnerTimer;
        spawnerCount = _spawnerCount;
    }
}
