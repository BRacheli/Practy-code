using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HtmlSerializer
{
    internal class Selector
    {
        public string TagName { get; set; }
        public string Id { get; set; }
        public List<string> Classes { get; set; }
        public Selector Parent { get; set; }
        public Selector Child { get; set; }

        public Selector()
        {
            Classes = new List<string>();
        }


        public static Selector ParseToQuerySelector(string queryString)
        {
            var selectorsArr = queryString.Split(' ');
            Selector rootSelector = null;
            Selector currentSelector = null;

            foreach (var selector in selectorsArr)
            {
                var parts = Regex.Split(selector, @"(?=[.#])");
                var newSelector = new Selector();

                foreach (var part in parts)
                {
                    if (string.IsNullOrEmpty(part))
                        continue;
                    if (part.StartsWith("#"))
                    {
                        newSelector.Id = part.Remove(0, 1);
                    }
                    else if (part.StartsWith("."))
                        newSelector.Classes.Add(part.Remove(0, 1));
                    else if (HtmlHelper.Instance.HtmlTags.Contains(part) ||
                            HtmlHelper.Instance.HtmlVoidTags.Contains(part))
                    {
                        newSelector.TagName = part;
                    }
                }
                if (rootSelector == null)
                {
                    rootSelector = newSelector;
                    currentSelector = rootSelector;
                }
                else
                {
                    currentSelector.Child = newSelector;
                    newSelector.Parent = currentSelector;
                    currentSelector = newSelector;
                }

            }
            return rootSelector;
        }
    }
}
