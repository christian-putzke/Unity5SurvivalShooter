using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace CompleteProject
{
	/**
	 * The save game manager class handels the loading and saving of game data
	 */
    public class SaveGameManager : MonoBehaviour
    {
		/**
		 * Checks the key inputs and loads / saves the game on matching keys 
		 */
		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.F5))
			{
				SaveGameManager.Save("MySaveGame");
			}

			if (Input.GetKeyDown(KeyCode.F6))
			{
				SaveGameManager.Load("MySaveGame");
			}
		}


		/**
		 * The load method loads the save game with the given name
		 */
		public static void Load(string saveGameName)
		{
			if (File.Exists(Application.persistentDataPath + "/" + saveGameName + ".dat"))
			{
				var bf = new BinaryFormatter();
				var file = File.Open(Application.persistentDataPath + "/" + saveGameName + ".dat", FileMode.Open);
				var saveGame = (SaveGame) bf.Deserialize(file);
				file.Close();


				var player			= GameObject.FindGameObjectWithTag("Player");
				var playerHealth	= player.GetComponent<PlayerHealth>();
				var playerTransform	= player.GetComponent<Transform>();

				playerHealth.currentHealth		= saveGame.player.health;
				playerHealth.healthSlider.value	= saveGame.player.health;
				playerTransform.position		= new Vector3(saveGame.player.positionX, saveGame.player.positionY, saveGame.player.positionZ);
				playerTransform.rotation		= new Quaternion(saveGame.player.rotationX, saveGame.player.rotationY, saveGame.player.rotationZ, saveGame.player.rotationW);


				ScoreManager.score = saveGame.game.score;
				Camera.main.transform.position = new Vector3(saveGame.game.cameraPositionX, saveGame.game.cameraPositionY, saveGame.game.cameraPositionZ);


				// load spawn timer values?
				var enemies = GameObject.FindGameObjectsWithTag("Enemy");
				for (var index = 0; index < enemies.Length; index ++)
				{
					GameObject.Destroy(enemies[index].gameObject);
				}

				for (var index = 0; index < saveGame.enemies.Length; index ++)
				{
					var enemySpawnPosition	= new Vector3(saveGame.enemies[index].positionX, saveGame.enemies[index].positionY, saveGame.enemies[index].positionZ);
					var enemySpawnRotation	= new Quaternion(saveGame.enemies[index].rotationX, saveGame.enemies[index].rotationY, saveGame.enemies[index].rotationZ, saveGame.enemies[index].rotationW);

					EnemyManager.instances[saveGame.enemies[index].type].Spawn(saveGame.enemies[index].health, enemySpawnPosition, enemySpawnRotation);
				}

				Debug.Log("Loaded!");
			}
		}


		/**
		 * Saves all game relevant data to a save game named like the given string
		 */
		public static void Save(string saveGameName)
		{
			var bf = new BinaryFormatter();
			var file = File.Create(Application.persistentDataPath + "/" + saveGameName + ".dat");
			var saveGame = new SaveGame();


			var player			= GameObject.FindGameObjectWithTag("Player");
			var playerHealth	= player.GetComponent<PlayerHealth>();
			var playerTransform	= player.GetComponent<Transform>();

			saveGame.player.health		= playerHealth.currentHealth;
			saveGame.player.positionX	= playerTransform.position.x;
			saveGame.player.positionY	= playerTransform.position.y;
			saveGame.player.positionZ	= playerTransform.position.z;
			saveGame.player.rotationX	= playerTransform.rotation.x;
			saveGame.player.rotationY	= playerTransform.rotation.y;
			saveGame.player.rotationZ	= playerTransform.rotation.z;
			saveGame.player.rotationW	= playerTransform.rotation.w;


			saveGame.game.score				= ScoreManager.score;
			saveGame.game.cameraPositionX	= Camera.main.transform.position.x;
			saveGame.game.cameraPositionY	= Camera.main.transform.position.y;
			saveGame.game.cameraPositionZ	= Camera.main.transform.position.z;


			var enemies = GameObject.FindGameObjectsWithTag("Enemy");
			saveGame.enemies = new EnemyData[enemies.Length];
			for (var index = 0; index < enemies.Length; index ++)
			{
				var enemyHealth		= enemies[index].gameObject.GetComponent<EnemyHealth>();
				var enemyType		= enemies[index].gameObject.GetComponent<EnemyType>();
				var enemyTransform	= enemies[index].gameObject.transform;

				saveGame.enemies[index]				= new EnemyData();
				saveGame.enemies[index].health		= enemyHealth.currentHealth;
				saveGame.enemies[index].positionX	= enemyTransform.position.x;
				saveGame.enemies[index].positionY	= enemyTransform.position.y;
				saveGame.enemies[index].positionZ	= enemyTransform.position.z;
				saveGame.enemies[index].rotationX	= enemyTransform.rotation.x;
				saveGame.enemies[index].rotationY	= enemyTransform.rotation.y;
				saveGame.enemies[index].rotationZ	= enemyTransform.rotation.z;
				saveGame.enemies[index].rotationW	= enemyTransform.rotation.w;
				saveGame.enemies[index].type		= enemyType.type;
			}


			bf.Serialize(file, saveGame);
			file.Close();

			Debug.Log("Saved!");
		}
    }


	/**
	 * Stores the character data
	 */
	[Serializable]
	public class SaveGame
	{
		public CharacterData	player	= new CharacterData();
		public GameData			game	= new GameData();
		public EnemyData[]		enemies;
	}


	/**
	 * Stores the character data
	 */
	[Serializable]
	public class CharacterData
	{
		// current health
		public int health;

		// current position
		public float positionX;
		public float positionY;
		public float positionZ;

		// current rotation
		public float rotationX;
		public float rotationY;
		public float rotationZ;
		public float rotationW;
	}

	/**
	 * Stores the enemy data
	 */
	[Serializable]
	public class EnemyData : CharacterData
	{
		// current rotation
		public string type;
	}


	/**
	 * Stores the game data
	 */
	[Serializable]
	public class GameData
	{
		public int score;

		public float cameraPositionX;
		public float cameraPositionY;
		public float cameraPositionZ;
	}
}