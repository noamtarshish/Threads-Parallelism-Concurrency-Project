using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class MTMergeSort
{
    public List<string> MergeSort(string[] strList, int nMin = 2)
    {
        if (strList.Length <= nMin)
        {
            // Base case: If the list size is less than or equal to nMin, sort it directly
            return strList.OrderBy(s => s).ToList();
        }

        int mid = strList.Length / 2;
        string[] leftArray = strList.Take(mid).ToArray();
        string[] rightArray = strList.Skip(mid).ToArray();

        Task<List<string>> leftTask = Task.Run(() => MergeSort(leftArray, nMin));
        Task<List<string>> rightTask = Task.Run(() => MergeSort(rightArray, nMin));
        Task.WaitAll(leftTask, rightTask);
        return Merge(leftTask.Result, rightTask.Result);
    }

    // Helper method to merge two sorted lists
    private List<string> Merge(List<string> left, List<string> right)
    {
        List<string> result = new List<string>();
        int i = 0, j = 0;

        while (i < left.Count && j < right.Count)
        {
            if (string.Compare(left[i], right[j]) <= 0)
            {
                result.Add(left[i]);
                i++;
            }
            else
            {
                result.Add(right[j]);
                j++;
            }
        }
        result.AddRange(left.Skip(i));
        result.AddRange(right.Skip(j));

        return result;
    }
}