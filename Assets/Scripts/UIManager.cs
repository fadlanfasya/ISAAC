using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

/// <summary>
/// The user interface (UI) manager is responsible for controlling which screen to display
/// as well as updating the current game heads up display (HUD). The UI manager is a singleton
/// and can be accessed in any script using the UIManager.Instance syntax.
/// </summary>
public class UIManager : MonoBehaviour
{
	// The static singleton instance of the UI manager.
	public static UIManager Instance { get; private set; }

	public Text scoreText;				// HUD text for the player score.
	public Text timerText;				// HUD text for the timer.
	public Text rankText;				// HUD text for the player rank.
	public GameObject[] screens;		// GameObject array for all the screens.
	public GameObject hud;				// GameObject for the HUD.
	public AudioClip buttonClick;		// Sound when a button is clicked.

	AudioSource buttonClickSource;

	void Awake()
	{
		// Register this script as the singleton instance.
		Instance = this;
	}

	void Start()
	{
		// Create audio sources for sound playback.
		buttonClickSource = AudioHelper.CreateAudioSource(gameObject, buttonClick);
	}

	/// <summary>
	/// Shows the screen with the given name and hide everything else.
	/// </summary>
	/// <param name="name">Name of the screen to be shown.</param>
	public void ShowScreen(string name)
	{
		// Loop through all the screens in the array.
		foreach (GameObject screen in screens)
		{
			// Activate the screen with the matching name, and deactivate
			// any screen that doesn't match.
			screen.SetActive(screen.name == name);
		}
	}

	/// <summary>
	/// Shows/hides the HUD.
	/// </summary>
	/// <param name="show">Do we show the HUD?</param>
	public void ShowHUD(bool show)
	{
		hud.SetActive(show);
	}

	/// <summary>
	/// Updates the HUD elements.
	/// </summary>
	/// <param name="score">The player's score.</param>
	/// <param name="timeSpent">The time spent so far.</param>
	/// <param name="rank">The player's rank.</param>
	public void UpdateHUD(int score, float timeSpent, PlayerRank rank)
	{
		ShowScore(score);
		ShowTimer(timeSpent);
		ShowRank(rank);
	}

	/// <summary>
	/// Updates the player score on the HUD
	/// </summary>
	/// <param name="score"></param>
	void ShowScore(int score)
	{
		scoreText.text = string.Format(LocalizationManager.Instance.GetString("HUD Score"), score.ToString());
	}

	/// <summary>
	/// Updates the timer on the HUD.
	/// </summary>
	/// <param name="timeSpent">Time spent.</param>
	void ShowTimer(float timeSpent)
	{
		TimeSpan tSpan = TimeSpan.FromSeconds(timeSpent);
		string timeString = String.Format("{0:00}:{1:00}:{2:000}", tSpan.Minutes, tSpan.Seconds, tSpan.Milliseconds);
		timerText.text = String.Format(LocalizationManager.Instance.GetString("HUD Timer"), timeString);
	}

	/// <summary>
	/// Updates the player rank on the HUD.
	/// </summary>
	/// <param name="rank">Player's current rank.</param>
	void ShowRank(PlayerRank rank)
	{
		string rankKey = "Rank " + rank.ToString();
		rankText.text = string.Format(LocalizationManager.Instance.GetString("HUD Rank"), LocalizationManager.Instance.GetString(rankKey));
	}

	/// <summary>
	/// Call this function to play the button click sound.
	/// </summary>
	public void OnButton()
	{
		buttonClickSource.Play();
	}

	/// <summary>
	/// Call this function when the language changed.
	/// </summary>
	public void OnLanguageChanged()
	{
		foreach (GameObject o in screens)
		{
			var staticText = o.GetComponent<StaticTextManager>();
			if (staticText)
			{
				staticText.OnLanguageChanged();
			}
		}
	}
}