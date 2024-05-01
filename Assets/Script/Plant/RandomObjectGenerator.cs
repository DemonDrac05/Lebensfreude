using System.Collections.Generic;
using UnityEngine;

public class RandomObjectGenerator : MonoBehaviour
{
    [System.Serializable]
    public class ObjectProbability
    {
        public GameObject objectPrefab;
        public float probability;
    }

    public Vector2 tileSize = new Vector2(16f, 16f);

    public List<ObjectProbability> objectsWithProbability;
    public List<Vector3> usedPosition = new List<Vector3>();
    void Start()
    {
        for (int i = 0; i < 200; i++)
        {
            GameObject spawnedObject = RandomizeObject();

            Vector3 randomPosition = GetRandomPositionInTile();

            Instantiate(spawnedObject, randomPosition, Quaternion.identity);

            usedPosition.Add(randomPosition);
        }
    }

    Vector3 GetRandomPositionInTile()
    {
        Vector3 randomPosition;
        do
        {
            int randomX = Random.Range(-10, 10);
            int randomY = Random.Range(-10, 10);

            int boolX = Random.Range(0, 2);
            int boolY = Random.Range(0, 2);

            float middleTileX = (boolX == 0) ? -0.5f : 0.5f;
            float middleTileY = (boolY == 0) ? -0.5f : 0.5f;

            randomPosition = new(randomX + middleTileX, randomY + middleTileY, 0f);
        } while(usedPosition.Contains(randomPosition));
        return randomPosition;
    }

    GameObject RandomizeObject()
    {
        float total = 0;

        foreach (var objectWithProb in objectsWithProbability)
        {
            total += objectWithProb.probability;
        }

        float randomPoint = Random.value * total;

        for (int i = 0; i < objectsWithProbability.Count; i++)
        {
            if (randomPoint < objectsWithProbability[i].probability)
            {
                return objectsWithProbability[i].objectPrefab;
            }
            else
            {
                randomPoint -= objectsWithProbability[i].probability;
            }
        }

        return null;
    }
}
