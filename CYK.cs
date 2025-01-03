/*
 *  Daniel Palmer
 *  CSCD 420
 *  12/6/24
 *  
 *  Notes if using the Visual Studio solution: 
 *  
 *         Output file is written to ..\bin\Debug\net8.0\output.txt
 *         If program cannot find "input.txt", make sure that it is set to "Copy if newer" in the properties
 */

namespace CYK;

public class CYK
{
    /// <summary>
    /// Program will take an input file "input.txt",
    /// where the first line is the number of grammars in the file, followed by a blank line
    /// -then each line contains a production in the grammar, followed by a blank line after
    /// -next line is the number of strings to be tested
    /// -then each line contains a string to be tested, followed by a blank line after
    /// repeat for each grammar
    ///
    /// Program then takes each string and determines if it is in the language defined by the grammar
    /// writes results to "output.txt"
    /// </summary>

    public static void Main()
    {
        try
        {
            List<List<string>> grammars = [];
            List<List<string>> sentences = [];

            LoadInput(grammars, sentences);

            StreamWriter sw = File.CreateText("output.txt");
            Console.SetOut(sw);

            for (int i = 0; i < grammars.Count; i++)
            {
                List<string> grammar = grammars[i];
                List<string> strings = sentences[i];

                PrintGrammar(grammar);

                foreach (string str in strings)
                {
                    ComputeTable(str, grammar);
                }
                Console.WriteLine();
            }
            sw.Close();
        }
        catch (FileNotFoundException e)
        {
            Console.WriteLine(e.Message);
        }
    }

    /// <summary>
    /// Reads input file and stores all grammars and sentences in their respective lists
    /// ignores the numbers in input file due to C# having wonky file reading
    /// </summary>
    /// <param name="grammars">collection of all grammars from input file</param>
    /// <param name="sentences">collection of all strings to be tested from the input file</param>

    private static void LoadInput(List<List<string>> grammars, List<List<string>> sentences)
    {
        string[] input = File.ReadAllLines("input.txt");

        // starts on first production in input file
        for (int line = 2; line < input.Length; line++)
        {
            List<string> grammar = [];
            List<string> strings = [];

            while (line < input.Length && input[line] != "")
            {
                grammar.Add(input[line]);
                line++;
            }
            grammars.Add(grammar);

            // skips blank line and number indicating how many strings to be tested
            line += 2;
            while (line < input.Length && input[line] != "")
            {
                strings.Add(input[line]);
                line++;
            }
            sentences.Add(strings);
        }
    }

    /// <summary>
    /// Prints grammar to the output file
    /// </summary>
    /// <param name="grammar">list of productions in the grammar to be printed</param>

    private static void PrintGrammar(List<string> grammar)
    {
        foreach (string production in grammar)
        {
            Console.WriteLine(production);
        }
        Console.WriteLine();
    }

    /// <summary>
    /// Computes the CYK table for the given string and grammar
    /// Prints result to output file
    /// </summary>
    /// <param name="s">the string being tested</param>
    /// <param name="grammar">list of productions in the grammar</param>

    private static void ComputeTable(string s, List<string> grammar)
    {
        string[,] table = new string[s.Length, s.Length];

        Dictionary<string, string> terminals = [];
        Dictionary<string, string> nonTerminals = [];

        // splits the grammar into terminals and non-terminals, converts to dictionary for easy lookup
        SplitGrammar(terminals, nonTerminals, grammar);

        ComputeFirstRow(s, terminals, table);

        for (int row = 1; row < s.Length; row++)
        {
            for (int col = 0; col < s.Length - row; col++)
            {
                for (int cell = 0; cell < row; cell++)
                {
                    if (table[cell, col] is not null && table[row - cell - 1, col + cell + 1] is not null)
                    {
                        string[] leftCell = table[cell, col].Split(" ");
                        string[] rightCell = table[row - cell - 1, col + cell + 1].Split(" ");
                        // checks if the combination of the two cells is a non-terminal production
                        foreach (var (left, right) in from string left in leftCell
                                                      from string right in rightCell
                                                      where nonTerminals.ContainsKey(left + right)
                                                      select (left, right))
                        {
                            if (table[row, col] is null)
                            {
                                table[row, col] = nonTerminals[left + right];
                            }
                            else
                            {
                                table[row, col] += " " + nonTerminals[left + right];
                            }
                        }
                    }
                }
            }
        }

        if (table[s.Length - 1, 0] is not null && table[s.Length - 1, 0].Contains('S'))
        {
            Console.WriteLine($"{s} is in the language defined by the above grammar");
        }
        else
        {
            Console.WriteLine($"{s} is NOT in the language defined by the above grammar");
        }
    }

    /// <summary>
    /// Splits the grammar into terminals and non-terminal
    /// Dictionary has key value pair of <RHS, LHS>
    /// ex. S --> AB turns into <AB, S> 
    /// if another production has the same RHS, it is appended to the value -> <AB, S C>
    /// </summary>
    /// <param name="terminals">dictionary of terminals in the grammar</param>
    /// <param name="nonTerminals">dictionary of non-terminals in the grammar</param>
    /// <param name="grammar">list of productions in the grammar</param>

    private static void SplitGrammar(Dictionary<string, string> terminals, Dictionary<string, string> nonTerminals, List<string> grammar)
    {
        foreach (string production in grammar)
        {
            if (char.IsUpper(production[6]))
            {
                try
                {
                    nonTerminals.Add(production[6..], $"{production[0]}");
                }
                catch (ArgumentException)
                {
                    nonTerminals[production[6..]] += $" {production[0]}";
                }
            }
            else
            {
                try
                {
                    terminals.Add(production[6..], $"{production[0]}");
                }
                catch (ArgumentException)
                {
                    terminals[production[6..]] += $" {production[0]}";
                }
            }
        }
    }

    /// <summary>
    /// Computes the first row of the CYK table
    /// </summary>
    /// <param name="s">the string being tested</param>
    /// <param name="terminals">dictionary of terminals in the grammar</param>
    /// <param name="table">the CYK table</param>

    private static void ComputeFirstRow(string s, Dictionary<string, string> terminals, string[,] table)
    {
        for (int i = 0; i < s.Length; i++)
        {
            if (terminals.ContainsKey(s[i].ToString()))
            {
                table[0, i] = terminals[s[i].ToString()];
            }
        }
    }
}
