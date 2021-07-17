// -----------------------------------------------------------------------
// <copyright file="KnownHashIndex.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace PD2Map
{
    using System.Collections.Generic;
    using System.IO;

    using DieselUnit.Utils;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class KnownHashIndex
    {
        private static readonly Dictionary<ulong, string> KnownHashes = new Dictionary<ulong, string>();

        public static void Load(string path)
        {
            using (var fs = new StreamReader(path))
            {
                string line = fs.ReadLine();
                    while (line != null)
                    {
                        
                        ulong hash = Hash64.HashString(line);
                        KnownHashes[hash] = line;
                        line = fs.ReadLine();
                    }
                    fs.Close();
            }
        }

        public static string GetKnownValue(ulong hash)
        {
            return KnownHashes.ContainsKey(hash) ? KnownHashes[hash] : null;
        }
    }
}
