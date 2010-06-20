namespace Client.Audio
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Threading;
	using System.IO;

	using OpenTK.Audio;
	using OpenTK.Audio.OpenAL;

	public class Player
	{
		private AudioContext _context;

		public Player()
		{
			_context = new AudioContext();
			
			for (int i = 0; i < 10; ++i)
				_freeSources.Enqueue(AL.GenSource());
		}

		// Loads a wave/riff audio file.
		private byte[] LoadWave(Stream stream, out int channels, out int bits, out int rate)
		{
			if (stream == null)
				throw new ArgumentNullException("stream");
			
			using (BinaryReader reader = new BinaryReader(stream))
			{
				// RIFF header
				string signature = new string(reader.ReadChars(4));
				if (signature != "RIFF")
					throw new NotSupportedException("Specified stream is not a wave file.");
				
				int riff_chunck_size = reader.ReadInt32();
				
				string format = new string(reader.ReadChars(4));
				if (format != "WAVE")
					throw new NotSupportedException("Specified stream is not a wave file.");
				
				// WAVE header
				string format_signature = new string(reader.ReadChars(4));
				if (format_signature != "fmt ")
					throw new NotSupportedException("Specified wave file is not supported.");
				
				int format_chunk_size = reader.ReadInt32();
				int audio_format = reader.ReadInt16();
				int num_channels = reader.ReadInt16();
				int sample_rate = reader.ReadInt32();
				int byte_rate = reader.ReadInt32();
				int block_align = reader.ReadInt16();
				int bits_per_sample = reader.ReadInt16();
				
				string data_signature = new string(reader.ReadChars(4));
				if (data_signature != "data")
					throw new NotSupportedException("Specified wave file is not supported.");
				
				int data_chunk_size = reader.ReadInt32();
				
				channels = num_channels;
				bits = bits_per_sample;
				rate = sample_rate;
				
				return reader.ReadBytes((int)reader.BaseStream.Length);
			}
		}

		private ALFormat GetSoundFormat(int channels, int bits)
		{
			switch (channels)
			{
				case 1:
					return bits == 8 ? ALFormat.Mono8 : ALFormat.Mono16;
				case 2:
					return bits == 8 ? ALFormat.Stereo8 : ALFormat.Stereo16;
				default:
					throw new NotSupportedException("The specified sound format is not supported.");
			}
		}

		private Dictionary<string, int> _loadedBuffers = new Dictionary<string, int>();
		private Queue<int> _freeSources = new Queue<int>();
		private List<int> _playingSources = new List<int>();

		public int Preload(string filename)
		{
			string path = Path.Combine("data", filename);
			
			int channels, bits_per_sample, sample_rate;
			byte[] sound_data = LoadWave(File.Open(path, FileMode.Open), out channels, out bits_per_sample, out sample_rate);
			
			int buffer = AL.GenBuffer();
			AL.BufferData(buffer, GetSoundFormat(channels, bits_per_sample), sound_data, sound_data.Length, sample_rate);
			
			_loadedBuffers[filename] = buffer;
			return buffer;
		}

		public void Play(string filename)
		{
			if (_freeSources.Count <= 0)
				return;
			
			int source = _freeSources.Dequeue();
			int buffer;
			
			if (!_loadedBuffers.TryGetValue(filename, out buffer))
			{
				buffer = Preload(filename);
			}
			
			AL.Source(source, ALSourcei.Buffer, buffer);
			AL.SourcePlay(source);
			_playingSources.Add(source);
		}

		public void Update()
		{
			List<int> sourcesToFree = new List<int>();
			foreach (int source in _playingSources)
			{
				int state;
				AL.GetSource(source, ALGetSourcei.SourceState, out state);
				
				if ((ALSourceState)state == ALSourceState.Stopped)
				{
					sourcesToFree.Add(source);
				}
			}
			
			foreach (int source in sourcesToFree)
			{
				AL.SourceStop(source);
				_playingSources.Remove(source);
				_freeSources.Enqueue(source);
			}
		}
		
	}
}

