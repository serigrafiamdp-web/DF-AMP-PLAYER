// DF-AMP PLAYER
// Decompiled from the exact compiled core embedded in MASTER_DFAMP_09G_07_OCT_2026.zip.
// Embedded core SHA-256: 01dd2d8df9900b8a4fe54ceb0698bd1ad00966e8e30fe11fccdc6803e7d7734a
// Original playback base: Dynamic Music by Numidium3rd / numidium (MIT).
// DF-AMP extended modifications: DF-AMP PLAYER BY RICO.
// Decompiled source is semantically recovered source; original comments/format/local names may differ.

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using DaggerfallConnect;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Serialization;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using DaggerfallWorkshop.Game.Utility;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;
using DaggerfallWorkshop.Game.Weather;
using UnityEngine;
using Wenzil.Console;

namespace DynamicMusic;

public sealed class DynamicMusic : MonoBehaviour
{
	private enum MusicPlaylist : byte
	{
		DungeonInterior,
		Sunny,
		Cloudy,
		Overcast,
		Rain,
		Snow,
		Temple,
		Tavern,
		Night,
		Shop,
		MagesGuild,
		Interior,
		Palace,
		Castle,
		Court,
		MainMenu,
		CharCreation,
		None
	}

	private enum MusicEnvironment : byte
	{
		Castle,
		City,
		DungeonExterior,
		DungeonInterior,
		Graveyard,
		MagesGuild,
		Interior,
		Palace,
		Shop,
		Tavern,
		Temple,
		Wilderness
	}

	private enum State : byte
	{
		Normal,
		FadingOut,
		FadingIn
	}

	private sealed class Playlist
	{
		public enum Flags : byte
		{
			None = 0,
			CrashIn = 1,
			ResumePrevious = 2,
			PlayUntilCombatEnd = 4,
			Sting = 8
		}

		private readonly string[] tracks;

		private int index;

		public Flags PlaylistFlags;

		public int TrackCount => tracks.Length;

		public string CurrentTrack => tracks[index];

		private void ShuffleTracks()
		{
			if (Instance.loopCustomTracks)
			{
				string text = tracks[(nint)tracks.LongLength - 1];
				for (int num = tracks.Length - 1; num > 0; num--)
				{
					int num2 = Random.Range(0, num + 1);
					ref string reference = ref tracks[num2];
					ref string reference2 = ref tracks[num];
					string text2 = tracks[num];
					string text3 = tracks[num2];
					reference = text2;
					reference2 = text3;
				}
				if (tracks[0] == text)
				{
					ref string reference = ref tracks[0];
					ref string reference3 = ref tracks[tracks.Length - 1];
					string text3 = tracks[tracks.Length - 1];
					string text2 = tracks[0];
					reference = text3;
					reference3 = text2;
				}
			}
		}

		public Playlist(List<string> trackList)
		{
			tracks = trackList.ToArray();
			ShuffleTracks();
		}

		public string GetNextTrack()
		{
			index = (index + 1) % tracks.Length;
			if (index == 0)
			{
				ShuffleTracks();
			}
			return tracks[index];
		}

		public static Flags GetFlagsFromText(string text)
		{
			string text2 = text.ToLower();
			Flags[] array = (Flags[])Enum.GetValues(typeof(Flags));
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].ToString().ToLower() == text2)
				{
					return array[i];
				}
			}
			return Flags.None;
		}

		public bool HasFlags(Flags flags)
		{
			return (PlaylistFlags & flags) == flags;
		}
	}

	private sealed class ConditionUsage
	{
		public enum Conditions : ushort
		{
			None,
			Night,
			Interior,
			Dungeon,
			DungeonCastle,
			LocationType,
			BuildingType,
			WeatherType,
			FactionId,
			Climate,
			ClimateIndex,
			RegionIndex,
			DungeonType,
			BuildingQuality,
			Season,
			Month,
			StartMenu,
			ReadingBook,
			Combat,
			Swimming,
			BuildingIsOpen,
			FastTraveled
		}

		public bool NegateArg { get; set; }

		public int[] ParameterArgs { get; set; }

		public Conditions Condition { get; set; }

		public static Conditions GetConditionFromText(string text)
		{
			string text2 = text.ToLower();
			Conditions[] array = (Conditions[])Enum.GetValues(typeof(Conditions));
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].ToString().ToLower() == text2)
				{
					return array[i];
				}
			}
			return Conditions.None;
		}
	}

	private static SongFiles[] _dungeonSongs;

	private static SongFiles[] _sunnySongs;

	private static SongFiles[] _sunnySongsFM;

	private static SongFiles[] _cloudySongs;

	private static SongFiles[] _cloudySongsFM;

	private static SongFiles[] _overcastSongs;

	private static SongFiles[] _overcastSongsFM;

	private static SongFiles[] _rainSongs;

	private static SongFiles[] _snowSongs;

	private static SongFiles[] _sneakingSongs;

	private static SongFiles[] _templeSongs;

	private static SongFiles[] _tavernSongs;

	private static SongFiles[] _nightSongs;

	private static SongFiles[] _dungeonSongsFM;

	private static SongFiles[] _daySongsFM;

	private static SongFiles[] _weatherRainSongsFM;

	private static SongFiles[] _weatherSnowSongsFM;

	private static SongFiles[] _sneakingSongsFM;

	private static SongFiles[] _templeSongsFM;

	private static SongFiles[] _tavernSongsFM;

	private static SongFiles[] _nightSongsFM;

	private static SongFiles[] _unusedDungeonSongs;

	private static SongFiles[] _unusedDungeonSongsFM;

	private static SongFiles[] _shopSongs;

	private static SongFiles[] _shopSongsFM;

	private static SongFiles[] _magesGuildSongs;

	private static SongFiles[] _magesGuildSongsFM;

	private static SongFiles[] _interiorSongs;

	private static SongFiles[] _interiorSongsFM;

	private static SongFiles[] _unusedKnightSong;

	private static SongFiles[] _unusedKnightSongFM;

	private static SongFiles[] _palaceSongs;

	private static SongFiles[] _palaceSongsFM;

	private static SongFiles[] _castleSongs;

	private static SongFiles[] _castleSongsFM;

	private static SongFiles[] _courtSongs;

	private static SongFiles[] _courtSongsFM;

	public SongFiles[] DungeonInteriorSongs = _dungeonSongs;

	public SongFiles[] SunnySongs = _sunnySongs;

	public SongFiles[] CloudySongs = _cloudySongs;

	public SongFiles[] OvercastSongs = _overcastSongs;

	public SongFiles[] RainSongs = _rainSongs;

	public SongFiles[] SnowSongs = _snowSongs;

	public SongFiles[] TempleSongs = _templeSongs;

	public SongFiles[] TavernSongs = _tavernSongs;

	public SongFiles[] NightSongs = _nightSongs;

	public SongFiles[] ShopSongs = _shopSongs;

	public SongFiles[] MagesGuildSongs = _magesGuildSongs;

	public SongFiles[] InteriorSongs = _interiorSongs;

	public SongFiles[] PalaceSongs = _palaceSongs;

	public SongFiles[] CastleSongs = _castleSongs;

	public SongFiles[] CourtSongs = _courtSongs;

	public SongFiles[] SneakingSongs = _sneakingSongs;

	private static Mod mod;

	private DaggerfallUnity daggerfallUnity;

	private PlayerGPS localPlayerGPS;

	private PlayerEnterExit playerEnterExit;

	private PlayerWeather playerWeather;

	private DynamicSongPlayer dynamicSongPlayer;

	private GameManager gameManager;

	private PlayerEntity playerEntity;

	private const float detectionCheckInterval = 3f;

	private float detectionCheckDelta;

	private const float fadeOutLength = 2f;

	private const float fadeInLength = 2f;

	private float fadeOutTime;

	private float fadeInTime;

	private const byte combatTaperLength = 2;

	private byte combatTaper;

	private Playlist[] customPlaylists;

	private Dictionary<int, ConditionUsage[]> userDefinedConditionSets;

	private string currentCustomTrack;

	private bool customTrackQueued;

	private bool combatMusicIsEnabled;

	private bool isInCombat;

	private int maxEnemyLevel;

	private bool resumeIsEnabled = true;

	private bool loopCustomTracks;

	private float previousTimeSinceStartup;

	private float deltaTime;

	private float resumeSeeker;

	private float stingWaitTime;

	private float stingDelay = 1f;

	private int resumePlaylist;

	private MusicPlaylist lastVanillaPlaylist;

	private bool gameLoaded;

	private int currentPlaylist;

	private bool isWaitingForTravelSting;

	private bool isPlayingSting;

	private int lastlocationIndex;

	private uint lastGameDays;

	private State currentState;

	private string debugPlaylistName;

	private string debugSongName;

	private GUIStyle guiStyle;

	private const string fileSearchPattern = "*.ogg";

	private const string modSignature = "Dynamic Music";


	public static DynamicMusic Instance { get; private set; }

	[Invoke(/*Could not decode attribute arguments.*/)]
	public static void Init(InitParams initParams)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected Obj, but got Unknown
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Expected Obj, but got Unknown
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Expected Obj, but got Unknown
		mod = initParams.Mod;
		GameObject val = new GameObject(mod.Title);
		Instance = val.AddComponent<DynamicMusic>();
		Instance.dynamicSongPlayer = val.AddComponent<DynamicSongPlayer>();
		Instance.dynamicSongPlayer.ModSignature = "Dynamic Music";
		Object.DontDestroyOnLoad((Object)(object)val);
		SaveLoadManager.OnLoad += SaveLoadManager_OnLoad;
		StartGameBehaviour.OnStartGame = (EventHandler)Delegate.Combine(StartGameBehaviour.OnStartGame, new EventHandler(StartGameBehaviour_OnStartGame));
		DaggerfallTravelPopUp.OnPostFastTravel += OnPostFastTravel;
		mod.LoadSettingsCallback = Instance.LoadSettings;
		DynamicSongPlayer.OnSongEnd += OnSongEnd;
	}

	private void LoadSettings(ModSettings settings, ModSettingsChange change)
	{
		combatMusicIsEnabled = settings.GetValue<bool>("Options", "Enable Combat Music");
		resumeIsEnabled = settings.GetValue<bool>("Options", "Enable Track Resume");
		loopCustomTracks = settings.GetValue<bool>("Options", "Loop Custom Tracks");
	}

	private void Start()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0472: Unknown result type (might be due to invalid IL or missing references)
		//IL_0496: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_060f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0619: Expected Obj, but got Unknown
		//IL_0620: Unknown result type (might be due to invalid IL or missing references)
		//IL_062a: Expected Obj, but got Unknown
		//IL_0631: Unknown result type (might be due to invalid IL or missing references)
		//IL_063b: Expected Obj, but got Unknown
		//IL_0642: Unknown result type (might be due to invalid IL or missing references)
		//IL_064c: Expected Obj, but got Unknown
		//IL_0659: Unknown result type (might be due to invalid IL or missing references)
		//IL_0663: Expected Obj, but got Unknown
		//IL_0664: Unknown result type (might be due to invalid IL or missing references)
		//IL_066e: Expected Obj, but got Unknown
		//IL_0679: Unknown result type (might be due to invalid IL or missing references)
		daggerfallUnity = DaggerfallUnity.Instance;
		ModSettings settings = mod.GetSettings();
		LoadSettings(settings, default);
		string text = Path.Combine(Application.streamingAssetsPath, "Sound", "DynMusic");
		List<Playlist> list = new List<Playlist>();
		string path = Path.Combine(text, "UserDefined.txt");
		if (Directory.Exists(text) && File.Exists(path))
		{
			using StreamReader streamReader = new StreamReader(path);
			userDefinedConditionSets = new Dictionary<int, ConditionUsage[]>();
			ushort num = 0;
			string text2;
			while ((text2 = streamReader.ReadLine()) != null)
			{
				text2 = text2.Trim();
				num++;
				if (text2 == string.Empty || text2[0] == '#')
				{
					continue;
				}
				bool flag = false;
				string[] array = null;
				if (text2.Split(new char[1] { '|' }).Length > 1)
				{
					array = text2.Split(new char[1] { '|' })[1].Split(new char[2] { ' ', ',' });
				}
				string[] array2 = text2.Split(new char[1] { '|' })[0].Split(new char[2] { ' ', ',' });
				string text3 = array2[0];
				if (!Directory.Exists(Path.Combine(text, text3)))
				{
					PrintParserError("Reference to non-existent playlist directory", num, text3);
					continue;
				}
				string[] files = Directory.GetFiles(Path.Combine(text, text3), "*.ogg");
				List<string> list2 = new List<string>();
				if (files.Length != 0)
				{
					string[] array3 = files;
					foreach (string item in array3)
					{
						list2.Add(item);
					}
					list.Add(new Playlist(list2));
				}
				int key = 17 + list.Count;
				List<ConditionUsage> list3 = new List<ConditionUsage>();
				int num2;
				for (num2 = 2; num2 < array2.Length; num2++)
				{
					bool negateArg = false;
					if (array2[num2].ToLower() == "not")
					{
						num2++;
						negateArg = true;
					}
					ConditionUsage.Conditions conditionFromText = ConditionUsage.GetConditionFromText(array2[num2]);
					if (conditionFromText == ConditionUsage.Conditions.None)
					{
						PrintParserError("Unrecognized condition", num, array2[num2]);
						flag = true;
						break;
					}
					num2++;
					if (!flag)
					{
						List<int> list4 = new List<int>();
						while (num2 < array2.Length && array2[num2] != "")
						{
							if (!int.TryParse(array2[num2++], out var result))
							{
								PrintParserError("Invalid argument", num, conditionFromText.ToString());
								flag = true;
								break;
							}
							list4.Add(result);
						}
						ConditionUsage item2 = new ConditionUsage
						{
							NegateArg = negateArg,
							ParameterArgs = list4.ToArray(),
							Condition = conditionFromText
						};
						list3.Add(item2);
					}
				}
				num2 = 0;
				if (array != null)
				{
					while (num2 < array.Length)
					{
						list[list.Count - 1].PlaylistFlags |= Playlist.GetFlagsFromText(array[num2++]);
					}
				}
				if (!flag)
				{
					userDefinedConditionSets[key] = list3.ToArray();
				}
			}
		}
		customPlaylists = new Playlist[17 + list.Count + 1];
		for (int j = 0; j < 18; j++)
		{
			string path2 = Path.Combine(text, $"{(MusicPlaylist)j}");
			if (!Directory.Exists(path2))
			{
				Directory.CreateDirectory(path2);
				continue;
			}
			string[] files2 = Directory.GetFiles(path2, "*.ogg");
			if (files2.Length != 0)
			{
				List<string> list5 = new List<string>();
				string[] array3 = files2;
				foreach (string item3 in array3)
				{
					list5.Add(item3);
				}
				customPlaylists[j] = new Playlist(list5);
			}
		}
		int num3 = 0;
		int num4 = 18;
		while (num3 < list.Count)
		{
			customPlaylists[num4] = list[num3++];
			num4++;
		}
		gameManager = GameManager.Instance;
		Object.Destroy((Object)(object)((Component)gameManager.DungeonParent.transform.Find("SongPlayer")).gameObject);
		Object.Destroy((Object)(object)((Component)gameManager.InteriorParent.transform.Find("SongPlayer")).gameObject);
		Object.Destroy((Object)(object)((Component)gameManager.ExteriorParent.transform.Find("SongPlayer")).gameObject);
		new GameObject("SongPlayer").transform.parent = gameManager.DungeonParent.transform;
		new GameObject("SongPlayer").transform.parent = gameManager.InteriorParent.transform;
		new GameObject("SongPlayer").transform.parent = gameManager.ExteriorParent.transform;
		playerEntity = gameManager.PlayerEntity;
		localPlayerGPS = gameManager.PlayerGPS;
		playerEnterExit = ((Component)localPlayerGPS).GetComponent<PlayerEnterExit>();
		playerWeather = ((Component)localPlayerGPS).GetComponent<PlayerWeather>();
		previousTimeSinceStartup = Time.realtimeSinceStartup;
		gameLoaded = false;
		currentState = State.FadingIn;
		fadeInTime = 2f;
		currentPlaylist = 17;
		if (DaggerfallUnity.Settings.AlternateMusic)
		{
			DungeonInteriorSongs = _dungeonSongsFM;
			SunnySongs = _sunnySongsFM;
			CloudySongs = _cloudySongsFM;
			OvercastSongs = _overcastSongsFM;
			RainSongs = _weatherRainSongsFM;
			SnowSongs = _weatherSnowSongsFM;
			TempleSongs = _templeSongsFM;
			TavernSongs = _tavernSongsFM;
			NightSongs = _nightSongsFM;
			ShopSongs = _shopSongsFM;
			MagesGuildSongs = _magesGuildSongsFM;
			InteriorSongs = _interiorSongsFM;
			PalaceSongs = _palaceSongsFM;
			CastleSongs = _castleSongsFM;
			CourtSongs = _courtSongsFM;
			SneakingSongs = _sneakingSongsFM;
		}
		PlayerEnterExit.OnTransitionInterior += OnTransitionInterior;
		PlayerEnterExit.OnTransitionExterior += OnTransitionExterior;
		PlayerEnterExit.OnTransitionDungeonInterior += OnTransitionDungeonInterior;
		PlayerEnterExit.OnTransitionDungeonExterior += OnTransitionDungeonExterior;
		((DaggerfallEntity)playerEntity).OnDeath += OnDeath;
		guiStyle = new GUIStyle();
		guiStyle.normal.textColor = Color.black;
		Debug.Log((object)"Dynamic Music initialized.");
		mod.IsReady = true;
	}

	private void Update()
	{
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		deltaTime = realtimeSinceStartup - previousTimeSinceStartup;
		previousTimeSinceStartup = realtimeSinceStartup;
		if (deltaTime < 0f)
		{
			deltaTime = 0f;
		}
		if (isPlayingSting && dynamicSongPlayer.IsStoppedClip && dynamicSongPlayer.IsStinging)
		{
			dynamicSongPlayer.IsStinging = false;
			isPlayingSting = false;
			currentState = State.FadingOut;
			fadeOutTime = 2f;
		}
		int num = currentPlaylist;
		if (!isPlayingSting && (!isInCombat || customPlaylists[num] == null || customPlaylists[num].HasFlags(Playlist.Flags.PlayUntilCombatEnd)))
		{
			currentPlaylist = (int)GetMusicPlaylist(localPlayerGPS, playerEnterExit, playerWeather);
			if (currentPlaylist != 17)
			{
				int userDefinedPlaylistKey = GetUserDefinedPlaylistKey(userDefinedConditionSets);
				if (userDefinedPlaylistKey >= 0)
				{
					currentPlaylist = userDefinedPlaylistKey;
					isPlayingSting = customPlaylists[currentPlaylist].HasFlags(Playlist.Flags.Sting);
				}
			}
		}
		if (stingWaitTime > 0f)
		{
			stingWaitTime -= deltaTime;
		}
		else
		{
			isWaitingForTravelSting = false;
		}
		switch (currentState)
		{
		case State.Normal:
			if (currentPlaylist != num && num != 17)
			{
				if (customPlaylists[currentPlaylist] != null && customPlaylists[currentPlaylist].HasFlags(Playlist.Flags.CrashIn))
				{
					currentState = State.FadingOut;
					fadeOutTime = 2f;
					fadeInTime = 2f;
					if (customPlaylists[currentPlaylist].HasFlags(Playlist.Flags.ResumePrevious))
					{
						resumePlaylist = num;
					}
				}
				else
				{
					currentState = State.FadingOut;
				}
			}
			if (currentState != State.FadingOut && customPlaylists[currentPlaylist] != null && customPlaylists[currentPlaylist].HasFlags(Playlist.Flags.Sting) && !dynamicSongPlayer.IsStinging)
			{
				PlayCurrentTrack();
				dynamicSongPlayer.IsStinging = true;
			}
			if (currentPlaylist == 17)
			{
				if (dynamicSongPlayer.IsPlaying)
				{
					currentCustomTrack = string.Empty;
					dynamicSongPlayer.Stop();
				}
			}
			else
			{
				dynamicSongPlayer.AudioSource.volume = DaggerfallUnity.Settings.MusicVolume;
			}
			break;
		case State.FadingOut:
			fadeOutTime += deltaTime;
			dynamicSongPlayer.AudioSource.volume = Mathf.Lerp(DaggerfallUnity.Settings.MusicVolume, 0f, fadeOutTime / 2f);
			if (fadeOutTime >= 2f)
			{
				fadeOutTime = 0f;
				currentState = State.FadingIn;
			}
			break;
		case State.FadingIn:
			if (currentPlaylist != num && num != 17)
			{
				if (customPlaylists[currentPlaylist] != null && customPlaylists[currentPlaylist].HasFlags(Playlist.Flags.CrashIn))
				{
					currentState = State.FadingOut;
					fadeOutTime = 2f;
					fadeInTime = 2f;
				}
				else
				{
					currentState = State.FadingOut;
					fadeInTime = 0f;
				}
				break;
			}
			if (customPlaylists[currentPlaylist] != null && customPlaylists[currentPlaylist].HasFlags(Playlist.Flags.Sting))
			{
				fadeInTime = 2f;
			}
			if (currentPlaylist != 17)
			{
				if (dynamicSongPlayer.AudioSource.volume == 0f)
				{
					PlayCurrentTrack();
				}
				fadeInTime += deltaTime;
				if (fadeInTime >= 2f)
				{
					fadeInTime = 0f;
					currentState = State.Normal;
					dynamicSongPlayer.AudioSource.volume = DaggerfallUnity.Settings.MusicVolume;
				}
				else
				{
					dynamicSongPlayer.AudioSource.volume = Mathf.Lerp(0f, DaggerfallUnity.Settings.MusicVolume, fadeInTime / 2f);
				}
			}
			break;
		}
		detectionCheckDelta += Time.deltaTime;
		if (detectionCheckDelta < 3f)
		{
			return;
		}
		if (currentState == State.Normal)
		{
			if (combatMusicIsEnabled && !gameManager.PlayerDeath.DeathInProgress && !playerEntity.Arrested && GetCombatStatus(out maxEnemyLevel))
			{
				isInCombat = true;
				combatTaper = 2;
			}
			else if (combatTaper == 0 || --combatTaper <= 0)
			{
				isInCombat = false;
			}
		}
		detectionCheckDelta = 0f;
	}

	private void OnGUI()
	{
		if (DefaultCommands.showDebugStrings || (currentState == State.Normal && dynamicSongPlayer.IsImported && dynamicSongPlayer.IsAudioSourcePlaying && dynamicSongPlayer.AudioSource.clip != null && !(dynamicSongPlayer.AudioSource.clip.length - dynamicSongPlayer.AudioSource.time > 2f)))
		{
			resumeSeeker = 0f;
			customTrackQueued = true;
			loopCustomTracks = false;
			currentState = State.FadingOut;
			fadeOutTime = 0f;
			DefaultCommands.showDebugStrings = false;
		}
	}

	private bool GetIsConditionTrue(ConditionUsage.Conditions condition, bool negate, int[] parameters)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Invalid comparison between Unknown and I4
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Invalid comparison between Unknown and I4
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Invalid comparison between Unknown and I4
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Invalid comparison between Unknown and I4
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Invalid comparison between Unknown and I4
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Invalid comparison between Unknown and I4
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Invalid comparison between Unknown and I4
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Invalid comparison between Unknown and I4
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Invalid comparison between Unknown and I4
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Invalid comparison between Unknown and I4
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		//IL_0247: Invalid comparison between Unknown and I4
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Invalid comparison between Unknown and I4
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Invalid comparison between Unknown and I4
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Invalid comparison between Unknown and I4
		//IL_02cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d3: Invalid comparison between Unknown and I4
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Invalid comparison between Unknown and I4
		//IL_0333: Unknown result type (might be due to invalid IL or missing references)
		//IL_0339: Invalid comparison between Unknown and I4
		//IL_037e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0384: Invalid comparison between Unknown and I4
		//IL_03ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d4: Invalid comparison between Unknown and I4
		//IL_041d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0423: Invalid comparison between Unknown and I4
		//IL_034f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0301: Unknown result type (might be due to invalid IL or missing references)
		//IL_0306: Unknown result type (might be due to invalid IL or missing references)
		//IL_030d: Invalid comparison between Unknown and I4
		//IL_043b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0441: Invalid comparison between Unknown and I4
		//IL_03a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ab: Invalid comparison between Unknown and I4
		//IL_0485: Unknown result type (might be due to invalid IL or missing references)
		//IL_048b: Invalid comparison between Unknown and I4
		//IL_04b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b9: Invalid comparison between Unknown and I4
		//IL_04db: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ec: Invalid comparison between Unknown and I4
		//IL_0517: Unknown result type (might be due to invalid IL or missing references)
		//IL_051c: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		switch (condition)
		{
		case ConditionUsage.Conditions.Night:
			flag = (int)gameManager.StateManager.CurrentState != 2 && daggerfallUnity.WorldTime.Now.IsNight;
			break;
		case ConditionUsage.Conditions.Interior:
			flag = (int)gameManager.StateManager.CurrentState != 2 && playerEnterExit.IsPlayerInside;
			break;
		case ConditionUsage.Conditions.Dungeon:
			flag = (int)gameManager.StateManager.CurrentState != 2 && playerEnterExit.IsPlayerInsideDungeon;
			break;
		case ConditionUsage.Conditions.DungeonCastle:
			flag = (int)gameManager.StateManager.CurrentState != 2 && playerEnterExit.IsPlayerInsideDungeonCastle;
			break;
		case ConditionUsage.Conditions.LocationType:
		{
			if ((int)gameManager.StateManager.CurrentState == 2 || playerEnterExit.IsPlayerInsideDungeon || !localPlayerGPS.IsPlayerInLocationRect)
			{
				return false;
			}
			int[] array = parameters;
			foreach (int num in array)
			{
				flag |= (int)localPlayerGPS.CurrentLocationType == num;
			}
			break;
		}
		case ConditionUsage.Conditions.BuildingType:
		{
			if ((int)gameManager.StateManager.CurrentState == 2)
			{
				return false;
			}
			int[] array = parameters;
			foreach (int num7 in array)
			{
				flag |= (int)playerEnterExit.BuildingType == num7;
			}
			break;
		}
		case ConditionUsage.Conditions.WeatherType:
		{
			if ((int)gameManager.StateManager.CurrentState == 2)
			{
				return false;
			}
			int[] array = parameters;
			foreach (int num8 in array)
			{
				flag |= (int)playerWeather.WeatherType == num8;
			}
			break;
		}
		case ConditionUsage.Conditions.FactionId:
		{
			if ((int)gameManager.StateManager.CurrentState == 2)
			{
				return false;
			}
			int[] array = parameters;
			foreach (int num4 in array)
			{
				flag |= playerEnterExit.FactionID == num4;
			}
			break;
		}
		case ConditionUsage.Conditions.Climate:
		{
			if ((int)gameManager.StateManager.CurrentState == 2)
			{
				return false;
			}
			int[] array = parameters;
			foreach (int num10 in array)
			{
				flag |= (int)localPlayerGPS.ClimateSettings.ClimateType == num10;
			}
			break;
		}
		case ConditionUsage.Conditions.ClimateIndex:
		{
			if ((int)gameManager.StateManager.CurrentState == 2)
			{
				return false;
			}
			int[] array = parameters;
			foreach (int num6 in array)
			{
				flag |= localPlayerGPS.CurrentClimateIndex == num6;
			}
			break;
		}
		case ConditionUsage.Conditions.RegionIndex:
		{
			if ((int)gameManager.StateManager.CurrentState == 2)
			{
				return false;
			}
			int[] array = parameters;
			foreach (int num2 in array)
			{
				flag |= localPlayerGPS.CurrentRegionIndex == num2;
			}
			break;
		}
		case ConditionUsage.Conditions.DungeonType:
		{
			if ((int)gameManager.StateManager.CurrentState == 2)
			{
				return false;
			}
			int[] array = parameters;
			foreach (int num9 in array)
			{
				flag |= (Object)(object)playerEnterExit.Dungeon != (Object)null && (int)playerEnterExit.Dungeon.Summary.DungeonType == num9;
			}
			break;
		}
		case ConditionUsage.Conditions.BuildingQuality:
		{
			if ((int)gameManager.StateManager.CurrentState == 2)
			{
				return false;
			}
			int[] array = parameters;
			foreach (int num5 in array)
			{
				flag |= playerEnterExit.BuildingDiscoveryData.quality == num5;
			}
			break;
		}
		case ConditionUsage.Conditions.Season:
		{
			if ((int)gameManager.StateManager.CurrentState == 2)
			{
				return false;
			}
			int[] array = parameters;
			foreach (int num3 in array)
			{
				flag |= (int)gameManager.StreamingWorld.CurrentPlayerLocationObject.CurrentSeason == num3;
			}
			break;
		}
		case ConditionUsage.Conditions.Month:
		{
			if ((int)gameManager.StateManager.CurrentState == 2)
			{
				return false;
			}
			int[] array = parameters;
			foreach (int num11 in array)
			{
				flag |= DaggerfallUnity.Instance.WorldTime.DaggerfallDateTime.MonthOfYear == num11;
			}
			break;
		}
		case ConditionUsage.Conditions.StartMenu:
			flag = (int)gameManager.StateManager.CurrentState == 2;
			break;
		case ConditionUsage.Conditions.Combat:
			if ((int)gameManager.StateManager.CurrentState == 2)
			{
				return false;
			}
			flag = isInCombat;
			if (parameters.Length != 0)
			{
				flag &= maxEnemyLevel - ((DaggerfallEntity)playerEntity).Level >= parameters[0];
			}
			break;
		case ConditionUsage.Conditions.Swimming:
			if ((int)gameManager.StateManager.CurrentState == 2)
			{
				return false;
			}
			flag = playerEnterExit.IsPlayerSwimming;
			break;
		case ConditionUsage.Conditions.BuildingIsOpen:
			if ((int)gameManager.StateManager.CurrentState == 2)
			{
				return false;
			}
			flag = ((Object)(object)playerEnterExit.Interior != (Object)null && (int)playerEnterExit.Interior.BuildingData.BuildingType >= PlayerActivate.openHours.Length) || (playerEnterExit.IsPlayerInside && !playerEnterExit.IsPlayerInsideDungeon && PlayerActivate.IsBuildingOpen(playerEnterExit.Interior.BuildingData.BuildingType));
			break;
		case ConditionUsage.Conditions.FastTraveled:
			flag = isWaitingForTravelSting;
			break;
		}
		if (!negate)
		{
			return flag;
		}
		return !flag;
	}

	private int GetUserDefinedPlaylistKey(Dictionary<int, ConditionUsage[]> conditionSets)
	{
		foreach (int key in conditionSets.Keys)
		{
			bool flag = true;
			ConditionUsage[] array = conditionSets[key];
			foreach (ConditionUsage conditionUsage in array)
			{
				flag &= GetIsConditionTrue(conditionUsage.Condition, conditionUsage.NegateArg, conditionUsage.ParameterArgs);
				if (!flag)
				{
					break;
				}
			}
			if (flag)
			{
				return key;
			}
		}
		return -1;
	}

	private void PrintParserError(string text, ushort lineNumber, string token)
	{
		Debug.Log((object)string.Format("{0} user-defined playlist: {1} at line {2}: {3}", new object[4] { "Dynamic Music", text, lineNumber, token }));
	}

	private void HandleLocationChange()
	{
		combatTaper = 0;
	}

	private void PlayCurrentTrack()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		uint num = daggerfallUnity.WorldTime.DaggerfallDateTime.ToClassicDaggerfallTime() / 1440;
		SongFiles song = dynamicSongPlayer.Song;
		int num2 = ((customPlaylists[currentPlaylist] != null) ? currentPlaylist : 17);
		bool flag = num2 != 17;
		if (flag && (currentCustomTrack != customPlaylists[currentPlaylist].CurrentTrack || customTrackQueued))
		{
			Playlist playlist = customPlaylists[num2];
			string track = ((resumeSeeker > 0f || (false && currentCustomTrack == customPlaylists[currentPlaylist].CurrentTrack)) ? playlist.CurrentTrack : playlist.GetNextTrack());
			GetDebuggingText(track, out debugPlaylistName, out debugSongName, currentPlaylist > 17);
			if (resumeIsEnabled && customPlaylists[currentPlaylist].HasFlags(Playlist.Flags.ResumePrevious))
			{
				resumeSeeker = dynamicSongPlayer.CurrentSecond;
				dynamicSongPlayer.Play(track);
			}
			else
			{
				if (num2 != resumePlaylist)
				{
					resumeSeeker = 0f;
				}
				dynamicSongPlayer.Play(track, resumeSeeker);
				resumeSeeker = 0f;
			}
			currentCustomTrack = playlist.CurrentTrack;
			dynamicSongPlayer.Song = (SongFiles)(-1);
			lastVanillaPlaylist = MusicPlaylist.None;
			customTrackQueued = false;
		}
		else if (num2 == 17)
		{
			if (num != lastGameDays || localPlayerGPS.CurrentLocationIndex != lastlocationIndex || (int)lastVanillaPlaylist != currentPlaylist)
			{
				song = GetSong((MusicPlaylist)currentPlaylist);
			}
			if (song == dynamicSongPlayer.Song)
			{
				lastGameDays = num;
				lastlocationIndex = localPlayerGPS.CurrentLocationIndex;
				lastVanillaPlaylist = (MusicPlaylist)currentPlaylist;
				return;
			}
			GetDebuggingText(song, out debugPlaylistName, out debugSongName);
			if (currentPlaylist != resumePlaylist)
			{
				resumeSeeker = 0f;
			}
			dynamicSongPlayer.Play(song, resumeSeeker);
			resumeSeeker = 0f;
			currentCustomTrack = string.Empty;
		}
		if (flag && dynamicSongPlayer.IsStoppedClip)
		{
			customTrackQueued = true;
		}
		lastGameDays = num;
		lastlocationIndex = localPlayerGPS.CurrentLocationIndex;
		lastVanillaPlaylist = (MusicPlaylist)currentPlaylist;
	}

	private MusicPlaylist GetMusicPlaylist(PlayerGPS localPlayerGPS, PlayerEnterExit playerEnterExit, PlayerWeather playerWeather)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Invalid comparison between Unknown and I4
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Expected I4, but got Unknown
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Expected I4, but got Unknown
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Expected I4, but got Unknown
		DaggerfallUnity instance = DaggerfallUnity.Instance;
		IUserInterfaceWindow topWindow = DaggerfallUI.UIManager.TopWindow;
		if ((int)gameManager.StateManager.CurrentState == 2 && !(topWindow is DaggerfallVidPlayerWindow) && !(topWindow is DaggerfallHUD))
		{
			if (topWindow is DaggerfallStartWindow || topWindow is DaggerfallUnitySaveGameWindow || (topWindow is DaggerfallPopupWindow && ((DaggerfallPopupWindow)((topWindow is DaggerfallPopupWindow) ? topWindow : null)).PreviousWindow is DaggerfallUnitySaveGameWindow) || topWindow is DaggerfallLoadClassicGameWindow)
			{
				return MusicPlaylist.MainMenu;
			}
			return MusicPlaylist.CharCreation;
		}
		if (!gameLoaded || !Object.op_Implicit((Object)(object)playerEnterExit) || !Object.op_Implicit((Object)(object)localPlayerGPS) || !Object.op_Implicit((Object)(object)instance) || topWindow is DaggerfallVidPlayerWindow)
		{
			return MusicPlaylist.None;
		}
		if (playerEntity.Arrested)
		{
			return MusicPlaylist.Court;
		}
		MusicEnvironment musicEnvironment = MusicEnvironment.Wilderness;
		if (!playerEnterExit.IsPlayerInside)
		{
			if (localPlayerGPS.IsPlayerInLocationRect)
			{
				LocationTypes currentLocationType = localPlayerGPS.CurrentLocationType;
				switch ((int)currentLocationType)
				{
				case 4:
				case 7:
				case 10:
				case 11:
				case 13:
					musicEnvironment = MusicEnvironment.DungeonExterior;
					break;
				case 12:
					musicEnvironment = MusicEnvironment.Graveyard;
					break;
				case 0:
				case 1:
				case 2:
				case 3:
__DFAMP_CONTINUE__