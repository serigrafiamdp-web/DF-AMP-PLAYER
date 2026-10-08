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

__DFAMP_CONTINUE__