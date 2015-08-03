using UnityEngine;
using System.Collections.Generic;
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
		 * The save game name
		 */
		public string saveGameName = "Khorinis";


		/**
		 * The players health class
		 */
		public static PlayerHealth playerHealth;


		/**
		 * The players transform
		 */
		public static Transform playerTransform;


		/**
		 * Loads and caches needed components on start up
		 */
		private void Start()
		{
			var player						= GameObject.FindGameObjectWithTag("Player");
			SaveGameManager.playerHealth	= player.GetComponent<PlayerHealth>();
			SaveGameManager.playerTransform	= player.GetComponent<Transform>();
		}


		/**
		 * Checks the key inputs and loads / saves the game on matching keys
		 */
		private void Update()
		{
			if (SaveGameManager.playerHealth.currentHealth > 0)
			{
				if (Input.GetButtonDown("Save"))
				{
					SaveGameManager.Save(this.saveGameName);
				}
				else if (Input.GetButtonDown("Load"))
				{
					SaveGameManager.Load(this.saveGameName);
				}
			}
		}


		/**
		 * The load method loads the save game with the given name and assigns the data to the game objects
		 */
		public static void Load(string saveGameName)
		{
			if (File.Exists(Application.persistentDataPath + "/" + saveGameName + ".savegame"))
			{
				var binaryFormatter	= new BinaryFormatter();
				var saveGameFile	= File.Open(Application.persistentDataPath + "/" + saveGameName + ".savegame", FileMode.Open);
				var saveGame		= (SaveGame) binaryFormatter.Deserialize(saveGameFile);
				saveGameFile.Close();

				// Be sure that the save game was successfully desirialized
				if (saveGame != null)
				{
					SaveGameManager.AssignPlayerData(saveGame);
					SaveGameManager.AssignGameData(saveGame);
					SaveGameManager.AssignEnemyData(saveGame);
					
					Debug.Log("The savegame " + saveGameName + " was successfully loaded!");
				}
			}
		}


		/**
		 * Assigns the player data (health, position, rotation) by the given save game
		 */
		private static void AssignPlayerData(SaveGame saveGame)
		{
			var playerPosition = new Vector3(
				saveGame.player.positionX,
				saveGame.player.positionY,
				saveGame.player.positionZ
			);

			var playerRotation = new Quaternion(
				saveGame.player.rotationX,
				saveGame.player.rotationY,
				saveGame.player.rotationZ,
				saveGame.player.rotationW
			);

			SaveGameManager.playerHealth.currentHealth		= saveGame.player.health;
			SaveGameManager.playerHealth.healthSlider.value	= saveGame.player.health;
			SaveGameManager.playerTransform.position		= playerPosition;
			SaveGameManager.playerTransform.rotation		= playerRotation;
		}


		/**
		 * Assigns the game data by the given save game
		 */
		private static void AssignGameData(SaveGame saveGame)
		{
			ScoreManager.score				= saveGame.game.score;
			Camera.main.transform.position	= new Vector3(
				saveGame.game.cameraPositionX,
				saveGame.game.cameraPositionY,
				saveGame.game.cameraPositionZ
			);
		}


		/**
		 * Spawns and assigns the enemy data by the given save game and destroies the current enemies
		 * TODO: Add spwan / load pooling so the game objects will not be destroied?
		 */
		private static void AssignEnemyData(SaveGame saveGame)
		{
			var enemies = GameObject.FindGameObjectsWithTag("Enemy");
			for (var index = 0; index < enemies.Length; index ++)
			{
				GameObject.Destroy(enemies[index].gameObject);
			}
			
			for (var index = 0; index < saveGame.enemies.Count; index ++)
			{
				var enemySpawnPosition	= new Vector3(
					saveGame.enemies[index].positionX,
					saveGame.enemies[index].positionY,
					saveGame.enemies[index].positionZ
				);
				var enemySpawnRotation	= new Quaternion(
					saveGame.enemies[index].rotationX,
					saveGame.enemies[index].rotationY,
					saveGame.enemies[index].rotationZ,
					saveGame.enemies[index].rotationW
				);
				var enemyHealth = saveGame.enemies[index].health;
				
				EnemyManager.instances[saveGame.enemies[index].type].Spawn(
					enemySpawnPosition,
					enemySpawnRotation,
					enemyHealth
				);
			}
		}


		/**
		 * Saves all game relevant data (player-, game- and enemy-data) to a save game named like the given string
		 */
		public static void Save(string saveGameName)
		{
			var binaryFormatter	= new BinaryFormatter();
			var saveGameFile	= File.Create(Application.persistentDataPath + "/" + saveGameName + ".savegame");
			var saveGame		= new SaveGame();

			SaveGameManager.AddPlayerData(saveGame);
			SaveGameManager.AddGameData(saveGame);
			SaveGameManager.AddEnemyData(saveGame);

			binaryFormatter.Serialize(saveGameFile, saveGame);
			saveGameFile.Close();

			Debug.Log("The savegame " + saveGameName + " was successfully saved!");
		}


		/**
		 * Adds the relevant player data to the given save game
		 */
		private static void AddPlayerData(SaveGame saveGame)
		{
			saveGame.player.health		= playerHealth.currentHealth;
			saveGame.player.positionX	= playerTransform.position.x;
			saveGame.player.positionY	= playerTransform.position.y;
			saveGame.player.positionZ	= playerTransform.position.z;
			saveGame.player.rotationX	= playerTransform.rotation.x;
			saveGame.player.rotationY	= playerTransform.rotation.y;
			saveGame.player.rotationZ	= playerTransform.rotation.z;
			saveGame.player.rotationW	= playerTransform.rotation.w;
		}


		/**
		 * Adds the relevant game data to the given save game
		 */
		private static void AddGameData(SaveGame saveGame)
		{
			saveGame.game.score				= ScoreManager.score;
			saveGame.game.cameraPositionX	= Camera.main.transform.position.x;
			saveGame.game.cameraPositionY	= Camera.main.transform.position.y;
			saveGame.game.cameraPositionZ	= Camera.main.transform.position.z;
		}


		/**
		 * Adds the relevant enemy data to the given save game
		 */
		private static void AddEnemyData(SaveGame saveGame)
		{
			var enemies			= GameObject.FindGameObjectsWithTag("Enemy");
			saveGame.enemies	= new List<SaveGameEnemyData>();

			for (var index = 0; index < enemies.Length; index ++)
			{
				var enemyHealth = enemies[index].gameObject.GetComponent<EnemyHealth>();
				
				if (!enemyHealth.IsDead())
				{
					var enemyTransform		= enemies[index].gameObject.transform;
					var saveGameEnemyData	= new SaveGameEnemyData();

					saveGameEnemyData.health	= enemyHealth.currentHealth;
					saveGameEnemyData.positionX	= enemyTransform.position.x;
					saveGameEnemyData.positionY	= enemyTransform.position.y;
					saveGameEnemyData.positionZ	= enemyTransform.position.z;
					saveGameEnemyData.rotationX	= enemyTransform.rotation.x;
					saveGameEnemyData.rotationY	= enemyTransform.rotation.y;
					saveGameEnemyData.rotationZ	= enemyTransform.rotation.z;
					saveGameEnemyData.rotationW	= enemyTransform.rotation.w;
					saveGameEnemyData.type		= enemyHealth.name;

					saveGame.enemies.Add(saveGameEnemyData);
				}
			}
		}
    }


	/**
	 * Stores the whole save game data
	 */
	[Serializable]
	public class SaveGame
	{
		public SaveGameCharacterData	player	= new SaveGameCharacterData();
		public SaveGameGameData			game	= new SaveGameGameData();
		public List<SaveGameEnemyData>	enemies;
	}


	/**
	 * Stores character data
	 */
	[Serializable]
	public class SaveGameCharacterData
	{
		/**
		 * Current character health
		 */
		public int health;


		/**
		 * Current character position
		 */
		public float positionX;
		public float positionY;
		public float positionZ;


		/**
		 * Current character rotation
		 */
		public float rotationX;
		public float rotationY;
		public float rotationZ;
		public float rotationW;
	}


	/**
	 * Stores enemy data
	 */
	[Serializable]
	public class SaveGameEnemyData : SaveGameCharacterData
	{
		/**
		 * The enemy type
		 */
		public string type;
	}


	/**
	 * Stores game data
	 */
	[Serializable]
	public class SaveGameGameData
	{
		/**
		 * The player game score
		 */
		public int score;


		/**
		 * The current camera position
		 */
		public float cameraPositionX;
		public float cameraPositionY;
		public float cameraPositionZ;
	}
}