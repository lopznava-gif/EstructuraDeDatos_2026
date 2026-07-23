using System;

class Program
{
    static void Main()
    {
        try
        {
            // Módulo 1: Inicialización aleatoria
            int[] calificaciones = new int[4];
            Random rng = Random.Shared;
            
            for (int i = 0; i < calificaciones.Length; i++)
            {
                calificaciones[i] = rng.Next(0, 101); // Valores del 0 al 100 inclusive
            }

            Console.WriteLine("=== Estado inicial: calificaciones desordenadas ===");
            ImprimirArreglo(calificaciones);

            // Módulo 2: Llamada al algoritmo de ordenamiento
            OrdenarPorBurbuja(calificaciones);

            // Módulo 3: Salida final
            Console.WriteLine("\n=== Estado final: calificaciones ordenadas (menor a mayor) ===");
            ImprimirArreglo(calificaciones);
        }
        catch (IndexOutOfRangeException ex)
        {
            Console.WriteLine($"[ERROR] Índice fuera de rango detectado: {ex.Message}");
            Console.WriteLine("Revisa los límites de tus ciclos for anidados.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR inesperado]: {ex.Message}");
        }
    }

    static void ImprimirArreglo(int[] arr)
    {
        Console.WriteLine(string.Join(", ", arr));
    }

    // Algoritmo Bubble Sort con bandera de optimización y contadores
    static void OrdenarPorBurbuja(int[] arr)
    {
        int n = arr.Length;
        int contadorIntercambios = 0;
        int contadorComparaciones = 0;
        bool swapped;

        for (int i = 0; i < n - 1; i++)
        {
            swapped = false;
            for (int j = 0; j < n - i - 1; j++)
            {
                contadorComparaciones++;
                if (arr[j] > arr[j + 1])
                {
                    int temp = arr[j];
                    arr[j] = arr[j + 1];
                    arr[j + 1] = temp;
                    
                    contadorIntercambios++;
                    swapped = true;
                }
            }
            if (!swapped)
                break;
        }

        Console.WriteLine($"\nTotal de intercambios realizados: {contadorIntercambios}");
        Console.WriteLine($"Total de comparaciones realizadas: {contadorComparaciones}");
    }
}