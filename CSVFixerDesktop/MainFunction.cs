using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic.FileIO;

namespace CSVFixerDesktop
{
    class MainFunction
    {
        /// <summary>
        /// Global Variables. We will store everything in RAM and process it so to avoid any re-read operations from disk.
        /// </summary>
        public static string _filePath;
        public static string _fileContents;
        public static int _linechangesCounter;
        public static List<int> fieldDividers;
        public static int _fieldCount;
        public static string[] _cells;
        public static List<string[]> _parsedCsv = new List<string[]>();
        public static int _maxAllowedFields;
        public static string _delimiter = ",";  //string[] delimiters = { "\",\"", ",", ";", "\t" };


        /// <summary>
        /// Clear the Global Variables
        /// </summary>
        public static void ClearEverything()
        {
            _filePath = null;
            _fileContents = null;

            if (fieldDividers != null)
                fieldDividers.Clear();

            if (_cells != null)
                Array.Clear(_cells, 0, _cells.Length);

            if (_parsedCsv != null)
                _parsedCsv.Clear();
        }


        /// <summary>
        /// This will count how many lines the csv has.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static int CountLineChanges(string filePath)
        {
            // clear everything
            ClearEverything();

            _filePath = filePath;

            // read the file and Count the linechanges
            _fileContents = new StreamReader(_filePath).ReadToEnd();
            string[] lines = _fileContents.Split(
                    new[] { "\r\n", "\r", "\n" },
                    StringSplitOptions.None
                );

            _linechangesCounter = lines.Length;

            return _linechangesCounter;
        }


        /// <summary>
        /// This will handle .csv that has everything in one line.
        /// </summary>
        public static void OneLineCsvHandling()
        {
            // split every single cell by the default delimiter
            SplitByDelimiterReadString(_delimiter);

            // Find all possible divisions
            fieldDividers = FindAllDivisors(_cells.Length);
        }


        /// <summary>
        /// This will read the _fileContents variable and create the _parsedCsv List<string[]> object
        /// </summary>
        public static void GenerateCsvObjectFromString()
        {
            // first clear any old data
            _parsedCsv.Clear();

            StringReader sr = new StringReader(_fileContents);

            using (TextFieldParser parser = new TextFieldParser(sr))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(_delimiter);
                parser.HasFieldsEnclosedInQuotes = true;

                while (!parser.EndOfData)
                {
                    _parsedCsv.Add(parser.ReadFields());
                }
            }

            _fieldCount = _parsedCsv[0].Length;

        }


        /// <summary>
        /// Directly read the csv file and convert it to List<string[]>
        /// No used for the time. I'll keep this for any future use
        /// </summary>
        public static void GenerateCsvObjectFromFile()
        {
            // first clear any old data
            _parsedCsv.Clear();

            // read the file and poppulate the global variable
            using (TextFieldParser parser = new TextFieldParser(_filePath))
            {
                parser.Delimiters = new string[] { _delimiter };

                while (true)
                {
                    string[] parts = parser.ReadFields();
                    if (parts == null)
                    {
                        break;
                    }
                    _parsedCsv.Add(parts);
                }
            }

            _fieldCount = _parsedCsv[0].Length;
        }


        /// <summary>
        /// This will read the _cells variable and by the given field number, will create the _parsedCsv List<string[]> object
        /// </summary>
        /// <param name="fieldCount"></param>
        public static void GenerateCsvObject(int fieldCount)
        {
            // first clear any old data
            _parsedCsv.Clear();

            // Split to rows
            int maxIndex = _cells.Length;

            for (int i = 0; i < maxIndex; i += fieldCount)
            {
                string[] temp = new string[fieldCount];

                for (int j = 0; j < fieldCount; j++)
                {
                    if ((i + j) < maxIndex)
                        temp[j] = _cells[i + j];
                }
                _parsedCsv.Add(temp);
            }
        }


        /// <summary>
        /// This will calculate all possible divisors according the number of elements in _cells variable
        /// </summary>
        /// <param name="cellCount"></param>
        /// <returns></returns>
        private static List<int> FindAllDivisors(int cellCount)
        {
            List<int> divisors = new List<int>();

            for (int i = 1; i <= cellCount; i++)
                if (cellCount % i == 0)
                    divisors.Add(i);

            // check for duplicates and reduce the size of the list
            for (int i = 0; i < divisors.Count(); i++)
            {
                if (CheckForDupplicateHeaders(divisors[i]))
                {
                    _maxAllowedFields = divisors[i-1];
                    divisors = divisors.Take(i).ToList();
                    break;
                }
            }

            return divisors;
        }


        /// <summary>
        /// Will check if there any headers with the same name and then reduce the numnber of possible divisions
        /// </summary>
        /// <param name="fieldCount"></param>
        /// <returns></returns>
        public static bool CheckForDupplicateHeaders(int fieldCount)
        {
            var query = _cells.Take(fieldCount).GroupBy(x => x)
                          .Where(g => g.Count() > 1)
                          .Select(y => y.Key)
                          .ToList();

            if (query.Count() > 0)
                return true;
            else
                return false;
        }


        /// <summary>
        /// Will read the file contents from Disk and split the csv file and store it to _cells
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="delimiter"></param>
        public static void SplitByDelimiterReadFile(string filePath, string delimiter)
        {
            using (TextFieldParser parser = new TextFieldParser(filePath))
            {
                parser.Delimiters = new string[] { delimiter };
                while (true)
                {
                    string[] parts = parser.ReadFields();
                    if (parts == null)
                        break;

                    // save the non-null result to the global variable
                    // I needed to remove the last entry as it is always empty
                    _cells = parts.Take(parts.Length - 1).ToArray();
                }
            }
        }


        /// <summary>
        ///  Will read the file contents from RAM and split the csv file and store it to _cells
        /// </summary>
        /// <param name="delimiter"></param>
        public static void SplitByDelimiterReadString(string delimiter)
        {
            using (TextFieldParser parser = new TextFieldParser(new StringReader(_fileContents)))
            {
                parser.Delimiters = new string[] { delimiter };
                while (true)
                {
                    string[] parts = parser.ReadFields();
                    if (parts == null)
                        break;

                    // save the non-null result to the global variable
                    // I needed to remove the last entry as it is always empty
                    _cells = parts.Take(parts.Length - 1).ToArray();
                }
            }
        }


        /// <summary>
        /// Will try to find the csv delimiter. Currently not used.. I need to do more tests on this
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string AutoDelimiterFinder(string input)
        {
            string[] delimiters = { "\",\"", ",", ";", "\t" };

            int counter = 0;
            int counterTemp = 0;
            string delim = "";
            foreach (string separator in delimiters)
            {
                var vv = input.Split(new string[] { separator }, StringSplitOptions.None);
                counter = vv.Count();
                if (counter > counterTemp)
                {
                    delim = separator.ToString();
                    counterTemp = counter;
                }
            }

            return delim;
        }

    }
}
