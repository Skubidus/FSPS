using FSPSLibrary;
using FSPSLibrary.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace FSPSWinUI.Storage
{
	public class JsonProfileStore : IProfileStore
	{
		private readonly string _filePath;
		private readonly JsonSerializerOptions _opts = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true };

		public JsonProfileStore(string filePath)
		{
			_filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
		}

		public async Task<IEnumerable<ProfileModel>> LoadAsync()
		{
			if (!File.Exists(_filePath))
			{
				return Array.Empty<ProfileModel>();
			}

			try
			{
				using var stream = File.OpenRead(_filePath);
				var doc = await JsonSerializer.DeserializeAsync<ProfileFile>(stream, _opts).ConfigureAwait(false);
				return doc?.Profiles ?? Array.Empty<ProfileModel>();
			}
			catch (Exception)
			{
				// Corrupt or unreadable file — treat as empty to avoid crashing the app
				return Array.Empty<ProfileModel>();
			}
		}

		public async Task SaveAsync(IEnumerable<ProfileModel> profiles)
		{
			var directory = Path.GetDirectoryName(_filePath) ?? AppContext.BaseDirectory;
			Directory.CreateDirectory(directory);

			var tempFile = Path.Combine(directory, Path.GetFileName(_filePath) + ".tmp");
			var doc = new ProfileFile { Version = 1, Profiles = profiles };
			try
			{
				using (var stream = File.Create(tempFile))
				{
					await JsonSerializer.SerializeAsync(stream, doc, _opts).ConfigureAwait(false);
				}

				// Atomic replace: if destination exists, replace; otherwise move
				if (File.Exists(_filePath))
				{
					File.Replace(tempFile, _filePath, null);
				}
				else
				{
					File.Move(tempFile, _filePath);
				}
			}
			catch
			{
				if (File.Exists(tempFile))
				{
					try { File.Delete(tempFile); } catch { }
				}
				throw;
			}
		}

		private sealed class ProfileFile
		{
			public int Version { get; set; }
			public IEnumerable<ProfileModel> Profiles { get; set; } = Array.Empty<ProfileModel>();
		}
	}
}
