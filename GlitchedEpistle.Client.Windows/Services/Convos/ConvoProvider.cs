using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using GlitchedPolygons.GlitchedEpistle.Client.Models;
using GlitchedPolygons.GlitchedEpistle.Client.Models.DTOs;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Convos;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Logging;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Constants;

using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Convos
{
    /// <summary>
    /// <see cref="Convo"/> provider. 
    /// Implements <see cref="GlitchedPolygons.GlitchedEpistle.Client.Services.Convos.IConvoProvider" />
    /// </summary>
    /// <seealso cref="GlitchedPolygons.GlitchedEpistle.Client.Services.Convos.IConvoProvider" />
    public class ConvoProvider : IConvoProvider
    {
        private readonly ILogger logger;
        private readonly Dictionary<string, Convo> convos = new Dictionary<string, Convo>(4);

        /// <summary>
        /// Gets all the convos currently loaded.
        /// </summary>
        public ICollection<Convo> GetAllConvos() => convos.Values.ToArray();

        /// <summary>
        /// Occurs when the <see cref="IConvoProvider" /> has finished loading/refreshing <see cref="T:GlitchedPolygons.GlitchedEpistle.Client.Models.Convo" />s into the <see cref="P:GlitchedPolygons.GlitchedEpistle.Client.Services.Convos.IConvoProvider.Convos" /> collection.
        /// </summary>
        public event EventHandler Loaded;

        /// <summary>
        /// Shall be raised when the <see cref="IConvoProvider" /> has finished
        /// saving the current state of the convos out to persistent memory.
        /// </summary>
        public event EventHandler Saved;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConvoProvider"/> class.
        /// </summary>
        public ConvoProvider(ILogger logger)
        {
            this.logger = logger;
            Load();
        }

        /// <summary>
        /// Gets or sets the <see cref="Convo"/> with the specified identifier.
        /// </summary>
        /// <param name="id">The convo identifier key.</param>
        /// <returns><see cref="Convo"/> instance if the id was found; <c>null</c> otherwise.</returns>
        public Convo this[string id]
        {
            get
            {
                if (convos.TryGetValue(id, out var convo))
                {
                    return convo;
                }
                return null;
            }
            set
            {
                if (!string.IsNullOrEmpty(id))
                {
                    convos[id] = value;
                }
            }
        }

        /// <summary>
        /// Saves all the currently loaded <see cref="Convo" />s in memory out to disk.
        /// </summary>
        /// <returns>Whether the saving procedure was successful or not.</returns>
        public void Save()
        {
            if (convos is null || convos.Count == 0)
            {
                return;
            }

            foreach (var kvp in convos)
            {
                if (kvp.Value is null)
                {
                    logger.LogError($"ConvoProvider::{nameof(Save)}: The convo dictionary entry \"{kvp.Key}\" has a null value and could thus not be saved!");
                }
                else if (kvp.Key != kvp.Value.Id)
                {
                    logger.LogError($"ConvoProvider::{nameof(Save)}: The dictionary key for the convo entry \"{kvp.Key}\" has a null value and could thus not be saved!");
                }
                else
                {
                    try
                    {
                        string path = Path.Combine(Paths.CONVOS_DIRECTORY, kvp.Key + ".json");
                        string json = JsonConvert.SerializeObject(kvp.Value, Formatting.Indented);
                        Directory.CreateDirectory(Path.Combine(Paths.CONVOS_DIRECTORY, kvp.Key));
                        File.WriteAllText(path, json);
                    }
                    catch (Exception e)
                    {
                        logger.LogError($"ConvoProvider::{nameof(Save)}: The convo \"{kvp.Key}\" could not be saved out to disk. Error message: {e.ToString()}");
                    }
                }
            }

            Saved?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Loads all <see cref="Convo" />s that are available inside the Epistle convos directory (see <see cref="Paths"/> for more details)
        /// into the <see cref="Convos"/> collection (the existing collection is overwritten!).
        /// </summary>
        public void Load()
        {
            try
            {
                var dir = new DirectoryInfo(Paths.CONVOS_DIRECTORY);
                if (!dir.Exists)
                {
                    dir.Create();
                }

                convos.Clear();
                FileInfo[] files = dir.GetFiles();

                for (int i = 0; i < files.Length; i++)
                {
                    string json = File.ReadAllText(files[i].FullName);
                    var convo = JsonConvert.DeserializeObject<Convo>(json);

                    if (convo != null && !string.IsNullOrEmpty(convo.Id))
                    {
                        convos[convo.Id] = convo;
                    }
                }

                Loaded?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception e)
            {
                logger.LogError($"ConvoProvider::{nameof(Load)}: One or more convos could not be loaded from disk. Error message: {e.ToString()}");
            }
        }
    }
}
