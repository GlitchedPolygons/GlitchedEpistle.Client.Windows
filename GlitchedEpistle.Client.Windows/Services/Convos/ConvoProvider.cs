using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;

using GlitchedPolygons.GlitchedEpistle.Client.Models;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Convos;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Constants;

using Newtonsoft.Json;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Convos
{
    public class ConvoProvider : IConvoProvider
    {
        private readonly List<Convo> convos = new List<Convo>(4);
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
                var convosBag = new ConcurrentBag<Convo>();
                Parallel.For(0, files.Length, i =>
                {
                    var convo = JsonConvert.DeserializeObject<Convo>(File.ReadAllText(files[i].FullName));
                    if (convo != null)
                    {
                        convosBag.Add(convo);
                    }
                });
                convos = convosBag.ToList();
            }
        }

        public Convo this[string id]
        {
            get
            {
                if (convos is null || convos.Count == 0)
                    return null;

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
    }
}

