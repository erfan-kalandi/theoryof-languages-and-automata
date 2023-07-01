using System.IO;
using System.Linq;
using System.Text.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Text.RegularExpressions;

namespace Project
{

    // class to convert json file to NFA class that contain all property moslty as string
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

    public class NFAToDFA
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Please Enter your NFA path:");
            var inpath = Console.ReadLine();
            Console.WriteLine("Please Enter your DFA path to save:");
            var outpath = Console.ReadLine();
            var NewNFAS = new FAS();
            NewNFAS = JsonSerializer.Deserialize<FAS>(File.ReadAllText(inpath));
            FA NFA = new FA(NewNFAS);
            FA DFA = NFAToDFA(NFA);
            FAS newDFA = new FAS(DFA);
            File.WriteAllText(outpath, JsonSerializer.Serialize(newDFA));
        }

        private static FA NFAToDFA(FA NFA)
        {
            FA DFA = new FA();
            DFA.initial_state = NFA.initial_state;
            DFA.input_symbols = NFA.input_symbols;
            DFA.states.Add(NFA.initial_state);
            for (int i = 0; i < DFA.states.Count; i++)
            {
                string state = DFA.states[i];
                if (!DFA.transitions.ContainsKey(state))
                    DFA.transitions[state] = new Dictionary<string, string>();
                if (state != "TRAP")
                {
                    var toks = state.Split(',');
                    foreach (var symbol in NFA.input_symbols)
                    {
                        HashSet<string> StateSet = new HashSet<string>();
                        foreach (var t in toks)
                        {
                            if (!NFA.transitions[t].ContainsKey(symbol))
                                StateSet.Add("TRAP");
                            else
                            {
                                Dictionary<string, bool> visits = new Dictionary<string, bool>();
                                foreach (var s in NFA.states)
                                    visits[s] = false;
                                var ValidStates = NFA.transitions[t][symbol].Split("','").Where(c => c != "");
                                foreach (var x in ValidStates)
                                    FindStateSet(NFA.transitions, x, visits, ref StateSet);
                            }
                        }
                        string ToAdd = string.Join(",", StateSet.Where(s => s != "TRAP").OrderBy(s => s));
                        if (!ToAdd.Contains("TRAP") && ToAdd != string.Empty)
                        {
                            DFA.transitions[state][symbol] = ToAdd;
                            if (!DFA.states.Contains(ToAdd))
                                DFA.states.Add(ToAdd);
                            foreach (var FS in NFA.final_states)
                            {
                                if (ToAdd.Contains(FS) && !DFA.final_states.Contains(ToAdd))
                                {
                                    DFA.final_states.Add(ToAdd);
                                    break;
                                }
                            }
                        }
                        else
                        {
                            DFA.transitions[state][symbol] = "TRAP";
                            if (!DFA.states.Contains("TRAP"))
                                DFA.states.Add("TRAP");
                        }
                    }
                }
                else
                {
                    foreach (var symbol in NFA.input_symbols)
                        DFA.transitions[state][symbol] = "TRAP";
                }
            }
            return DFA;
        }

        //this method by a dfs find a set of state that we can reach from a state by one symbol and some lambda
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
                    if (transitions[top].ContainsKey(""))
                    {
                        foreach (var v in transitions[top][""].Split(','))
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