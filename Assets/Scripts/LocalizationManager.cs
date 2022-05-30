using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class LocalizationManager : MonoBehaviour
{
	public static LocalizationManager Instance;

	List<string> languages;
	Dictionary<string, Dictionary<string, string>> locDictionary;
	static string currentLanguage;

	void Awake()
	{
		Instance = this;
		languages = new List<string>();

		locDictionary = CSVReader.Read("Localization");
		var first = locDictionary.ElementAt(0).Value;
		foreach(string k in first.Keys)
		{
			languages.Add(k);
		}

		if (currentLanguage == null || currentLanguage.Length == 0)
		{
			currentLanguage = languages.First();
		}
	}

	public string GetString(string key)
	{
		return GetLocString(key, currentLanguage);
	}

	public string GetLanguageString(string language)
	{
		return GetLocString("Language", language);
	}

	string GetLocString(string key, string language)
	{
		if (locDictionary.ContainsKey(key))
		{
			if (locDictionary[key].ContainsKey(language))
			{
				return locDictionary[key][language];
			}
		}

		return "?_no_key_?";
	}

	public List<string> GetLanguages()
	{
		return languages.ToList(); // copy
	}

	public string GetCurrentLanguage()
	{
		return currentLanguage;
	}

	public void SetLanguage(string language)
	{
		if (languages.Contains(language))
		{
			currentLanguage = language;
			GameplayManager.Instance.OnLanguageChanged();
		}
		else
		{
			Debug.LogWarningFormat("Localizer does not have translations for language {0}, ignoring...", language);
		}
	}

	// Use this for initialization
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{

	}
}
