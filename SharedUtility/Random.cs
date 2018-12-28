using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace SUS.Shared
{
    public static class RandomImpl
    {
        private static readonly CSPRandom _Random;

        static RandomImpl()
        {
            _Random = new CSPRandom();
        }

        public static int Next()
        {
            return _Random.Next();
        }

        public static int Next(int values)
        {
            return _Random.Next(values);
        }

        public static int Next(int min, int max)
        {
            return _Random.Next(min, max+1);
        }

        public static double NextDouble()
        {
            return _Random.NextDouble();
        }
    }

    public sealed class CSPRandom
    {
        private readonly RNGCryptoServiceProvider cspRng = new RNGCryptoServiceProvider();
        private const int BUFFER_SIZE = sizeof(double);
        private byte[] buffer = new byte[BUFFER_SIZE];

        public int Next()
        {
            this.cspRng.GetBytes(buffer);
            return BitConverter.ToInt32(this.buffer, 0) & 0x7fffffff;
        }

        public int Next(int values)
        {
            if (values <= 0)
                return 0;

            return Next() % values; // Return the remainder (range based.)
        }

        public int Next(int min, int max)
        {
            if (min <= 0 || max <= 0)
                return 0;
            if (min >= max)
                return max;

            return min + Next(max - min);   // Since it can proc '0', we add the min to it.
        }

        public double NextDouble()
        {
            this.cspRng.GetBytes(buffer);
            var ul = BitConverter.ToUInt64(this.buffer, 0) / (1 << 11);
            return ul / (Double)(1UL << 53);
        }
    }

    public class DiceRoll
    {
        //private static readonly string ndmRegex = @"^([0-9]*)d([0-9]+|%)$";
        private static readonly string ndmRegex = @"^(\-?[0-9]+)(?:(d(\-?[0-9]+))?)(?:((\+|\-)[0-9]+)?)";

        private string m_Expression = string.Empty;
        private int m_Amount = 0;
        private int m_Faces = 0;
        private int m_Modifier = 0;

        #region Constructors
        public DiceRoll(int dice, int faces) : this(dice, faces, 0) { }
        public DiceRoll(int dice, int faces, int mod)
        {
            Dice = dice;
            Faces = faces;
            Modifier = mod;
            m_Expression = "none";
        }

        public DiceRoll(string expression)
        {
            m_Expression = expression;
            Convert(expression);
        }
        #endregion

        #region Getters / Setters 
        private int Dice 
        {
            get { return m_Amount; }
            set
            {
                if (value <= 0)
                    value = 1;
                else if (value == Dice)
                    return;

                m_Amount = value;
            }
        }

        private int Faces
        {
            get { return m_Faces; }
            set
            {
                if (value <= 0)
                    value = 1;
                else if (value == Faces)
                    return;

                m_Faces = value;
            }
        }

        private int Modifier
        {
            get { return m_Modifier; }
            set
            {
                if (value == Modifier)
                    return;

                m_Modifier = value;
            }
        }

        public int Minimum { get { return Dice + Modifier; } }
        public int Maximum { get { return (Dice * Faces) + Modifier; } }
        #endregion

        public void Display()
        {
            Console.WriteLine("  {0,10}:  Amount: {1,-3} Faces: {2,-3} Modifier: {3,4}  :: Roll:{4}", m_Expression, Dice, Faces, Modifier, Roll());
        }

        /// <summary>
        ///     Generates a pseudo-random roll based on your current dice attributes.
        /// </summary>
        /// <returns></returns>
        public int Roll()
        {   // Minimum will always be the dice count (dice * 1).  Maximum will be dice count * faces.
            int val = Utility.RandomMinMax(Dice, Dice * Faces);
            if (val + Modifier <= 0)
                return 1;           // Prevention from the modifier being overly negative.
            return val + Modifier;  // Otherwise, just return the positive value.
        }

        private void Convert(string expression)
        {
            if (!Regex.IsMatch(expression, ndmRegex))
                return; // Regex doesn't match. Just return- it is invalid input.

            Match m = Regex.Match(expression, ndmRegex);
            if (m == null)
                return; // Something occured while attempting to get the match- assume invalid.

            // Expand our potential +/- signs so we have either 1 or 3 tokens.
            List<string> tokens = m.Value.Replace("-", " - ").Replace("+", " + ").Split(' ').Select(p => p.Trim()).ToList();
            switch (tokens.Count)
            {
                case 1:
                    // Process only 'NdM' type of expressions.
                    ParseString(tokens[0]);
                    break;
                case 3:
                    // Process ('NdM', +/-, Mod) tokens.
                    ParseList(tokens);
                    return;
                default:
                    return;
            }
        }

        private bool ParseString(string token)
        {   // Parses: 'N' or 'NdM'
            Modifier = 0;

            int amt;
            List<string> tokens = token.Split('d').ToList();    // Split based on the 'd' in 'NdM'
            if (tokens.Count == 1)
            {   // Token count: 1 - A single number was passed... try and parse it.
                if (int.TryParse(tokens[0], out amt))
                {
                    Dice = amt;     // Assign it to be our minimum
                    Faces = 1;      //  and maximum.
                    return true;    // The parse was successful.
                }
            }   
            else if (tokens.Count > 2)
            {   // Tokens are not something we can parse.
                Dice = Faces = 1;   // Assign a value and just return.
                return false;       // Was not a successful parse.
            }

            int faces;
            if (int.TryParse(tokens[0], out amt))
            {   // Going to attempt to parse a 'NdM' based string. Gets the amount of dice first.
                if (amt <= 0)
                    amt = 1;    // We attempted to parse a negative or 0, minimum is 1.
                Dice = amt;
                if (int.TryParse(tokens[1], out faces))
                {   // Parses the face value of the dice.
                    if (faces <= 0)
                        faces = 1;  // Our face value can not be negative or 0, set it to 1.
                    Faces = faces;
                }
                else
                    Faces = 1;      // An error occured parsing the face value, set it to 1.
                return true;
            }

            Dice = Faces = 1;       // Could not parse, assign a minimum value.
            return false;
        }

        private void ParseList(List<string> tokens)
        {   // Attempt to parse the 'NdM' portion first.
            if (ParseString(tokens[0]))
                Modifier = getModifier(tokens[1], tokens[2]);   // Attempts to parse the modifier.
        }

        private int getModifier(string sign, string number)
        {
            bool positive = false;

            // Check the sign first, + / -
            if (sign == "+")
                positive = true;
            else if (sign == "-")
                positive = false;

            int mod = 0;
            if (int.TryParse(number, out mod))
            {   // Parses the modifier.
                if (positive)
                    return mod;     // If it is positive, return.
                else
                    return -(mod);  // If it is negative, return a negative version.
            }

            // Was unable to parse, return 0 as a modifier.
            return 0;
        }
    }
}
