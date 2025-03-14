/*
Nombre: Angel Escalante Garza 
Matricula: 3067907
-----------------------------
Nombre: Armando Emiliano Martinez Zamora
Matricula: 2836763
-----------------------------
Nombre: Hazel Isaac Ortiz Jiménez 
Matricula: 2891135
-----------------------------
Nombre: Roberto Moreno Almaguer
Matricula: 3047031
-----------------------------
*/
//Ejemplo de ejecucion visual studio code: dotnet run -- "/Users/angelescalantegarza/Documents/Proyectos Software/FASE 1/MiProyecto/files" "/Users/angelescalantegarza/Documents/Proyectos Software/FASE 1/MiProyecto/output"
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Uso: Tokenize.exe <input-directory> <output-directory>");
            return;
        }

        string inputFolderPath = args[0];
        string outputFolderPath = args[1];

        if (!Directory.Exists(outputFolderPath))
        {
            Directory.CreateDirectory(outputFolderPath);
        }

        string logFilePath5 = Path.Combine(outputFolderPath, "a5_matricula.txt");
        string logFilePath6 = Path.Combine(outputFolderPath, "a6_matricula.txt");
        string tokenizedFilePath = Path.Combine(outputFolderPath, "tokens_dictionary.txt");

        Dictionary<string, (int count, HashSet<string> files)> tokenStats = new(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, int> consolidatedTokens = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        string[] specificFiles = { "simple.html", "medium.html", "hard.html"};

        using (StreamWriter logFile5 = new StreamWriter(logFilePath5))
        using (StreamWriter logFile6 = new StreamWriter(logFilePath6))
        {
            Stopwatch totalStopwatch = Stopwatch.StartNew();

            // Procesar todos los archivos para generar archivos cleaned_word
            string[] allFiles = Directory.GetFiles(inputFolderPath, "*.html");
            foreach (string file in allFiles)
            {
                string cleanedFile = RemoveHtmlTags(file);
                ExtractAndSortWords(cleanedFile, outputFolderPath);
            }

            // Procesar solo los archivos específicos para el diccionario y tokens
            foreach (string file in specificFiles)
            {
                string fullFilePath = Path.Combine(inputFolderPath, file);

                if (!File.Exists(fullFilePath))
                {
                    Console.WriteLine($"El archivo {file} no existe en el directorio de entrada.");
                    continue;
                }

                Stopwatch fileStopwatch = Stopwatch.StartNew();
                string cleanedFile = RemoveHtmlTags(fullFilePath);
                string[] words = ExtractAndSortWords(cleanedFile, outputFolderPath);
                fileStopwatch.Stop();

                logFile6.WriteLine($"Archivo: {file} - Tiempo procesamiento: {fileStopwatch.ElapsedMilliseconds} ms");

                foreach (string word in words)
                {
                    if (tokenStats.ContainsKey(word))
                    {
                        tokenStats[word] = (tokenStats[word].count + 1, tokenStats[word].files);
                        tokenStats[word].files.Add(file);
                    }
                    else
                    {
                        tokenStats[word] = (1, new HashSet<string> { file });
                    }

                    if (consolidatedTokens.ContainsKey(word))
                    {
                        consolidatedTokens[word]++;
                    }
                    else
                    {
                        consolidatedTokens[word] = 1;
                    }
                }
            }

            Stopwatch dictStopwatch = Stopwatch.StartNew();
            GenerateTokenDictionary(tokenStats, tokenizedFilePath, '|');
            dictStopwatch.Stop();

            logFile6.WriteLine($"Tiempo para generar el diccionario de tokens: {dictStopwatch.ElapsedMilliseconds} ms");

            Stopwatch alphaStopwatch = Stopwatch.StartNew();
            GenerateConsolidatedTokensFile(consolidatedTokens, outputFolderPath, "consolidated_tokens_alpha.txt", true);
            alphaStopwatch.Stop();
            logFile5.WriteLine($"Tiempo para generar archivo consolidado (orden alfabético): {alphaStopwatch.ElapsedMilliseconds} ms");

            Stopwatch freqStopwatch = Stopwatch.StartNew();
            GenerateConsolidatedTokensFile(consolidatedTokens, outputFolderPath, "consolidated_tokens_freq.txt", false);
            freqStopwatch.Stop();
            logFile5.WriteLine($"Tiempo para generar archivo consolidado (orden por frecuencia): {freqStopwatch.ElapsedMilliseconds} ms");

            totalStopwatch.Stop();
            logFile5.WriteLine($"Tiempo total de ejecución: {totalStopwatch.ElapsedMilliseconds} ms");
            logFile6.WriteLine($"Tiempo total de ejecución: {totalStopwatch.ElapsedMilliseconds} ms");
        }

        Console.WriteLine("Proceso completado. Revisa los archivos en el directorio de salida.");
    }

    static string RemoveHtmlTags(string fileName)
    {
        try
        {
            string content = File.ReadAllText(fileName);
            string cleanedContent = Regex.Replace(content, "<.*?>", string.Empty);
            string directory = Path.GetDirectoryName(fileName) ?? "./";
            string newFileName = Path.Combine(directory, Path.GetFileNameWithoutExtension(fileName) + "_cleaned.txt");
            File.WriteAllText(newFileName, cleanedContent);
            return newFileName;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al limpiar {fileName}: {ex.Message}");
            return string.Empty;
        }
    }

    static string[] ExtractAndSortWords(string fileName, string outputFolderPath)
    {
        try
        {
            string content = File.ReadAllText(fileName);
            string[] words = Regex.Split(content, "[^\\w-]+").Where(w => !string.IsNullOrEmpty(w)).ToArray();
            Array.Sort(words, StringComparer.OrdinalIgnoreCase);

            string wordFileName = Path.Combine(outputFolderPath, Path.GetFileNameWithoutExtension(fileName) + "_words.txt");
            File.WriteAllLines(wordFileName, words);

            return words;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al extraer palabras de {fileName}: {ex.Message}");
            return Array.Empty<string>();
        }
    }

    static void GenerateConsolidatedTokensFile(Dictionary<string, int> tokens, string outputFolderPath, string fileName, bool sortByAlpha)
    {
        try
        {
            string consolidatedFilePath = Path.Combine(outputFolderPath, fileName);

            IEnumerable<KeyValuePair<string, int>> sortedTokens;
            if (sortByAlpha)
            {
                sortedTokens = tokens.OrderBy(kvp => kvp.Key, StringComparer.OrdinalIgnoreCase);
            }
            else
            {
                sortedTokens = tokens.OrderByDescending(kvp => kvp.Value).ThenBy(kvp => kvp.Key, StringComparer.OrdinalIgnoreCase);
            }

            using (StreamWriter writer = new StreamWriter(consolidatedFilePath))
            {
                foreach (var kvp in sortedTokens)
                {
                    writer.WriteLine($"{kvp.Key}: {kvp.Value}");
                }
            }

            Console.WriteLine($"Archivo consolidado generado: {consolidatedFilePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al generar el archivo consolidado: {ex.Message}");
        }
    }

    static void GenerateTokenDictionary(Dictionary<string, (int count, HashSet<string> files)> tokens, string filePath, char separator)
    {
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            writer.WriteLine($"Token{separator}Repeticiones{separator}# de archivos");

            foreach (var kvp in tokens)
            {
                writer.WriteLine($"{kvp.Key}{separator}{kvp.Value.count}{separator}{kvp.Value.files.Count}");
            }
        }
    }
}