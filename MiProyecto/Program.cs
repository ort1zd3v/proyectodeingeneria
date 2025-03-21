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
using System.Globalization;

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
        string logFilePath7 = Path.Combine(outputFolderPath, "a7_matricula.txt");
        string tokenizedFilePath = Path.Combine(outputFolderPath, "diccionario.txt");
        string postingFilePath = Path.Combine(outputFolderPath, "posting.txt");

        Dictionary<string, (int count, HashSet<string> files)> tokenStats = new(StringComparer.OrdinalIgnoreCase);

        string[] specificFiles = { "simple.html", "medium.html", "hard.html" };

        using (StreamWriter logFile5 = new StreamWriter(logFilePath5))
        using (StreamWriter logFile6 = new StreamWriter(logFilePath6))
        using (StreamWriter logFile7 = new StreamWriter(logFilePath7))
        {
            Stopwatch totalStopwatch = Stopwatch.StartNew();

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
                }
            }

            Stopwatch dictStopwatch = Stopwatch.StartNew();
            GenerateTokenDictionary(tokenStats, tokenizedFilePath, postingFilePath);
            dictStopwatch.Stop();

            logFile7.WriteLine($"Tiempo para generar diccionario y posting: {dictStopwatch.ElapsedMilliseconds} ms");

            totalStopwatch.Stop();
            logFile5.WriteLine($"Tiempo total de ejecución: {totalStopwatch.ElapsedMilliseconds} ms");
            logFile6.WriteLine($"Tiempo total de ejecución: {totalStopwatch.ElapsedMilliseconds} ms");
            logFile7.WriteLine($"Tiempo total de ejecución: {totalStopwatch.ElapsedMilliseconds} ms");
        }

        Console.WriteLine("Proceso completado. Revisa los archivos en el directorio de salida.");
    }

    static string RemoveHtmlTags(string fileName)
    {
        try
        {
            string content = File.ReadAllText(fileName);
            string cleanedContent = Regex.Replace(content, "<.*?>", string.Empty);
            cleanedContent = cleanedContent.Normalize(NormalizationForm.FormC);

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
            string[] words = Regex.Split(content, "[^\p{L}\p{N}-]+")
                                  .Where(w => !string.IsNullOrEmpty(w))
                                  .Select(w => w.ToLowerInvariant())
                                  .ToArray();
            Array.Sort(words, StringComparer.OrdinalIgnoreCase);
            return words;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al extraer palabras de {fileName}: {ex.Message}");
            return Array.Empty<string>();
        }
    }

    static void GenerateTokenDictionary(Dictionary<string, (int count, HashSet<string> files)> tokens, string dictPath, string postingPath)
    {
        using (StreamWriter dictWriter = new StreamWriter(dictPath))
        using (StreamWriter postingWriter = new StreamWriter(postingPath))
        {
            dictWriter.WriteLine("Token|Repeticiones|# de archivos");
            postingWriter.WriteLine("Token|Archivos");

            foreach (var kvp in tokens.OrderBy(kvp => kvp.Key, StringComparer.OrdinalIgnoreCase))
            {
                dictWriter.WriteLine($"{kvp.Key}|{kvp.Value.count}|{kvp.Value.files.Count}");
                postingWriter.WriteLine($"{kvp.Key}|{string.Join(",", kvp.Value.files)}");
            }
        }
    }
}