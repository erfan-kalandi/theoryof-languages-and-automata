# theory-of-languages-and-automata
In this project, I have implemented a number of practical parts of the theory of languages ​​and aoutomata in C# language

  part1 --> convert NFA to DFA
  part2 --> DFA simplification
  part3 --> Examining admission to the field in FA
  part4 --> Perform operations on Regular Language(Star,Concat,Union)
  
All sections receive the input in the form of a Jason file and the output is also in the form of a Jason file

Input example:
{
    "states": "{'q0','q1','q2','q3','q4'}",
    "input_symbols": "{'a','b'}",
    "transitions": {
      "q0": {
        "a": "{'q1'}"
      },
      "q1": {
        "": "{'q3'}",
        "b": "{'q2'}"
      },
      "q2": {
        "a": "{'q3'}"
      },
      "q3": {
        "b": "{'q4'}"
      },
      "q4": {
        "a": "{'q2'}"
      }
    },
    "initial_state": "q0",
    "final_states": "{'q1','q3'}"
}
