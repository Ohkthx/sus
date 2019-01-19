using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace SUS.Shared
{
    public static class RandomImpl
    {
        private static readonly CspRandom Random;

        static RandomImpl()
        {
            Random = new CspRandom();
        }

        public static int Next()
        {
            return Random.Next();
        }

        public static int Next(int values)
        {
            return Random.Next(values);
        }

        public static int Next(int min, int max)
        {
            return Random.Next(min, max+1);
        }

        public static double NextDouble()
        {
            return Random.NextDouble();
        }
    }

    public class CspRandom
    {
        private readonly RNGCryptoServiceProvider m_CspRng = new RNGCryptoServiceProvider();
        private const int BufferSize = sizeof(double);
        private readonly byte[] m_Buffer = new byte[BufferSize];

        public int Next()
        {
            m_CspRng.GetBytes(m_Buffer);
            return BitConverter.ToInt32(m_Buffer, 0) & 0x7fffffff;
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
            m_CspRng.GetBytes(m_Buffer);
            var ul = BitConverter.ToUInt64(m_Buffer, 0) / (1 << 11);
            return ul / 9.00719925474099E+15D;
        }
    }

    public class DiceRoll
    {
        //private static readonly string ndmRegex = @"^([0-9]*)d([0-9]+|%)$";
        private const string NdmRegex = @"^(\-?[0-9]+)(?:(d(\-?[0-9]+))?)(?:((\+|\-)[0-9]+)?)";

        private int m_Amount;
        private int m_Faces;
        private int m_Modifier;

        #region Constructors

        public DiceRoll(string expression)
        {
            Convert(expression);
        }

        #endregion

        #region Getters / Setters 
        private int Dice 
        {
            get => m_Amount;
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
            get => m_Faces;
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
            get => m_Modifier;
            set
            {
                if (value == Modifier)
                    return;

                m_Modifier = value;
            }
        }

        public int Minimum => Dice + Modifier;
        public int Maximum => (Dice * Faces) + Modifier;

        #endregion

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
            if (!Regex.IsMatch(expression, NdmRegex))
                return; // Regex doesn't match. Just return- it is invalid input.

            var m = Regex.Match(expression, NdmRegex);

            // Expand our potential +/- signs so we have either 1 or 3 tokens.
            var tokens = m.Value.Replace("-", " - ").Replace("+", " + ").Split(' ').Select(p => p.Trim()).ToList();
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

            if (int.TryParse(tokens[0], out amt))
            {   // Going to attempt to parse a 'NdM' based string. Gets the amount of dice first.
                if (amt <= 0)
                    amt = 1;    // We attempted to parse a negative or 0, minimum is 1.
                Dice = amt;

                if (int.TryParse(tokens[1], out var faces))
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

        private void ParseList(IReadOnlyList<string> tokens)
        {   // Attempt to parse the 'NdM' portion first.
            if (ParseString(tokens[0]))
                Modifier = GetModifier(tokens[1], tokens[2]);   // Attempts to parse the modifier.
        }

        private static int GetModifier(string sign, string number)
        {
            var positive = false;

            switch (sign)
            {
                // Check the sign first, + / -
                case "+":
                    positive = true;
                    break;
                case "-":
                    break;
            }

            if (!int.TryParse(number, out var mod)) return 0; // Parses the modifier.

            if (positive)
                return mod;     // If it is positive, return.

            return -(mod);  // If it is negative, return a negative version.

        }
    }
}
