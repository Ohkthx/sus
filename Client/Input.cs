using System;
using System.Collections.Generic;
using System.Linq;

namespace SUS.Client
{
    public class Input
    {
        private readonly List<string> _arguments; // Non-command arguments.
        private int _currentPosition; // Current position in the list of arguments.
        private int _iteratePosition; // Current position that is used by the Iterate() function.

        /// <summary>
        ///     Get's user input, continuously asks until the input is valid.
        /// </summary>
        /// <returns>Input from the user.</returns>
        public static string Get()
        {
            string input;
            do
            {
                Console.Write(" > ");
                input = Console.ReadLine();
            } while (string.IsNullOrWhiteSpace(input));

            return input;
        }

        /// <summary>
        ///     Prints the enum-based options and gets valid input from the user.
        /// </summary>
        /// <typeparam name="TEnum">Type of the enum to print and get.</typeparam>
        /// <param name="allowNone">Allow none as an option?</param>
        /// <param name="noneValue">Value of none, if it exists.</param>
        /// <returns>Enum that was chosen.</returns>
        public static TEnum PrintAndGet<TEnum>(bool allowNone, int noneValue = -1)
            where TEnum : struct, IConvertible, IFormattable
        {
            PrintUniqueOptions<TEnum>(allowNone, noneValue);

            while (true)
            {
                var input = Get();
                if (!ConvertEnum<TEnum>(input, out var parsed, allowNone, noneValue))
                    continue;

                return parsed;
            }
        }

        /// <summary>
        ///     Parses input for to get the BaseOption and also attempts the the "Get Option" as well.
        /// </summary>
        /// <returns>Success or Failure of the action.</returns>
        private bool InputParser()
        {
            // Attempt to parse the base option first.
            foreach (var baseOpt in _arguments)
            {
                if (!ConvertEnum<BaseOptions>(baseOpt, out var found, false, 0))
                    continue;

                Base = found;
                _arguments.Remove(baseOpt); // We do not need it in our list of arguments anymore.
                break;
            }

            // If we were unable to parse the base, return failure.
            if (Base == BaseOptions.None)
                return false;

            // There are no more arguments to parse, or the Base is not "get"
            if (_arguments.Count == 0 || Base != BaseOptions.Get)
                return true;

            // Try to parse the rest of the command from the user.
            foreach (var getOpt in _arguments)
            {
                if (!ConvertEnum<GetOptions>(getOpt, out var found, false, 0))
                    continue;

                GetOption = found;
                _arguments.Remove(getOpt); // Remove the "get" option from out arguments.
                break;
            }

            return true;
        }

        /// <summary>
        ///     Checks if there is a previous value that can be obtained from the list of arguments.
        /// </summary>
        private bool HasPrevious()
        {
            if (_currentPosition > 0)
                return true;

            _currentPosition = 0; // Ensure that _currentPosition is never below zero.
            return false;
        }

        /// <summary>
        ///     Checks if there is a next value that can be obtained from the list of arguments.
        /// </summary>
        private bool HasNext()
        {
            if (_currentPosition < _arguments.Count - 1)
                return true;

            _currentPosition = _arguments.Count - 1; // Ensure that _currentPosition never exceeds the maximum value.
            return false;
        }

        /// <summary>
        ///     Gets the previous value in the argument list of it exists.
        /// </summary>
        /// <param name="previous">Value to be assigned if the action is successful.</param>
        /// <returns>Success or Failure of the action.</returns>
        public bool Previous(out string previous)
        {
            previous = string.Empty; // Set our default value.

            // Prevent trying to access OutOfBounds information.
            if (!HasPrevious())
                return false;

            previous = _arguments[--_currentPosition];
            return true;
        }

        /// <summary>
        ///     Gets the next value in the argument list if it exists.
        /// </summary>
        /// <param name="next">Value to be assigned if the action is successful.</param>
        /// <returns>Success or Failure of the action.</returns>
        public bool Next(out string next)
        {
            next = string.Empty; // Set our default value.

            // Prevent trying to access OutOfBounds information.
            if (!HasNext())
                return false;

            next = _arguments[++_currentPosition];
            return true;
        }

        /// <summary>
        ///     Starts at the beginning of the list and iterates each value.
        /// </summary>
        /// <param name="next">Next value in the list if it exists.</param>
        /// <returns>Success if the value exists, failure if it does not.</returns>
        public bool Iterate(out string next)
        {
            next = string.Empty;

            // Reset the iterator to 0 and return "false" signalling the end.
            if (_iteratePosition >= _arguments.Count)
            {
                _iteratePosition = 0;
                return false;
            }

            // Get the current iteration and post-increment our iterator.
            next = _arguments[_iteratePosition++];
            return true;
        }

        /// <summary>
        ///     Returns the input arguments in a list.
        /// </summary>
        /// <returns>List of remainder arguments.</returns>
        public List<string> ArgsToList()
        {
            return _arguments;
        }

        #region Constructors

        public Input(string userInput)
        {
            if (string.IsNullOrWhiteSpace(userInput))
                throw new ArgumentException("User input must have a value.", nameof(userInput));

            // Split the string input a list and assign it.
            _arguments = userInput.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();

            // Parse the input.
            if (!InputParser())
                throw new ArgumentException("Could not understand the option selected.", nameof(userInput));
        }

        public Input(List<string> userInput)
        {
            if (userInput == null || userInput.Count == 0)
                throw new ArgumentException("User input must have a value.", nameof(userInput));

            // Assign our input.
            _arguments = userInput;

            // Parse the input.
            if (!InputParser())
                throw new ArgumentException("Could not understand the option selected.", nameof(userInput));
        }

        #endregion

        #region Getters / Setters

        public int Count => _arguments.Count;

        public BaseOptions Base { get; private set; }

        public GetOptions GetOption { get; private set; }

        #endregion

        #region Enum Conversions

        /// <summary>
        ///     Gets a list of enum names that are unique in their underlying value. Ignoring duplicates and aliases.
        /// </summary>
        /// <typeparam name="TEnum">Type of the Enum.</typeparam>
        /// <param name="allowNone">Is none a valid argument to consider?</param>
        /// <param name="noneValue">If None is in the enum, the value it holds.</param>
        /// <returns>List of unique enum names pertaining to the TEnum type.</returns>
        public static List<string> UniqueEnumNames<TEnum>(bool allowNone, int noneValue = -1)
            where TEnum : struct, IConvertible, IFormattable
        {
            var uniqueNames = new List<string>();

            var enumPos = 0;
            foreach (var e in (TEnum[]) Enum.GetValues(typeof(TEnum)))
            {
                // Get the integer and string value of the enum.
                var enumValue = Convert.ToInt32(e);
                var enumName = Enum.GetName(typeof(TEnum), e);

                // If it is a duplicate or alias, skip it.
                if (enumPos != enumValue)
                    continue;

                // If the value is "none" and we are ignoring "none", continue.
                if (!allowNone && noneValue >= 0 && enumPos == noneValue && enumName.ToLower() == "none")
                {
                    ++enumPos;
                    continue;
                }

                // Add the value to the list to be returned.
                uniqueNames.Add(enumName);
                ++enumPos;
            }

            return uniqueNames;
        }

        /// <summary>
        ///     Takes in a string, attempts to match it to a value of an Enum. Works for integer based Enums.
        /// </summary>
        /// <typeparam name="TEnum">Type of Enum</typeparam>
        /// <param name="value">String to convert to Enum.</param>
        /// <param name="input">If successful, holds the converted value.</param>
        /// <param name="allowNone">Is none a valid argument to consider?</param>
        /// <param name="noneValue">If None is in the enum, the value it holds.</param>
        /// <returns>Success or failure of the action.</returns>
        public static bool ConvertEnum<TEnum>(string value, out TEnum input, bool allowNone, int noneValue = -1)
            where TEnum : struct, IConvertible, IFormattable
        {
            // Set the default value to the value of "none" in the event we return false.
            input = (TEnum) Enum.Parse(typeof(TEnum), noneValue.ToString(), true);

            var enumPos = -1;
            foreach (var enumName in Enum.GetNames(typeof(TEnum)))
            {
                ++enumPos; // Increment the position of the enum.

                // Get the enum in the form of the enum type and integer.
                var enumCode = (TEnum) Enum.Parse(typeof(TEnum), enumName, true);
                var enumValue = Convert.ToInt32(enumCode);

                // If the value is "none" and we are ignoring "none", continue.
                if (!allowNone && noneValue >= 0 && enumPos == noneValue && enumName.ToLower() == "none")
                    continue;

                // Compare if the enum contains the passed value, continue if not.
                if (!enumName.ToLower().Contains(value.ToLower()))
                    continue;

                // Success, convert it to the first occurrence in the enum (aliases / duplicates are treated as same value.
                input = (TEnum) Enum.Parse(typeof(TEnum), enumValue.ToString(), true);
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Prints out options that are enum based.
        /// </summary>
        /// <typeparam name="TEnum">Type of enum.</typeparam>
        /// <param name="allowNone">Display none?</param>
        /// <param name="noneValue">Value of none if it is present in the enum.</param>
        public static void PrintUniqueOptions<TEnum>(bool allowNone, int noneValue = -1)
            where TEnum : struct, IConvertible, IFormattable
        {
            var characterCount = 0;

            Console.WriteLine("\nOptions:");
            foreach (var opt in UniqueEnumNames<TEnum>(allowNone, noneValue))
            {
                characterCount += opt.Length + 3; // Used to decide if we need to wrap.

                // Display the option on the same line..
                Console.Write($"[{opt.ToLower()}] ");

                if (characterCount < 50)
                    continue;

                // Reset the counter and add a new line.
                characterCount = 0;
                Console.WriteLine();
            }

            Console.WriteLine();
        }

        #endregion
    }
}