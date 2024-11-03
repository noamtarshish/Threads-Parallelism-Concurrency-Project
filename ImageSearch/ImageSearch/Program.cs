using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;

class ImageSearch
{
    //Helper Function that load the image and convert it to Bitmap to work with the pixels
    static Color[][] LoadImage(string path)
    {
        Bitmap bitmap = new Bitmap(path);
        Color[][] pixels = new Color[bitmap.Height][];
        for (int y = 0; y < bitmap.Height; y++)
        {
            pixels[y] = new Color[bitmap.Width];
            for (int x = 0; x < bitmap.Width; x++)
            {
                pixels[y][x] = bitmap.GetPixel(x, y);
            }
        }
        Console.WriteLine($"Loaded image from {path} with dimensions {bitmap.Width}x{bitmap.Height}");
        return pixels;
    }

    // Algorithm for Exact
    static bool ExactMatch(Color[][] bigImage, Color[][] smallImage, int startX, int startY)
    {
        for (int y = 0; y < smallImage.Length; y++)
        {
            for (int x = 0; x < smallImage[0].Length; x++)
            {
                if (bigImage[startY + y][startX + x] != smallImage[y][x])
                {
                    return false;
                }
            }
        }
        return true;
    }

    // Algorithm for Euclidean
    static bool EuclideanMatch(Color[][] bigImage, Color[][] smallImage, int startX, int startY)
    {
        for (int y = 0; y < smallImage.Length; y++)
        {
            for (int x = 0; x < smallImage[0].Length; x++)
            {
                Color largePixel = bigImage[startY + y][startX + x];
                Color smallPixel = smallImage[y][x];
                double distance = Math.Sqrt(
                    Math.Pow(largePixel.R - smallPixel.R, 2) +
                    Math.Pow(largePixel.G - smallPixel.G, 2) +
                    Math.Pow(largePixel.B - smallPixel.B, 2)
                );
                if (distance != 0)
                {
                    return false;
                }
            }
        }
        return true;
    }

    // Function to search the solution by given the original picture, smaller picture, number of threads and choosen algorithm
    static void SearchSection(Color[][] bigImage, Color[][] smallImage, int startRow, int endRow, List<(int, int)> matches, object lockObj, Func<Color[][], Color[][], int, int, bool> matchFunc)
    {
        for (int y = startRow; y <= endRow; y++)
        {
            for (int x = 0; x <= bigImage[0].Length - smallImage[0].Length; x++)
            {
                if (matchFunc(bigImage, smallImage, x, y))
                {
                    lock (lockObj)
                    {
                        matches.Add((x, y));
                    }
                }
            }
        }
    }

    // Using threads for parallel search
    static List<(int, int)> ParallelSearch(Color[][] bigImage, Color[][] smallImage, int numThreads, Func<Color[][], Color[][], int, int, bool> matchFunc)
    {
        List<(int, int)> allMatches = new List<(int, int)>();
        List<Thread> threads = new List<Thread>();
        int sectionHeight = (bigImage.Length - smallImage.Length + 1 + numThreads - 1) / numThreads; // Ceiling division
        object lockObj = new object();

        for (int i = 0; i < numThreads; i++)
        {
            int startRow = i * sectionHeight;
            int endRow = Math.Min(startRow + sectionHeight - 1, bigImage.Length - smallImage.Length);

            Thread thread = new Thread(() =>
            {
                SearchSection(bigImage, smallImage, startRow, endRow, allMatches, lockObj, matchFunc);
            });
            threads.Add(thread);
            thread.Start();
            Console.WriteLine($"Started thread {thread.ManagedThreadId} for section starting at {startRow} to {endRow}");
        }

        foreach (var thread in threads)
        {
            thread.Join();
        }

        return allMatches;
    }

    static void Main(string[] args)
    {
        if (args.Length != 4)
        {
            Console.WriteLine("Invalid parameters.");
            Console.WriteLine("Usage: ImageSearch <image1> <image2> <nThreads> <algorithm>");
            return;
        }

        string image1Path = args[0];
        string image2Path = args[1];
        if (!int.TryParse(args[2], out int numThreads) || numThreads < 1)
        {
            Console.WriteLine("Invalid number of threads.");
            return;
        }

        string algorithm = args[3];
        Func<Color[][], Color[][], int, int, bool> matchFunc;

        // Choosing the Algorithm 
        if (algorithm == "exact")
        {
            matchFunc = ExactMatch;
        }
        else if (algorithm == "euclidian")
        {
            matchFunc = EuclideanMatch;
        }
        else
        {
            Console.WriteLine("Unsupported algorithm. Use 'exact' or 'euclidian'.");
            return;
        }

        if (!File.Exists(image1Path) || !File.Exists(image2Path))
        {
            Console.WriteLine("One or both image files do not exist.");
            return;
        }
        
        Color[][] bigImage = LoadImage(image1Path);
        Color[][] smallImage = LoadImage(image2Path);

        var matches = ParallelSearch(bigImage, smallImage, numThreads, matchFunc);

        foreach (var match in matches)
        {
            Console.WriteLine($"{match.Item1},{match.Item2}");
        }

        Console.WriteLine($"Total matches found: {matches.Count}");
    }
}
