using UnityEngine;
using System.Collections.Generic;

namespace CompleteProject
{
    public class EnemyManager : MonoBehaviour
    {
		public static Dictionary<string, EnemyManager> instances = new Dictionary<string, EnemyManager>(); // A static dictionary which contains every used enemy manager instance referenced by the enemy name
        public PlayerHealth playerHealth;       // Reference to the player's heatlh.
        public GameObject enemy;                // The enemy prefab to be spawned.
        public float spawnTime = 3f;            // How long between each spawn.
        public Transform[] spawnPoints;         // An array of the spawn points this enemy can spawn from.


        void Start ()
        {
            // Call the Spawn function after a delay of the spawnTime and then continue to call after the same amount of time.
            InvokeRepeating ("Spawn", spawnTime, spawnTime);

			if (!EnemyManager.instances.ContainsKey(enemy.name))
			{
				EnemyManager.instances.Add(enemy.name, this);
			}
        }


        void Spawn ()
        {
            // If the player has no health left...
            if(playerHealth.currentHealth <= 0f)
            {
                // ... exit the function.
                return;
            }

            // Find a random index between zero and one less than the number of spawn points.
            int spawnPointIndex = Random.Range (0, spawnPoints.Length);

			this.Spawn(spawnPoints[spawnPointIndex].position, spawnPoints[spawnPointIndex].rotation);
        }

		/**
		 * Spawns an enemy with the given health at the given position and rotation
		 */
		public void Spawn(Vector3 position, Quaternion rotation, int health = 0)
		{
			var enemyGameObject		= (GameObject) Instantiate (enemy, position, rotation);
			enemyGameObject.name	= enemy.name;

			if (health > 0)
			{
				enemyGameObject.GetComponent<EnemyHealth>().currentHealth = health;
			}
		}
    }
}