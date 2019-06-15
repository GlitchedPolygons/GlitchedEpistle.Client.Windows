using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using GlitchedPolygons.GlitchedEpistle.Client.Models;
using GlitchedPolygons.GlitchedEpistle.Client.Models.DTOs;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Convos;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Constants;

using Newtonsoft.Json;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Convos
{
    /// <summary>
    /// <see cref="Convo"/> provider. 
    /// Implements <see cref="GlitchedPolygons.GlitchedEpistle.Client.Services.Convos.IConvoProvider" />
    /// </summary>
    /// <seealso cref="GlitchedPolygons.GlitchedEpistle.Client.Services.Convos.IConvoProvider" />
    public class ConvoProvider : IConvoProvider
    {
        private List<Convo> convos = new List<Convo>(4);
        public ICollection<Convo> Convos => convos;

        /// <summary>
        /// Occurs when the <see cref="IConvoProvider" /> has finished loading/refreshing <see cref="T:GlitchedPolygons.GlitchedEpistle.Client.Models.Convo" />s into the <see cref="P:GlitchedPolygons.GlitchedEpistle.Client.Services.Convos.IConvoProvider.Convos" /> collection.
        /// </summary>
        public event EventHandler Loaded;

        /// <summary>
        /// Shall be raised when the <see cref="IConvoProvider" /> has finished
        /// saving the current state of the <see cref="Convos" /> out to persistent memory.
        /// </summary>
        public event EventHandler Saved;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConvoProvider"/> class.
        /// </summary>
        public ConvoProvider()
        {
            Load();
        }

        /// <summary>
        /// Gets the <see cref="Convo"/> with the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns><see cref="Convo"/> instance if the id was found; <c>null</c> otherwise.</returns>
        public Convo this[string id]
        {
            get
            {
                if (convos is null || convos.Count == 0)
                {
                    return null;
                }

                foreach (var convo in convos)
                {
                    if (convo is null || convo.Id != id)
                    {
                        continue;
                    }
                    return convo;
                }

                return null;
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

            void SerializeAndWrite()
            {
                for (int i = 0; i < convos.Count; i++)
                {
                    var convo = convos[i];
                    if (convo is null || string.IsNullOrEmpty(convo.Id))
                    {
                        continue;
                    }
                    Directory.CreateDirectory(Path.Combine(Paths.CONVOS_DIRECTORY, convo.Id));
                    File.WriteAllText(Path.Combine(Paths.CONVOS_DIRECTORY, convo.Id + ".json"), JsonConvert.SerializeObject(convo, Formatting.Indented));
                }

                Saved?.Invoke(this, EventArgs.Empty);
            }

            try
            {
                SerializeAndWrite();
            }
            catch (Exception)
            {
                System.Threading.Thread.Sleep(100);
                try { SerializeAndWrite(); } catch (Exception) { }
            }
        }

        /// <summary>
        /// Loads all <see cref="Convo" />s that are available inside the Epistle convos directory (see <see cref="Paths"/> for more details)
        /// into the <see cref="Convos"/> collection (the existing collection is overwritten!).
        /// </summary>
        public void Load()
        {
            DirectoryInfo dir = new DirectoryInfo(Paths.CONVOS_DIRECTORY);
            if (!dir.Exists)
            {
                dir = Directory.CreateDirectory(Paths.CONVOS_DIRECTORY);
            }

            convos.Clear();

            FileInfo[] files = dir.GetFiles();
            for (int i = 0; i < files.Length; i++)
            {
                var convo = JsonConvert.DeserializeObject<Convo>(File.ReadAllText(files[i].FullName));
                if (convo != null)
                {
                    convos.Add(convo);
                }
            }

            convos = convos.OrderBy(c => c.Name).ToList();
            Loaded?.Invoke(this, EventArgs.Empty);
        }
    }
}
