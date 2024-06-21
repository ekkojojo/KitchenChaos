using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatesCounterVisual : MonoBehaviour
{
    [SerializeField] PlatesCounter PlatesCounter;
    [SerializeField] Transform spawnPosition;
    [SerializeField] Transform platePrefab;

    List<GameObject> plateGameObjectList;
    private void Start()
    {
        plateGameObjectList = new List<GameObject>();

        PlatesCounter.OnPlateSpawn += PlatesCounter_OnPlateSpawn;
        PlatesCounter.OnPlateRemove += PlatesCounter_OnPlateRemove;
    }

    private void PlatesCounter_OnPlateRemove(object sender, System.EventArgs e)
    {
        GameObject plateGameObject = plateGameObjectList[plateGameObjectList.Count - 1];
        plateGameObjectList.Remove(plateGameObject);
        Destroy(plateGameObject);
    }

    private void PlatesCounter_OnPlateSpawn(object sender, System.EventArgs e)
    {
        Transform plateTransform = Instantiate(platePrefab, spawnPosition);

        float offsetY = .1f;
        plateTransform.localPosition = new Vector3(0, offsetY * plateGameObjectList.Count, 0);

        plateGameObjectList.Add(plateTransform.gameObject);
    }
}
