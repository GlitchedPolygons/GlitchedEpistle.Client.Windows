using System.IO;
using System.Collections.Generic;

using GlitchedPolygons.GlitchedEpistle.Client.Models;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Convos;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Constants;

using Newtonsoft.Json;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Convos
{
    public class ConvoProvider : IConvoProvider
    {
        private readonly List<Convo> convos = new List<Convo>(8);

        public ICollection<Convo> Convos => convos;

        public ConvoProvider()
        {
            var dir = new DirectoryInfo(Paths.CONVOS_DIRECTORY);
            if (!dir.Exists)
            {
                dir = Directory.CreateDirectory(Paths.CONVOS_DIRECTORY);
            }
            
            FileInfo[] files = dir.GetFiles();
            if (files.Length > 0)
            {
                for (int i = 0; i < files.Length; i++)
                {
                    var convo = JsonConvert.DeserializeObject<Convo>(File.ReadAllText(files[i].FullName));
                    if (convo is null)
                        continue;
                    convos.Add(convo);
                }
            }
        }

        public Convo this[string id]
        {
            get
            {
                for (int i = 0; i < convos.Count; i++)
                {
                    var convo = convos[i];
                    if (convo is null || convo.Id != id)
                        continue;
                    return convo;
                }
                return null;
            }
        }
    }
}

