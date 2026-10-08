using System;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Security;
using DaggerfallWorkshop;
using DaggerfallWorkshop.AudioSynthesis;
using DaggerfallWorkshop.AudioSynthesis.Midi;
using DaggerfallWorkshop.AudioSynthesis.Sequencer;
using DaggerfallWorkshop.AudioSynthesis.Synthesis;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using DaggerfallWorkshop.Utility.AssetInjection;
using UnityEngine;

namespace DynamicMusic;

[RequireComponent(typeof(AudioSource))]
public class DynamicSongPlayer : MonoBehaviour
{
	public delegate void OnSongEndHandler();

	public class MyMemoryFile : IResource
	{
		private byte[] file;

		private string fileName;

		public MyMemoryFile(byte[] file, string fileName)
		{
			this.file = file;
			this.fileName = fileName;
		}

		public string GetName()
		{
			return fileName;
		}

		public bool DeleteAllowed()
		{
			return false;
		}

		public bool ReadAllowed()
		{
			return true;
		}

		public bool WriteAllowed()
		{
			return false;
		}

		public void DeleteResource()
		{
		}

		public Stream OpenResourceForRead()
		{
			return new MemoryStream(file);
		}

		public Stream OpenResourceForWrite()
		{
			return null;
		}
	}

	private const string sourceFolderName = "SoundFonts";

	private const string defaultSoundFontFilename = "TimGM6mb.sf2";

	private const int sampleRate = 48000;

	private const int polyphony = 100;

	[NonSerialized]
	[HideInInspector]
	public bool IsSequencerPlaying;

	public bool ShowDebugString;

	[Range(0f, 10f)]
	public float Gain = 5f;

	public string SongFolder = "Songs/";

	public SongFiles Song = (SongFiles)(-1);

	private Synthesizer midiSynthesizer;

	private MidiFileSequencer midiSequencer;

	private float[] sampleBuffer = new float[0];

	private int channels;

	private int bufferLength;

	private int numBuffers;

	private bool playEnabled;

	private float oldGain;

	private bool clipStarted;

	private AudioClip streamedSong;

	private AudioClip oldSong;

	public string ModSignature { private get; set; }

	public AudioSource AudioSource { get; private set; }

	public bool IsImported { get; set; }

	public bool IsAudioSourcePlaying
	{
		get
		{
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Invalid comparison between Unknown and I4
			if (!AudioSource.isPlaying)
			{
				if (Object.op_Implicit((Object)(object)AudioSource.clip))
				{
					return (int)AudioSource.clip.loadState == 1;
				}
				return false;
			}
			return true;
		}
	}

	public bool IsPlaying
	{
		get
		{
			if (!IsAudioSourcePlaying)
			{
				if (midiSequencer != null)
				{
					return midiSequencer.IsPlaying;
				}
				return false;
			}
			return true;
		}
	}

	public bool IsStoppedClip
	{
		get
		{
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Invalid comparison between Unknown and I4
			if (clipStarted && Object.op_Implicit((Object)(object)AudioSource.clip) && (int)AudioSource.clip.loadState == 2)
			{
				return !AudioSource.isPlaying;
			}
			return false;
		}
	}

	public bool IsStinging { get; set; }

	public float CurrentSecond
	{
		get
		{
			if (!midiSequencer.IsPlaying)
			{
				return AudioSource.time;
			}
			return midiSequencer.CurrentTime / midiSequencer.Synth.SampleRate;
		}
	}

	public bool HasPlayedOnce { get; private set; }

	public static event OnSongEndHandler OnSongEnd;

	private void Start()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected Obj, but got Unknown
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected Obj, but got Unknown
		AudioSource = ((Component)this).GetComponent<AudioSource>();
		AudioSource.volume = 0f;
		InitSynth();
		DaggerfallVidPlayerWindow.OnVideoStart += DaggerfallVidPlayerWindow_OnVideoStart;
		DaggerfallVidPlayerWindow.OnVideoEnd += DaggerfallVidPlayerWindow_OnVideoEnd;
	}

	[HandleProcessCorruptedStateExceptions]
	[SecurityCritical]
	private void Update()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Invalid comparison between Unknown and I4
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		if (!IsImported)
		{
			if (midiSequencer != null)
			{
				IsSequencerPlaying = midiSequencer.IsPlaying;
				Gain = AudioSource.volume * 5f;
				if ((int)Song != -1 && !midiSequencer.IsPlaying)
				{
					PlaySequencer(Song);
				}
			}
		}
		else
		{
			if (IsAudioSourcePlaying)
			{
				return;
			}
			if (AudioSource.time >= AudioSource.clip.length)
			{
				HasPlayedOnce = true;
			}
			StopSequencer();
			if (Object.op_Implicit((Object)(object)AudioSource.clip) && !IsStinging)
			{
				AudioSource.Play();
			}
			clipStarted = true;
			if (Object.op_Implicit((Object)(object)oldSong))
			{
				oldSong.UnloadAudioData();
				try
				{
					Object.Destroy((Object)(object)oldSong);
				}
				catch (AccessViolationException)
				{
					Debug.Log((object)(ModSignature + ": Caught/ignored access violation when destroying old audio clip."));
				}
			}
		}
	}

	public void Play()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		if ((int)Song != -1)
		{
			Play(Song);
		}
	}

	public void Play(SongFiles song, float timeSeek = 0f)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		if (InitSynth())
		{
			Stop();
			AudioClip clip = default;
			if (IsImported = SoundReplacement.TryImportSong(song, ref clip))
			{
				Song = song;
				AudioSource.clip = clip;
				AudioSource.time = timeSeek;
				clipStarted = false;
			}
			else
			{
				PlaySequencer(song, (int)timeSeek);
			}
			AudioSource.loop = true;
			HasPlayedOnce = false;
		}
	}

	public void Play(string track, float timeSeek = 0f)
	{
		Stop();
		oldSong = streamedSong;
		if (IsImported = TryLoadSong(track, out streamedSong))
		{
			AudioSource.clip = streamedSong;
			AudioSource.time = timeSeek;
			AudioSource.loop = false;
			clipStarted = false;
		}
		HasPlayedOnce = false;
	}

	public void Stop()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		if (InitSynth())
		{
			if (IsImported)
			{
				IsImported = false;
				AudioSource.Stop();
				AudioSource.clip = null;
			}
			Song = (SongFiles)(-1);
			StopSequencer();
		}
	}

	public void StopSequencer()
	{
		if (midiSequencer.IsPlaying)
		{
			midiSequencer.Stop();
			midiSynthesizer.NoteOffAll(true);
			midiSynthesizer.ResetSynthControls();
			midiSynthesizer.ResetPrograms();
			playEnabled = false;
		}
	}

	private bool InitSynth()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Expected Obj, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected Obj, but got Unknown
		if ((Object)(object)AudioSource == (Object)null)
		{
			LogErrorMessage("Could not find AudioSource component.");
			return false;
		}
		if (midiSynthesizer == null)
		{
			if (((object)AudioSettings.driverCapabilities/*cast due to constrained. prefix*/).ToString() == "Mono")
			{
				channels = 1;
			}
			else
			{
				channels = 2;
			}
			AudioSettings.GetDSPBufferSize(ref bufferLength, ref numBuffers);
			midiSynthesizer = new Synthesizer(48000, channels, bufferLength / numBuffers, numBuffers, 100);
			string text = DaggerfallUnity.Settings.SoundFont;
			byte[] array = LoadBank(text);
			if (array == null)
			{
				array = LoadDefaultSoundFont();
				text = "TimGM6mb.sf2";
				Debug.LogFormat("Using default SoundFont {0}", new object[1] { "TimGM6mb.sf2" });
			}
			else
			{
				Debug.LogFormat("Trying custom SoundFont {0}", new object[1] { text });
			}
			if (array == null)
			{
				return false;
			}
			midiSynthesizer.LoadBank((IResource)(object)new MyMemoryFile(array, text));
			midiSynthesizer.ResetSynthControls();
		}
		if (midiSequencer == null)
		{
			midiSequencer = new MidiFileSequencer(midiSynthesizer);
		}
		if (midiSynthesizer == null || midiSequencer == null)
		{
			LogErrorMessage("Failed to init synth.");
			return false;
		}
		return true;
	}

	private string EnumToFilename(SongFiles song)
	{
		return ((object)song/*cast due to constrained. prefix*/).ToString().Remove(0, "song_".Length) + ".mid";
	}

	private byte[] LoadBank(string filename)
	{
		if (string.IsNullOrEmpty(filename))
		{
			return null;
		}
		string text = Path.Combine(Path.Combine(Application.streamingAssetsPath, "SoundFonts"), filename);
		if (!File.Exists(text))
		{
			LogErrorMessage("Could not find file '" + text + "', falling back to default soundfont TimGM6mb.sf2.");
			return null;
		}
		return File.ReadAllBytes(text);
	}

	private byte[] LoadDefaultSoundFont()
	{
		TextAsset val = Resources.Load<TextAsset>("TimGM6mb.sf2");
		if ((Object)(object)val != (Object)null)
		{
			return val.bytes;
		}
		LogErrorMessage("Bank file 'TimGM6mb.sf2' not found.");
		return null;
	}

	private void PlaySequencer(SongFiles song, int timeSeek = 0)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected Obj, but got Unknown
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		string text = EnumToFilename(song);
		byte[] array = LoadSong(text);
		if (array != null)
		{
			MidiFile val = new MidiFile((IResource)(object)new MyMemoryFile(array, text));
			if (midiSequencer.LoadMidi(val))
			{
				TimeSpan timeSpan = new TimeSpan(0, 0, timeSeek);
				midiSequencer.Seek(timeSpan);
				midiSequencer.Play();
				Song = song;
				playEnabled = true;
				IsSequencerPlaying = true;
			}
		}
	}

	private byte[] LoadSong(string filename)
	{
		byte[] result = default;
		if (SoundReplacement.TryImportMidiSong(filename, ref result))
		{
			return result;
		}
		TextAsset val = Resources.Load<TextAsset>(Path.Combine(SongFolder, filename));
		if ((Object)(object)val != (Object)null)
		{
			return val.bytes;
		}
		LogErrorMessage("Song file '" + filename + "' not found.");
		return null;
	}

	private void DaggerfallVidPlayerWindow_OnVideoStart()
	{
		oldGain = Gain;
		Gain = 0f;
	}

	private void DaggerfallVidPlayerWindow_OnVideoEnd()
	{
		Gain = oldGain;
	}

	private bool TryLoadSong(string path, out AudioClip audioClip)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected Obj, but got Unknown
		if (File.Exists(path))
		{
			WWW val = new WWW("file://" + path);
			audioClip = val.GetAudioClip(true, true);
			return (Object)(object)audioClip != (Object)null;
		}
		audioClip = null;
		return false;
	}

	private void LogErrorMessage(string errorMessage)
	{
		DaggerfallUnity.LogMessage("DynamicSongPlayer: " + errorMessage, false);
	}

	private void OnAudioFilterRead(float[] data, int channels)
	{
		if (!playEnabled || midiSynthesizer == null || midiSequencer == null)
		{
			return;
		}
		if (sampleBuffer.Length != midiSynthesizer.WorkingBufferSize)
		{
			sampleBuffer = new float[midiSynthesizer.WorkingBufferSize];
		}
		try
		{
			if (midiSequencer.IsMidiLoaded && midiSequencer.IsPlaying)
			{
				midiSequencer.FillMidiEventQueue();
				midiSynthesizer.GetNext(sampleBuffer);
				for (int i = 0; i < data.Length; i++)
				{
					data[i] = sampleBuffer[i] * Gain;
				}
			}
		}
		catch (Exception)
		{
		}
	}

	public DynamicSongPlayer()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
	}
}
