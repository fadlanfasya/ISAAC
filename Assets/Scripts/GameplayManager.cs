using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// The gameplay manager is responsible for controlling the overall flow of the game. The
/// game is divided into three main states: Tutorial, InGame, and GameOver. The user interface
/// and input controls are different depending on the current game state. The gameplay
/// manager tracks the player progress and switches between the game states based on
/// the results as well as the user input. The gameplay manager is a singleton and can be
/// accessed in any script using the GameplayManager.Instance syntax.
/// </summary>
public class GameplayManager : MonoBehaviour
{
	// The static singleton instance of the gameplay manager.
	public static GameplayManager Instance { get; private set; }

	// Whether we are restarting or not (default is false).
	public static bool Restart { get; set; }

	// Enumeration for the different game states. The default starting
	// state is the tutorial.
	enum GameState
	{
		Tutorial,	// Show player the game instructions.
		InGame,		// Player can start controlling the robot.
		GameOver,	// Game ended, player input is blocked.
	};
	GameState state = GameState.Tutorial;
	
	PlayerRank rank = PlayerRank.Gold;	// The rank of the player for the current level.
	float timeSpent = 0f;				// The time spent by the player.
	int score = 0;						// The player's score.

	void Awake()
	{
		// Register this script as the singleton instance.
		Instance = this;
	}

	void Start()
	{
		// Refresh and hide the HUD.
		UIManager.Instance.ShowHUD(false);
		UIManager.Instance.UpdateHUD(score, timeSpent, rank);

		// Check if we are restarting.
		if (Restart)
		{
			// Reset the flag
			Restart = false;

			// Start the game right away.
			OnStartGame();
		}
		else
		{
			// If no, show the tutorial screen.
			UIManager.Instance.ShowScreen("Tutorial");
		}
	}

	void Update()
	{
		// Check if we can start playing.
		if (CanPlay())
		{
			// Update the time spent and display on the HUD.
			timeSpent += Time.deltaTime;

			// Update player rank.
			rank = LevelManager.Instance.GetRank(timeSpent, score);

			UIManager.Instance.UpdateHUD(score, timeSpent, rank);
		}
	}

	/// <summary>
	/// Reloads the current scene.
	/// </summary>
	void ReloadScene()
	{
		Application.LoadLevel(Application.loadedLevel);
	}

	/// <summary>
	/// Call this function to start the gameplay.
	/// </summary>
	public void OnStartGame()
	{
		state = GameState.InGame;
		UIManager.Instance.ShowHUD(true);
		UIManager.Instance.ShowScreen("");
	}

	/// <summary>
	/// Call this function to restart the current level.
	/// </summary>
	public void OnRestart()
	{
		// Set the restart flag to true, this will skip the tutorial next time.
		Restart = true;

		// Reload the current scene.
		Invoke("ReloadScene", 0.5f);
	}

	/// <summary>
	/// Call this function when the player collects a pickup.
	/// </summary>
	public void OnPickup()
	{
		++score;
		rank = LevelManager.Instance.GetRank(timeSpent, score);
		UIManager.Instance.UpdateHUD(score, timeSpent, rank);
	}

	/// <summary>
	/// Call this function when the player is caught.
	/// </summary>
	public void OnPlayerCaught()
	{
		state = GameState.GameOver;
		UIManager.Instance.ShowScreen("GameOver");
	}

	/// <summary>
	/// Call this function when the player reaches the goal.
	/// </summary>
	public void OnGoal()
	{
		state = GameState.GameOver;
		UIManager.Instance.ShowScreen("YouWin");
	}

	/// <summary>
	/// Determines whether the player can start playing.
	/// </summary>
	/// <returns><c>true</c> if the player can play; otherwise, <c>false</c>.</returns>
	public bool CanPlay()
	{
		// The player can move only during the InGame state.
		return (state == GameState.InGame);
	}

	public void OnLanguageChanged()
	{
		UIManager.Instance.OnLanguageChanged();
		UIManager.Instance.UpdateHUD(score, timeSpent, rank);
	}
}