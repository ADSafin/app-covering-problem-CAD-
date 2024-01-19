using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Drawing;
using Color = System.Drawing.Color;
using Image = System.Windows.Controls.Image;
using Pen = System.Drawing.Pen;
using Application = System.Windows.Application;
using System.Windows.Forms;

namespace modules_seats;

public class Settes
{
    // Функция Конвертации изображения
    public static BitmapImage ConvertImg(System.Drawing.Image src)
    {
        var ms = new MemoryStream();
        src.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
        var image = new BitmapImage();
        image.BeginInit();
        ms.Seek(0, SeekOrigin.Begin);
        image.StreamSource = ms;
        image.EndInit();
        return image;
    }
    
    private static int FindMinDistance(int[] distances, bool[] shortestPathTreeSet, int numVertices)
    {
        int minDistance = int.MaxValue;
        int minIndex = 0;

        for (int v = 0; v < numVertices; v++)
        {
            if (!shortestPathTreeSet[v] && distances[v] <= minDistance)
            {
                minDistance = distances[v];
                minIndex = v;
            }
        }

        return minIndex;
    }
    
    public static int DijkstraAlgorithm(int[,] graph, int source, int target)
    {
        int numVertices = graph.GetLength(0);
        int[] distances = new int[numVertices];
        bool[] shortestPathTreeSet = new bool[numVertices];

        for (int i = 0; i < numVertices; i++)
        {
            distances[i] = int.MaxValue;
            shortestPathTreeSet[i] = false;
        }

        distances[source] = 0;

        for (int count = 0; count < numVertices - 1; count++)
        {
            int u = FindMinDistance(distances, shortestPathTreeSet, numVertices);
            shortestPathTreeSet[u] = true;

            for (int v = 0; v < numVertices; v++)
            {
                if (!shortestPathTreeSet[v] && graph[u, v] != 0 &&
                    distances[u] != int.MaxValue &&
                    distances[u] + graph[u, v] < distances[v])
                {
                    distances[v] = distances[u] + graph[u, v];
                }
            }
        }

        return distances[target];
    }

    public static int CalculateTotalLength(int[,] matrix, List<int> points)
    {
        int totalLength = 0;

        for (int i = 0; i < points.Count - 1; i++)
        {
            int startPoint = points[i] - 1; // Индекс начальной точки в матрице
            int endPoint = points[i + 1] - 1; // Индекс конечной точки в матрице

            totalLength += matrix[startPoint, endPoint];
        }

        return totalLength;
    }
}