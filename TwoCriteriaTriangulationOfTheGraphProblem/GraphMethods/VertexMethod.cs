﻿using System.Collections.Generic;
using TwoCriteriaTriangulationOfTheGraphProblem.GraphElements;

namespace TwoCriteriaTriangulationOfTheGraphProblem.GraphMethods
{
    public static class VertexMethod
    {
        //wykorzystywane w naprawie, sprawdza i aktualizuje stopnie wierzchołków
        public static Stack<Vertex>[] GetVertexDegreeInfo(double[][] matrix, List<Vertex> verticesList)
        {
            Stack<Vertex>[] tempArray = new Stack<Vertex>[3];
            tempArray[0] = new Stack<Vertex>();//przechowuje wierzchołki o 0 stopniu
            tempArray[1] = new Stack<Vertex>();//przechowuje wierzchołki o nieparzystym stopniu
            tempArray[2] = new Stack<Vertex>();//przechowuje wierzchołki o parzystym stopniu
            CalculateTheSum(matrix, verticesList);//aktualizacja sumy w macierzy i stopni wierzchołków

            for (int i = 0; i < matrix.Length; i++)
            {
                var lastColumn = matrix[i].Length - 1;
                //0 stopien
                if (matrix[i][lastColumn] == 0)
                {
                    tempArray[0].Push(verticesList[i]);
                }
                //nieparzyste
                if (matrix[i][lastColumn] % 2 == 1)
                {
                    tempArray[1].Push(verticesList[i]);
                }
                //parzyste
                if (matrix[i][lastColumn] % 2 == 0)
                {
                    tempArray[2].Push(verticesList[i]);
                }
            }

            return tempArray;
        }

        public static void AddVertexGroupInfo(Graph graph, Dictionary<Vertex, int> groupsVertices)
        {
            foreach (var vertex in graph.Vertices)
            {
                vertex.Tooltip = $"Group: {groupsVertices[vertex]}";
            }
        }

        //wpisanie sąsiednich wierzchołków do listy sąsiadów<=wykorzystywane w naprawie
        public static void SetVertexNeighbors(double[][] matrix, List<Vertex> existingVertices)
        {
            for (int i = 0; i < matrix.Length; i++)
            {
                existingVertices[i].Neighbors = new List<Vertex>();

                for (int k = 0; k < matrix[i].Length - 1; k++)
                {
                    if (matrix[i][k] == 1)
                    {
                        existingVertices[i].Neighbors.Add(existingVertices[k]);
                    }
                }
            }
        }

        //aktualizacja sumy w macierzy i stopni wierzchołków
        public static void CalculateTheSum(double[][] matrix, List<Vertex> existingVertices)
        {
            var numberOfVertices = matrix.Length;

            for (int i = 0; i < numberOfVertices; i++)
            {
                double sum = 0;

                for (int j = 0; j < numberOfVertices; j++)
                {
                    sum += matrix[i][j];
                }

                matrix[i][numberOfVertices] = sum;
                existingVertices[i].VertexDegree = (int)sum;
            }
        }
    }
}
