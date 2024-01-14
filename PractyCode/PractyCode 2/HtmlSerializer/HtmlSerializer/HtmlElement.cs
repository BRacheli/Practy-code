using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HtmlSerializer
{
    public class HtmlElement
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<string> Attributes { get; set; }
        public List<string> Classes { get; set; }
        public string InnerHtml { get; set; }
        public HtmlElement Parent { get; set; }
        public List<HtmlElement> Children { get; set; }

        public HtmlElement()
        {
            Attributes = new List<string>();
            Classes = new List<string>();
            Children= new List<HtmlElement>();
        }

        public IEnumerable<HtmlElement> Descendants()
        {
            Queue<HtmlElement> queue = new Queue<HtmlElement>();
            queue.Enqueue(this);

            while (queue.Count > 0)
            {
                HtmlElement currentElement = queue.Dequeue();
                yield return currentElement;

                foreach (var child in currentElement.Children)
                {
                    queue.Enqueue(child);
                }

            }
        }

        public IEnumerable<HtmlElement> Ancestors()
        {
            HtmlElement currentElement = this;

            while (currentElement != null)
            {
                currentElement = currentElement.Parent;
                yield return currentElement;
            }
        }

        public override string ToString()
        {
            string str = "";
            if (this == null)
                return str;
            str = "<";
            if(this.Name!=null)
                str += this.Name;
            if(this.Id!=null) 
                str +=(" id="+ this.Id);
            if(this.Attributes!=null&& this.Attributes.Count > 0)
            {
                str += " attributes: ";
                str += string.Join(" ", Attributes);

            }
            if (this.Classes != null && this.Classes.Count > 0)
            {
                str += " classes: ";
                foreach (var c in this.Classes)
                {
                    str += ("." + c + " ");
                }
            }
            str += ">";
            return str;
        }
    }
}
