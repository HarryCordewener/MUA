namespace MUA
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class Markup_String
    {
        Markup_String left;
        Markup_String right;
        UInt32 weight;
        private Markup markup { get; set; } // Placeholder for an actual Markup Class.
        private String mystring { get; set; } // Only applicable in the case of a 'No Markup()' case.

        public Markup_String()
        {
            substrings = new List<Markup_String>();
            markup = new Markup(); // No Markup
            weight = 0;
        }

        public Markup_String(ref String str)
        {
            substrings = new List<Markup_String>();
            markup = new Markup(); // No Markup
            mystring = str;
        }

        public Markup_String(ref Markup_String str, Markup mark)
        {
            substrings = new List<Markup_String>();
            substrings.Add(str);
            markup = mark;
        }

        public Markup_String append(ref Markup_String str)
        {
            substrings.Add(str);
            return this;
        }

        public Markup_String prepend(ref Markup_String str)
        {
            substrings.Insert(0, str);
            return this;
        }

        public Markup_String insert(ref Markup_String str, int i)
        {
            substrings.Insert(i, str);
            return this;
        }

        public Markup_String delete(ref Markup_String str)
        {
            substrings.Remove(str);
            return this;
        }

        public Markup_String deleteAt(int i)
        {
            substrings.RemoveAt(i);
            return this;
        }

        public Markup_String flatten()
        {
            Markup_String flatmark = new Markup_String();
            if (!markup.Markup())
            { // Basecase

            }
            else
            {
                foreach (Markup_String s in substrings)
                {
                    s.flatten();
                }
            }

        }
    }
}
