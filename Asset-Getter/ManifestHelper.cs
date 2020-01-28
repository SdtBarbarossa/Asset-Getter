using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace Asset_Getter
{
    public class ManifestHelper
    {
        public List<string> resources { get; set; }
        public List<string> prefixes { get; set; }

        public ManifestHelper()
        {
            this.resources = new List<string>();
            this.prefixes = new List<string>();
        }

        public void ReadFromFile(string filepath)
        {
            var allLines = Regex.Replace(HttpUtility.UrlEncode(File.ReadAllText(filepath, Encoding.ASCII)), @"%[\da-fA-F]{2}", " ");
            allLines = Regex.Replace(allLines, @"[A-Z]", "");
            allLines = Regex.Replace(allLines, @"[+]", "");
            allLines = Regex.Replace(allLines, @"[(]", "");
            allLines = Regex.Replace(allLines, @"[)]", "");
            allLines = Regex.Replace(allLines, @"[!]", "");
            allLines = Regex.Replace(allLines, @"[?]", "");

            foreach (string text in allLines.Split(' '))
            {
                var temptext = text;

                if (!string.IsNullOrWhiteSpace(temptext) && temptext.Contains("_") && !temptext.EndsWith("_") && !temptext.Contains("."))
                {
                    if (!Regex.Match(temptext, @"^[a-z]").Success)
                    {
                        temptext = temptext.Substring(1);
                    }

                    if(temptext.Length == 1)
                    {
                        continue;
                    }

                    resources.Add(temptext);
                    var prefix = temptext.Split('_').First();
                    prefixes.Add(prefix);
                }
            }

            this.prefixes = prefixes.Distinct().ToList();
            this.resources = resources.Distinct().ToList();

            prefixes.Sort();
            resources.Sort();
        }
    }
}
