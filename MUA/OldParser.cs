using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace MUA
{
    class Parser
    {
        // String line = "function(arg1,arg2)";
        private StringBuilder _line = new StringBuilder();

        [Flags]
        private enum Matchstate : byte { Function = 0x01, Commandlist = 0x02, String = 0x04 };
        private Matchstate _matchstates;
        private int _pos;
        private int _functioninvocations;
        private int _closingsquarebrackets;
        private int _closingparenthesis;
        private int _closingwibblies;
        Regex lowerCharASCII = new Regex(@"^([a-z])(.*$)");
        Regex upperCharASCII = new Regex(@"^([A-Z])(.*$)");
        Regex integerASCII = new Regex(@"^([0-9]+)(.*$)");
        Regex doubleASCII = new Regex(@"^(?:[0-9]+.[0-9]+|[0-9]+|.[0-9]+)(.*$)");
        Regex alphaNumericStringASCII = new Regex(@"^(\d|\w)+(.*$)");
        Regex functionNameASCII = new Regex(@"^([a-z](?:\d|\w|_)+)(\()(.*$)");
        Regex functionPlus = new Regex(@"^(.*?)(?<!(?:\\\\)*\\)(,|])(.*$)");
        Regex escapeChar = new Regex(@"^(\\\\|\\%|%\\|%%|(?:\\|%)[\[\]\(\)\{\}\<\>])(.*$)");
        Regex specialStartChar = new Regex(@".+([\\%\[\(])(.*$)");
        Regex evaluationReg = new Regex(@"^(\[)(.*$)");

        public Parser()
        {
            this._line.Insert(0, @"[functionname(arg1,arg2,arg3)]");
            _pos = -1;
            _functioninvocations = 0;
            _closingsquarebrackets = 0;
            _closingparenthesis = 0;
            _closingwibblies = 0;
            _matchstates = Matchstate.Function;
        }

        public Parser(String line)
        {
            _line.Insert(0, line);
            _pos = 0;
            _functioninvocations = 0;
            _closingsquarebrackets = 0;
            _closingparenthesis = 0;
            _closingwibblies = 0;
        }

        private Parser()
        {
            switch (_matchstates)
            {
                case Matchstate.Function:
                    functionMatching();
                    break;
                case Matchstate.Commandlist:
                    commandListMatching();
                    break;
                case Matchstate.String:
                    stringMatching();
                    break;
                default:
                    break;
            }
        }


        private GroupCollection eat(Regex reg = null)
        {
            if (reg == null)
                reg = stringCall;

            Match matches = reg.Match(_line.ToString());
            if (matches.Success)
            {
                int beforelast = matches.Groups.Count - 2;
                Group blgroup = matches.Groups[beforelast];

                Console.WriteLine("Matched: " + blgroup.Value);
                Console.WriteLine(matches.Success.ToString());
                _line.Remove(blgroup.Index, blgroup.Length);
                _pos = blgroup.Index;
                Console.WriteLine(_line.ToString());
                return matches.Groups;
            }
            else
                return null;
        }

        private bool _advance(int amount = 1)
        {
            _pos += amount;
            return (_pos < _line.Length);
        }

        private bool functionMatching()
        {
            GroupCollection arguments;
            GroupCollection result = eat(functionNameASCII);
            if (result == null) return false;

            _functioninvocations++;
            _closingparenthesis--;
            string fun_name = result[0].Value;
            string rest = result[1].Value;
            Stack<String> argumentstack = new Stack<String>();
            stringMatchingUntil(",)");


            return true;
        }

        private bool commandListMatching();

        private Char stringMatchingUntil(String until)
        {
            GroupCollection stringmatch;
            bool running = true;

            while ((stringmatch = eat(specialStartChar)) != null)
            {
                if (stringmatch == null)
                    return '0';
                // else
                foreach (char c in until)
                {
                    if (stringmatch[1].ToString()[0] == c)
                    {
                        return c;
                    }
                }

                _pos++;
            }
            while (running) ;
        }

        private bool _lowerChar()
        {
            int compare = _line[_pos].CompareTo('a');
            return (compare >= 0 && compare < 26);
        }

        private bool _upperChar()
        {
            int compare = _line[_pos].CompareTo('A');
            return (compare >= 0 && compare < 26);
        }

        private bool _int()
        {
            int compare = _line[_pos].CompareTo('0');
            return (compare >= 0 && compare < 10);
        }

        private bool stringEntity()
        {
            return _specialChar() || _unicodeChar();
        }

        private bool _specialChar()
        {
            // See about making this a switch, with the values of \ and %?
            if (_line[_pos].Equals('\\'))
            {
                if (_advance())
                {
                    if (_line[_pos + 1].Equals('(') || _line[_pos + 1].Equals(')') ||
                            _line[_pos + 1].Equals('[') || _line[_pos + 1].Equals(']') || _line[_pos + 1].Equals(','))
                    {
                        // REPLACE ITEM WITH CHARACTER
                        return true;
                    }
                }
                else
                {
                    // TODO: REPLACE WITH NOTHINGNESS
                }
            }
            else if (_line[_pos].Equals('%'))
            {
                return _percentChar();
            }
            return false;
        }

        /**
         * <remarks>TODO
         * </remarks> 
         */
        private bool _unicodeChar()
        {
            _line[_pos].ToString().IsNormalized();
            return true;
        }

        private bool _alphaNumericChar()
        {
            return (_int() || _lowerChar() || _upperChar());
        }

        /**
         * <remarks>TODO
         * </remarks> 
         */
        private bool _percentChar()
        {
            if (_alphaNumericChar())
            {
                return true;
            }
            else if (_pos >= _line.Length - 3)
            {
                // STUFF
                return true;
            }
            return false;
        }

    }
}
