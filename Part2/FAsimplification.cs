using System.IO;
using System.Linq;
using System.Text.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace Project
{

    // class to convert json file to FA class that contain all property moslty as string
    public class FAS
    {
        public FAS()
        { }
        public FAS(FA f)
        {
            states = MakeString(f.states);
            input_symbols = MakeString(f.input_symbols); ;
            initial_state = f.initial_state;
            final_states = MakeString(f.final_states);
            transitions = new Dictionary<string, Dictionary<string, string>>();
            foreach (var t in f.transitions)
            {
                string state = t.Key.Replace(",", "");
                transitions[state] = new Dictionary<string, string>();
                foreach (var x in t.Value)
                {
                    transitions[state][x.Key] = x.Value.Replace(",", "");
                }
            }
        }

        public string states { get; set; }
        public string input_symbols { get; set; }
        public Dictionary<string, Dictionary<string, string>> transitions { get; set; }
        public string initial_state { get; set; }
        public string final_states { get; set; }

        public string MakeString(List<string> s) => "{'" + string.Join("','", s.Select(c => c.Replace(",", ""))) + "'}";
    }

    //class To convert a NFAS class to standard FA class for exaple by split string and convert it list
    public class FA
    {
        public FA()
        {
            states = new List<string>();
            input_symbols = new List<string>();
            final_states = new List<string>();
            transitions = new Dictionary<string, Dictionary<string, string>>();
        }
        public FA(FAS f)
        {
            this.states = SplitString(f.states);
            this.input_symbols = SplitString(f.input_symbols);
            this.initial_state = f.initial_state;
            this.final_states = SplitString(f.final_states);
            this.transitions = new Dictionary<string, Dictionary<string, string>>();
            foreach (var transiton in f.transitions)
            {
                this.transitions[transiton.Key] = new Dictionary<string, string>();
                foreach (var symbol in transiton.Value)
                {
                    if (f.transitions[transiton.Key][symbol.Key].Contains("{"))
                        this.transitions[transiton.Key][symbol.Key] = f.transitions[transiton.Key][symbol.Key].
                        Substring(2, f.transitions[transiton.Key][symbol.Key].Length - 4);
                    else
                        this.transitions[transiton.Key][symbol.Key] = f.transitions[transiton.Key][symbol.Key];
                }
            }
        }

        public List<string> states { get; set; }
        public List<string> input_symbols { get; set; }
        public Dictionary<string, Dictionary<string, string>> transitions { get; set; }
        public string initial_state { get; set; }
        public List<string> final_states { get; set; }

        public List<string> SplitString(string s) => s.Substring(2, s.Length - 4).Split("','").ToList();
    }


    public class Simplification
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Please Enter your DFA path:");
            var inpath = Console.ReadLine();
            Console.WriteLine("Please Enter your Simplified DFA path to save:");
            var outpath = Console.ReadLine();
            var DFAS = new FAS();
            DFAS = JsonSerializer.Deserialize<FAS>(File.ReadAllText(inpath));
            FA DFA = new FA(DFAS);
            FA SimpleDFA = DFASimplify(DFA);
            FAS newDFA = new FAS(SimpleDFA);
            File.WriteAllText(outpath, JsonSerializer.Serialize(newDFA));
        }

        public static FA DFASimplify(FA DFA)
        {
            FA SimpleDFA = new FA();
            SimpleDFA.initial_state = DFA.initial_state;
            SimpleDFA.input_symbols = DFA.input_symbols;
            SimpleDFA.transitions = DFA.transitions;
            SimpleDFA.final_states = DFA.final_states;
            Dictionary<string, bool> visits = new Dictionary<string, bool>();
            foreach (var s in DFA.states)
                visits[s] = false;
            HashSet<string> StateSet = new HashSet<string>();
            FindStateSet(DFA.transitions, DFA.initial_state, visits, ref StateSet);
            SimpleDFA.states = StateSet.OrderBy(s => s).ToList();
            foreach (var s in DFA.states)
            {
                if (!SimpleDFA.states.Contains(s))
                    SimpleDFA.transitions.Remove(s);
            }
            for (int i = 0; i < DFA.final_states.Count; i++)
            {
                string f = DFA.final_states[i];
                if (!SimpleDFA.states.Contains(f))
                    SimpleDFA.final_states.Remove(f);
            }
            bool moreSimple = true;
            while (moreSimple)
            {
                moreSimple = false;
                bool b = false;
                foreach (var s1 in SimpleDFA.states)
                {
                    foreach (var s2 in SimpleDFA.states)
                    {
                        if (s1 != s2)
                        {
                            if (Issame(SimpleDFA, s1, s2))
                            {
                                string newstate = s1.Last()<s2.Last()?s1+s2:s2+s1;
                                SimpleDFA.transitions.Remove(s2);
                                SimpleDFA.transitions[newstate] = SimpleDFA.transitions[s1];
                                SimpleDFA.transitions.Remove(s1);
                                SimpleDFA.states.Remove(s2);
                                SimpleDFA.states.Add(newstate);
                                SimpleDFA.states.Remove(s1);
                                var temp_transition = SimpleDFA.transitions;
                                foreach (var key in temp_transition.Keys)
                                {
                                    foreach (var sym in temp_transition[key].Keys)
                                    {
                                        if (temp_transition[key][sym] == s2 || temp_transition[key][sym] == s1)
                                            SimpleDFA.transitions[key][sym] = newstate;
                                    }
                                }
                                if (SimpleDFA.initial_state == s1 || SimpleDFA.initial_state == s2)
                                {
                                    SimpleDFA.initial_state = newstate;
                                }
                                if (SimpleDFA.final_states.Contains(s1) && SimpleDFA.final_states.Contains(s2))
                                {
                                    SimpleDFA.final_states.Remove(s1);
                                    SimpleDFA.final_states.Remove(s2);
                                    SimpleDFA.final_states.Add(newstate);

                                }
                                b = true;
                                moreSimple = true;
                            }
                        }
                        if (b)
                            break;
                    }
                    if (b)
                        break;
                }
            }
            return SimpleDFA;
        }
        private static bool Issame(FA SimpleDFA, string s1, string s2)
        {
            if (SimpleDFA.final_states.Contains(s1) && !SimpleDFA.final_states.Contains(s2))
                return false;
            if (SimpleDFA.final_states.Contains(s2) && !SimpleDFA.final_states.Contains(s1))
                return false;
            foreach (var symbol in SimpleDFA.input_symbols)
            {
                if (SimpleDFA.transitions[s1][symbol] != SimpleDFA.transitions[s2][symbol]
                && (!$"{s1}{s2}".Contains(SimpleDFA.transitions[s1][symbol]) || !$"{s1}{s2}".Contains(SimpleDFA.transitions[s2][symbol])))
                    return false;
            }
            return true;
        }

        private static void FindStateSet(Dictionary<string, Dictionary<string, string>> transitions,
        string state, Dictionary<string, bool> visits,
        ref HashSet<string> StateSet)
        {
            Stack<string> stack = new Stack<string>();
            stack.Push(state);
            while (stack.Count > 0)
            {
                string top = stack.Pop();
                if (visits[top] == false)
                {
                    StateSet.Add(top);
                    visits[top] = true;
                    foreach (var symbol in transitions[top].Keys)
                    {
                        foreach (var v in transitions[top][symbol].Split(','))
                        {
                            if (!visits[v])
                                stack.Push(v);
                        }
                    }
                }
            }
        }
    }
}
