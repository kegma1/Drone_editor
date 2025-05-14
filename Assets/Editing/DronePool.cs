using System;
using UnityEngine;
using UnityEngine.Pool;

public class DronePool : MonoBehaviour
{
    
    public GameObject dronePrefab; 
    private ObjectPool<GameObject> _pool;
    public EditorGraphic editorGraphic;

    public void createPool() {
        if (dronePrefab != null && _pool == null)
        {
            _pool = new ObjectPool<GameObject>(CreateDrone, onGetDrone, onDroneRelease, defaultCapacity: editorGraphic.MaxDones, maxSize: editorGraphic.MaxDones);
        }
    }

    private void onGetDrone(GameObject @object) {
        @object.SetActive(true);
    }

    private GameObject CreateDrone() {
        var drone = Instantiate(dronePrefab);
        drone.transform.SetParent(transform);
        drone.SetActive(false);

        return drone;
    }

    private void onDroneRelease(GameObject @object) {
        @object.SetActive(false);
    }

    public GameObject GetDrone() {
        return _pool.Get();
    }

    public void releaseDrone(GameObject @object) {
        _pool.Release(@object);
    }
}
