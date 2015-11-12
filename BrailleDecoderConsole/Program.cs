using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HTP
{
    class Program
    {
        static void Main(string[] args)
        {
            int firstAlpha = 'a', lastAlpha = 'z';
            int alphaSize = GetAlphaRangeSize((char)firstAlpha, (char)lastAlpha);
            int decadeSize = 10;
            int[] characterBrailleBinding = new int[alphaSize];

            string[] characters = new string[52];

            //Setting-up 1st - 3rd decade codes
            for(int currentChar = firstAlpha, decade = 0, cell = 0; currentChar <= lastAlpha; currentChar++)
            {
                //Use Decade-Cell coordinate
                //Base 0
                decade = ((currentChar - firstAlpha) / decadeSize);     //decade = (((currentChar - firstAlpha) / decadeSize) + 1);
                cell = (((currentChar % (firstAlpha)) + 1) % decadeSize);   //Current cell the character resides

                int cellBinaryValue = GetCellBinaryValue(cell);      //Cell value bit-setting
                int decadeBinaryValue = GetDecadeBinaryValue(decade);      //Decade bit-setting
                int brailleValue = GetBrailleBinaryValue(cellBinaryValue, decadeBinaryValue);
                int index = GetCharacterIndex(alphaSize, lastAlpha, currentChar);

                characterBrailleBinding[index] = brailleValue;
                characters[index] = ((char)currentChar).ToString();


                Console.WriteLine(
                    "Char: {0}; Decade: {1}; Cell: {2}; Converted Binary: {3}; Index: {4}",
                    characters[index], decade, cell, Convert.ToString(brailleValue, 2), index
                    );
            }

            List<string> nonLatinAlphabet = new List<string>(Enumerable.Repeat<string>(string.Empty, Convert.ToInt32("111111", 2) + 1));     //Array should shoulder all braille byte code

            //3rd Decade
            nonLatinAlphabet[Convert.ToInt32("111011", 2)] = "and";
            nonLatinAlphabet[Convert.ToInt32("111111", 2)] = "for";
            nonLatinAlphabet[Convert.ToInt32("101111", 2)] = "of";
            nonLatinAlphabet[Convert.ToInt32("011011", 2)] = "the";
            nonLatinAlphabet[Convert.ToInt32("011111", 2)] = "with";

            //4th Decade
            nonLatinAlphabet[Convert.ToInt32("100001", 2)] = "ch";
            nonLatinAlphabet[Convert.ToInt32("101001", 2)] = "gh";
            nonLatinAlphabet[Convert.ToInt32("110001", 2)] = "sh";
            nonLatinAlphabet[Convert.ToInt32("110101", 2)] = "th";
            nonLatinAlphabet[Convert.ToInt32("100101", 2)] = "wh";
            nonLatinAlphabet[Convert.ToInt32("111001", 2)] = "ed";
            nonLatinAlphabet[Convert.ToInt32("111101", 2)] = "er";
            nonLatinAlphabet[Convert.ToInt32("101101", 2)] = "ou";
            nonLatinAlphabet[Convert.ToInt32("011001", 2)] = "ow";
            characterBrailleBinding[GetCharacterIndex(alphaSize, lastAlpha, 'w')] = Convert.ToInt16("011101", 2);       //By-pass letter 'w' binding code, since it belongs to 4th decade

            //5th Decade
            //nonLatinAlphabet[Convert.ToInt32("", 2)] = "";

            Console.WriteLine();
            Console.WriteLine();

            char[][] brailleData = new char[2][]{
                GetSampleData(),
                GetChallengeData()
            };

            for (int index = 0; index < brailleData.Length; index++)
            {
                char[] brailleDatum = brailleData[index];

                Console.WriteLine("Braille Codes: \n");
                int[] brailleCodes = ConvertInputToBrailleCodes(brailleDatum);

                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("Braille Data: \n\n");
                Console.WriteLine(brailleDatum);

                PrintBrailleOutput(brailleCodes, characterBrailleBinding, nonLatinAlphabet);

                Console.WriteLine();
                Console.WriteLine("-------------------------------------------");
            }
            Console.ReadLine();
        }

        static int[] ConvertInputToBrailleCodes(char[] brailleData)
        {
            int brailleRowCount = 3, braileColumnCount = 3, newlineCharCount = System.Environment.NewLine.ToArray().Length;
            int brailleDimensionSize = brailleRowCount * braileColumnCount;
            //Characters = (Columns * Row); Row = (Characters/Columns); Row = (Characters/Dimension)
            int estimatedBrailleCharacterCount = brailleData.Length / brailleDimensionSize;             //Trimming newline
            int estimatedBrailleCharacterSpillCount = brailleData.Length % brailleDimensionSize;        //Spill = system defined newline; anything out of braille data
            int currentBrailleLineCount = brailleData.Length / brailleRowCount;
            int[] brailleCodes = new int[estimatedBrailleCharacterCount];

            char[] normalizedBrailleData = brailleData;
           
            if(brailleData.ToString().LastIndexOf(System.Environment.NewLine) != ((brailleData.Length - 1) - System.Environment.NewLine.ToArray().Length))
            {

            }
            //int normalizedBrailleDataSize =
            //    (brailleDimensionSize * estimatedBrailleCharacterCount) +
            //    (estimatedBrailleCharacterSpillCount > 0 ? braileColumnCount : 0)                       //Make sure newlines are properly padded
            //    ;
             

            //System.Array.Resize(ref normalizedBrailleData, normalizedBrailleDataSize);   //Ensure that offset computation works properly

            for (int index = 0; index < estimatedBrailleCharacterCount; index++)
            {
                int value = 0x0;
                int brailleCharacterOffset = (index * braileColumnCount);

                char bitToCompare = 'O';

                value = value | ((normalizedBrailleData[brailleCharacterOffset] == bitToCompare) ? 0x20 : 0x0);
                value = value | ((normalizedBrailleData[brailleCharacterOffset + 1] == bitToCompare) ? 0x10 : 0x0);
                value = value | ((normalizedBrailleData[brailleCharacterOffset + currentBrailleLineCount] == bitToCompare) ? 0x8 : 0x0);
                value = value | ((normalizedBrailleData[brailleCharacterOffset + currentBrailleLineCount + 1] == bitToCompare) ? 0x4 : 0x0);
                value = value | ((normalizedBrailleData[brailleCharacterOffset + (currentBrailleLineCount * 2)] == bitToCompare) ? 0x2 : 0x0);
                value = value | ((normalizedBrailleData[brailleCharacterOffset + (currentBrailleLineCount * 2) + 1] == bitToCompare) ? 0x1 : 0x0);

                brailleCodes[index] = value;

                Console.WriteLine("\t{0} {1}", value, Convert.ToString(value, 2));
            }

            return brailleCodes;
        }

        static void PrintBrailleOutput(int[] brailleCodes, int[] characterBrailleBinding, List<string> nonLatinAlphabet)
        {
            Console.Write("Output Text Data: \n\t");

            for (int index = 0, charCounter = 0; charCounter < brailleCodes.Length; )
            {
                int brailleCode = brailleCodes[charCounter];

                if (!(index < characterBrailleBinding.Length))  //Continue character for out-of-bound comparison
                {
                    //Look into non-alphabet
                    string nonAlphaChar = nonLatinAlphabet[brailleCode];

                    if (string.IsNullOrEmpty(nonAlphaChar))
                        Console.Write("<NotFound=" + brailleCode + ">");
                    else
                        Console.Write(nonAlphaChar);
                    //

                    charCounter++;
                    index = 0;

                    continue;
                }

                if (characterBrailleBinding[index] == brailleCode)
                {
                    Console.Write(GetCharacter('a', index));
                    charCounter++;
                    index = 0;

                    continue;
                }

                index++;
            }
        }

        static char[] GetSampleData()
        {
            //StringBuilder data = new StringBuilder();

            //data.AppendLine("O. O. O. O. O. .O O. O. O. OO");
            //data.AppendLine("OO .O O. O. .O OO .O OO O. .O");
            //data.AppendLine(".. .. O. O. O. .O O. O. O. ..");

            //Debug.WriteLine(data.ToString());
            //Debug.WriteLine(SampleData.Braille_HelloWorld);

            //return data.ToString().ToArray();
            string path = Path.Combine(System.Environment.CurrentDirectory, global::HTP.Properties.Settings.Default.Path_BrailleHelloWorld);
            StringReader reader = new StringReader(File.ReadAllText(path));
            char[] data = reader.ReadToEnd().ToCharArray();

            return data;
        }

        static char[] GetChallengeData()
        {
            //StringBuilder data = new StringBuilder();

            //data.AppendLine(".O O. .O OO O. O. .O OO O. OO O. .O O. .O");
            //data.AppendLine("O. .O O. .. .. OO OO .. .. .. OO OO .O OO");
            //data.AppendLine("O. O. O. O. .. O. O. O. OO .. .. .O O. .O");

            //return data.ToString().ToArray();

            string path = Path.Combine(System.Environment.CurrentDirectory, global::HTP.Properties.Settings.Default.Path_BrailleSampleData);
            StringReader reader = new StringReader(File.ReadAllText(path));
            char[] data = reader.ReadToEnd().ToCharArray();

            return data;
        }

        static char GetCharacter(int firstAlpha, int index)
        {
            return (char) (firstAlpha + index);
        }

        static int GetCharacterIndex(int alphaSize, int lastAlpha, int charAscii)
        {
            return (alphaSize - ((lastAlpha - charAscii) + 1));   //Base '0'
        }

        static int GetBrailleBinaryValue(int cell, int decade)
        {
            int brailleBinaryValue = 0x0;

            brailleBinaryValue = (cell << 2) | decade;

            return brailleBinaryValue;
        }

        static int GetAlphaRangeSize(char firstAlpha, char lastAlpha)
        {
            return (lastAlpha - firstAlpha) + 1;        //Base '0'
        }

        static int GetDecadeBinaryValue(int decade)
        {
            //Set bit from the left-to-right
            int bitPositionBoundary = 2; //Limit manipulation only to the last 2 bits
            //Expected decade values to binary: 0 = 0000, 1 = 0010, 2 = 0011
            //0x3 = 0011
            int decadeBinaryValue = (0x3 >> (bitPositionBoundary - decade)) + (decade % bitPositionBoundary);

            return decadeBinaryValue;
        }

        static int GetCellBinaryValue(int cell)
        {
            int cellBinaryValue = 0x0;      //Cell value bit-setting

            switch (cell)
            {
                case 1:
                    cellBinaryValue = 0x8;
                    break;
                case 2:
                    cellBinaryValue = 0xA;
                    break;
                case 3:
                    cellBinaryValue = 0xC;
                    break;
                case 4:
                    cellBinaryValue = 0xD;
                    break;
                case 5:
                    cellBinaryValue = 0x9;
                    break;
                case 6:
                    cellBinaryValue = 0xE;
                    break;
                case 7:
                    cellBinaryValue = 0xF;
                    break;
                case 8:
                    cellBinaryValue = 0xB;
                    break;
                case 9:
                    cellBinaryValue = 0x6;
                    break;
                case 0:
                    cellBinaryValue = 0x7;
                    break;
            }

            return cellBinaryValue;
        }
    }
}
