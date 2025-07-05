using UnityEngine;
using UnityEngine.Serialization;

namespace VRFPSKit
{
    public class BirdSpawner : MonoBehaviour
    {
        public Transform[] spawnPoints;
        public float minSpawnInterval;
        public float maxSpawnInterval;
        public GameObject birdPrefab;
    
        // Start is called before the first frame update
        void Start() => TrySpawnBird();

        public void TrySpawnBird()
        {
            //Invoke itself after random time
            Invoke(nameof(TrySpawnBird), Random.Range(minSpawnInterval, maxSpawnInterval));
            
            if (!enabled) return;
            if (spawnPoints.Length == 0) return;
            
            //Randomize a spawn point
            Transform spawn = spawnPoints[Random.Range(0, spawnPoints.Length)];
            GameObject skeet = Instantiate(birdPrefab, spawn.position, spawn.rotation);
        }
    }
}
