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
                    transitions[state][x.Key] = "{'" + x.Value + "'}";
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


    //class To convert a FAS class to standard FA class for exaple by split string and convert it list
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

    public class Operations
    {
        public static void Main()
        {
            Console.WriteLine("choose your operation:\n1)Star\n2)Concat\n3)Union");
            int op = 0;
            var x = int.TryParse(Console.ReadLine(), out op);
            switch (op)
            {
                case 1:
                    MainStar();
                    break;
                case 2:
                    MainConcat();
                    break;
                case 3:
                    MainUnion();
                    break;
                default:
                    Main();
                    break;

            }
        }

        
        static void MainStar()
        {
            Console.WriteLine("Please Enter your NFA path:");
            var inpath = Console.ReadLine();
            Console.WriteLine("Please Enter your Simplified NFA path to save:");
            var outpath = Console.ReadLine();
            var NFAS = new FAS();
            NFAS = JsonSerializer.Deserialize<FAS>(File.ReadAllText(inpath));
            FA NFA = new FA(NFAS);
            FA Star = FAStar(NFA);
            FAS newNFA = new FAS(Star);
            File.WriteAllText(outpath, JsonSerializer.Serialize(newNFA));
        }



        public static void MainConcat()
        {
            Console.WriteLine("Please Enter your NFA1 path:");
            var inpath1 = Console.ReadLine();
            Console.WriteLine("Please Enter your NFA2 path:");
            var inpath2 = Console.ReadLine();
            Console.WriteLine("Please Enter your Concat NFA path to save:");
            var outpath = Console.ReadLine();
            var NFAS1 = new FAS();
            var NFAS2 = new FAS();
            NFAS1 = JsonSerializer.Deserialize<FAS>(File.ReadAllText(inpath1));
            NFAS2 = JsonSerializer.Deserialize<FAS>(File.ReadAllText(inpath2));
            FA NFA1 = new FA(NFAS1);
            FA NFA2 = new FA(NFAS2);
            FA Concat = FAConcat(NFA1, NFA2);
            FAS newNFA = new FAS(Concat);
            File.WriteAllText(outpath, JsonSerializer.Serialize(newNFA));
        }



        private static void MainUnion()
        {
            Console.WriteLine("Please Enter your NFA1 path:");
            var inpath1 = Console.ReadLine();
            Console.WriteLine("Please Enter your NFA2 path:");
            var inpath2 = Console.ReadLine();
            Console.WriteLine("Please Enter your Union NFA path to save:");
            var outpath = Console.ReadLine();
            var NFAS1 = new FAS();
            var NFAS2 = new FAS();
            NFAS1 = JsonSerializer.Deserialize<FAS>(File.ReadAllText(inpath1));
            NFAS2 = JsonSerializer.Deserialize<FAS>(File.ReadAllText(inpath2));
            FA NFA1 = new FA(NFAS1);
            FA NFA2 = new FA(NFAS2);
            FA Concat = FAUnion(NFA1, NFA2);
            FAS newNFA = new FAS(Concat);
            File.WriteAllText(outpath, JsonSerializer.Serialize(newNFA));
        }




        private static FA FAStar(FA NFA)
        {
            string newInintial = NFA.states.Last();
            newInintial =  newInintial.Replace(newInintial.Last(),(char)(newInintial.Last()+1));
            string newFinal = newInintial.Replace(newInintial.Last(),(char)(newInintial.Last()+1));
            NFA.states.Add(newInintial);
            NFA.states.Add(newFinal);
            NFA.transitions[newInintial] = new Dictionary<string, string>();
            NFA.transitions[newInintial][""] =$"{NFA.initial_state}','{newFinal}";
            foreach (var f in NFA.final_states)
                NFA.transitions[f][""] = $"{newFinal}";
            NFA.initial_state = newInintial;
            NFA.final_states.Clear();
            NFA.final_states.Add(newFinal);
            NFA.transitions[newFinal] = new Dictionary<string, string>();
            NFA.transitions[newFinal][""] = $"{newInintial}";
            return NFA;
        }




        private static FA FAConcat(FA NFA1, FA NFA2)
        {
            FA Concat = new FA();
            Concat.initial_state = NFA1.initial_state;
            Concat.input_symbols.AddRange(NFA1.input_symbols);
            foreach (var symbol in NFA2.input_symbols)
            {
                if (!Concat.input_symbols.Contains(symbol))
                    Concat.input_symbols.Add(symbol);
            }
            Concat.states.AddRange(NFA1.states);
            Concat.transitions = NFA1.transitions;
            int idx = NFA1.states.Count;
            foreach (var s in NFA2.states)
            {
                string newstate = s.Replace(s.Last(),(char)(s.Last()+idx));
                Concat.transitions[newstate] = new Dictionary<string, string>();
                Concat.states.Add(newstate);
                if (NFA2.final_states.Contains(s))
                    Concat.transitions[newstate][""] =s.Replace(s.Last(),(char)(s.Last()+Concat.states.Count-NFA1.states.Count));
                foreach (var transiton in NFA2.transitions[s])
                {
                    var toks = transiton.Value.Split("','").Select(q => q.Replace(q.Last(),(char)(q.Last()+NFA1.states.Count)));
                    Concat.transitions[newstate][transiton.Key] = $"{string.Join("','", toks)}";
                }
            }
            foreach (var item in NFA1.final_states)
            {
                Concat.transitions[item][""] =Concat.states[NFA1.states.Count];
            }
            string ss = Concat.states.Last();
            string news = ss.Replace(ss.Last(),(char)(ss.Last()+1));
            Concat.states.Add(news);
            Concat.transitions[news] = new Dictionary<string, string>();
            Concat.final_states.Add(news);
            return Concat;
        }



        public static FA FAUnion(FA NFA1, FA NFA2)
        {
            FA Union = new FA();
            Union.input_symbols.AddRange(NFA1.input_symbols);
            foreach (var symbol in NFA2.input_symbols)
            {
                if (!Union.input_symbols.Contains(symbol))
                    Union.input_symbols.Add(symbol);
            }
            Union.states.AddRange(NFA1.states);
            Union.transitions = NFA1.transitions;
            int idx = NFA1.states.Count;
            foreach (var s in NFA2.states)
            {
                string newstate = s.Replace(s.Last(),(char)(s.Last()+idx));
                Union.transitions[newstate] = new Dictionary<string, string>();
                Union.states.Add(newstate);
                if (NFA2.final_states.Contains(s))
                    Union.transitions[newstate][""] =s.Replace(s.Last(),(char)(s.Last()+Union.states.Count-NFA1.states.Count));
                foreach (var transiton in NFA2.transitions[s])
                {
                    var toks = transiton.Value.Split("','").Select(q => q.Replace(q.Last(),(char)(q.Last()+NFA1.states.Count)));
                    Union.transitions[newstate][transiton.Key] = $"{string.Join("','", toks)}";
                }
            }
            string ss = Union.states.Last();
            string news = ss.Replace(ss.Last(),(char)(ss.Last()+1));
            Union.states.Add(news);
            Union.initial_state = news;
            Union.transitions[news]= new Dictionary<string, string>();
            Union.transitions[news][""] = $"{NFA1.initial_state}','{NFA2.initial_state.Replace(NFA2.initial_state.Last(),(char)(NFA2.initial_state.Last()+NFA1.states.Count))}";
            string newF = ss.Replace(ss.Last(),(char)(ss.Last()+2));
            Union.states.Add(newF);
            Union.final_states.Add(newF);
            Union.transitions[newF] = new Dictionary<string, string>();
            foreach (var item in NFA1.final_states)
            {
                Union.transitions[item][""] = $"{Union.final_states[0]}";
            }
            foreach (var item in NFA2.final_states)
            {
                Union.transitions[$"{ item.Replace(item.Last(),(char)(item.Last()+NFA1.states.Count))}"][""] = $"{Union.final_states[0]}";
            }
            return Union;
        }
    }
}