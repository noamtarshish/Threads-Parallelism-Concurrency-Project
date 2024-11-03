using System;
using System.Threading;

class MatrixMultiplier
{
    public static void MultiplyMatricesConcurrently(int[,] matrixA, int[,] matrixB, int[,] resultMatrix, int rowsA, int colsA, int colsB, int numThreads)
    {
        int rowsPerThread = rowsA / numThreads;
        Thread[] threads = new Thread[numThreads];

        for (int i = 0; i < numThreads; i++)
        {
            int startRow = i * rowsPerThread;
            int endRow = (i == numThreads - 1) ? rowsA : startRow + rowsPerThread;
            threads[i] = new Thread(() =>
            {
                Thread.Sleep(30000); 
                MultiplyPartial(matrixA, matrixB, resultMatrix, startRow, endRow, colsA, colsB);
            });
            threads[i].Start();
            Console.WriteLine($"Thread {threads[i].ManagedThreadId} started.");
            Thread.Sleep(500); 
        }

        Console.WriteLine("Delaying after starting threads..."); //We add delay that we can track easier about the threads
        Thread.Sleep(40000); 

        foreach (var thread in threads)
        {
            thread.Join();
        }

        Console.WriteLine("Delaying after computation...");
        Thread.Sleep(60000); //We add another delay to catch 20 threads in the task manager
    }

    //Function that multiply the matrices
    private static void MultiplyPartial(int[,] matrixA, int[,] matrixB, int[,] resultMatrix, int startRow, int endRow, int colsA, int colsB)
    {
        Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} performing computation.");
        for (int i = startRow; i < endRow; i++)
        {
            for (int j = 0; j < colsB; j++)
            {
                resultMatrix[i, j] = 0;
                for (int k = 0; k < colsA; k++)
                {
                    resultMatrix[i, j] += matrixA[i, k] * matrixB[k, j];
                }
            }
        }
        Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} completed computation."); //We add line printed to see when every thread end it comutation to track about it in task manager
        Thread.Sleep(5000); 
    }
}
