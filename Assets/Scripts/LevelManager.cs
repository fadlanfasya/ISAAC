using UnityEngine;
using System.Collections;

/// <summary>
/// The level manager is responsible for defining the player ranking. The level manager is a
/// singleton and can be accessed in any script using the LevelManager.Instance syntax.
/// </summary>
public class LevelManager : MonoBehaviour
{
	// The static singleton instance of the level manager.
	public static LevelManager Instance;

	public float goldRequirement;		// Time requirement for Gold rank.
	public float silverRequirement;		// Time requirement for Silver rank.
	public float timeBonus;				// Time bonus for each pickup.

	void Awake()
	{
		// Register this script as the singleton instance.
		Instance = this;
	}

	/// <summary>
	/// Gets the rank for the player given the time spent and pickups collected.
	/// </summary>
	/// <returns>The player rank.</returns>
	/// <param name="timeSpent">The time spent.</param>
	/// <param name="pickupsCollected">The number of pickups collected.</param>
	public PlayerRank GetRank(float timeSpent, int pickupsCollected)
	{
		// Set default rank to Bronze.
		PlayerRank rank = PlayerRank.Bronze;

		// Compute total time.
		float totalTime = timeSpent - (pickupsCollected * timeBonus);

		// Check time limit for Gold rank.
		if (totalTime <= goldRequirement)
		{
			rank = PlayerRank.Gold;
		}
		// Check time limit for Silver rank.
		else if (totalTime <= silverRequirement)
		{
			rank = PlayerRank.Silver;
		}
		return rank;
	}
}