using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class DistanceMatrix
{
    private double[,] distanceMatrix;

    // Конструктор, который принимает двумерный массив расстояний
    public DistanceMatrix(double[,] matrix)
    {
        distanceMatrix = matrix;
    }

    // Метод для получения расстояния между двумя вершинами
    public double GetDistance(int from, int to)
    {
        if (from < 0 || from >= distanceMatrix.GetLength(0) || to < 0 || to >= distanceMatrix.GetLength(1))
        {
            throw new IndexOutOfRangeException($"Индексы {from}, {to} выходят за пределы массива.");
        }
        return distanceMatrix[from, to];
    }

    // Метод для получения общего числа вершин (размерности матрицы)
    public int GetVertexCount()
    {
        return distanceMatrix.GetLength(0); // Количество строк (вершин)
    }
}

public class SimulatedAnnealing
{
    private DistanceMatrix distanceMatrix;
    private Random randomizer = new Random();
    private double initialTemperature = 1000;
    private double coolingRate = 0.995;
    private int iterationsPerTemperature = 1000;

    // Конструктор
    public SimulatedAnnealing(double[,] distanceMatrix)
    {
        this.distanceMatrix = new DistanceMatrix(distanceMatrix); // Инициализация DistanceMatrix
    }

    public List<int> FindOptimalHamiltonianCycle()
    {
        List<int> currentSolution = GenerateInitialSolution();
        double currentEnergy = CalculateRouteLength(currentSolution);

        List<int> bestSolution = new List<int>(currentSolution);
        double bestEnergy = currentEnergy;

        double temperature = initialTemperature;

        while (temperature > 1)
        {
            for (int i = 0; i < iterationsPerTemperature; i++)
            {
                List<int> newSolution = GetNeighboringSolution(currentSolution);
                double newEnergy = CalculateRouteLength(newSolution);

                if (AcceptanceProbability(currentEnergy, newEnergy, temperature) > randomizer.NextDouble())
                {
                    currentSolution = newSolution;
                    currentEnergy = newEnergy;
                }

                if (currentEnergy < bestEnergy)
                {
                    bestSolution = newSolution;
                    bestEnergy = currentEnergy;
                }
            }

            temperature *= coolingRate;  // Охлаждение
        }

        return bestSolution;
    }

    public double CalculateRouteLength(List<int> route)
    {
        double length = 0;
        for (int i = 0; i < route.Count - 1; i++)
        {
            length += distanceMatrix.GetDistance(route[i], route[i + 1]);
        }
        length += distanceMatrix.GetDistance(route.Last(), route.First());  // Замыкание цикла
        return length;
    }

    private List<int> GenerateInitialSolution()
    {
        List<int> solution = Enumerable.Range(0, distanceMatrix.GetVertexCount()).ToList();
        return solution;
    }

    private List<int> GetNeighboringSolution(List<int> solution)
    {
        List<int> newSolution = new List<int>(solution);
        int i = randomizer.Next(newSolution.Count);
        int j = randomizer.Next(newSolution.Count);
        while (i == j)
        {
            j = randomizer.Next(newSolution.Count);
        }

        // Перестановка двух случайных вершин
        int temp = newSolution[i];
        newSolution[i] = newSolution[j];
        newSolution[j] = temp;

        return newSolution;
    }

    private double AcceptanceProbability(double currentEnergy, double newEnergy, double temperature)
    {
        if (newEnergy < currentEnergy)
        {
            return 1.0;  // Всегда принимаем, если новый путь лучше
        }
        return Math.Exp((currentEnergy - newEnergy) / temperature);
    }
}

public class EntryPoint
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Нахождение кратчайшего гамильтонова цикла с использованием алгоритма отжига");
        Console.WriteLine("Пожалуйста, проверьте наличие и формат текстового файла с матрицей смежности для графа.");
        Console.WriteLine("Пожалуйста, введите путь к файлу:");

        string filePath = Console.ReadLine();

        // Чтение матрицы смежности из файла
        double[,] adjacencyMatrix = ReadAdjacencyMatrixFromFile(filePath);

        // Использование алгоритма отжига для поиска оптимального гамильтонова цикла
        SimulatedAnnealing optimization = new SimulatedAnnealing(adjacencyMatrix);
        List<int> optimalPath = optimization.FindOptimalHamiltonianCycle();

        // Вывод результата
        Console.WriteLine("Кратчайший путь: " + (optimalPath != null ? string.Join(" -> ", optimalPath) : "Путь не найден"));
        Console.WriteLine("Длина пути: " + (optimalPath != null ? optimization.CalculateRouteLength(optimalPath).ToString() : "Путь не найден"));
    }

    public static double[,] ReadAdjacencyMatrixFromFile(string filePath)
    {
        string[] lines = File.ReadAllLines(filePath);
        int size = lines.Length;

        // Убираем пустые строки
        lines = lines.Where(line => !string.IsNullOrWhiteSpace(line)).ToArray();
        size = lines.Length;

        // Создаем матрицу с размером, соответствующим количеству строк
        double[,] matrix = new double[size, size];

        for (int i = 0; i < size; i++)
        {
            string[] values = lines[i].Split(',');

            // Логируем строку для диагностики
            Console.WriteLine($"Строка {i + 1}: {lines[i]}");

            // Проверка, что количество значений в строке соответствует размерности матрицы
            if (values.Length != size)
            {
                throw new InvalidOperationException($"Количество столбцов в строке {i + 1} ({values.Length}) не совпадает с количеством строк в матрице ({size}).");
            }

            for (int j = 0; j < size; j++)
            {
                string value = values[j].Trim();
                if (value == "inf")
                {
                    matrix[i, j] = double.PositiveInfinity;
                }
                else
                {
                    matrix[i, j] = double.Parse(value);
                }
            }
        }

        return matrix;
    }
}
