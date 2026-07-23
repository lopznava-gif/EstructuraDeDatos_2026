using System;
using System.Diagnostics;

class Program
{
    static void Main()
    {
        Console.WriteLine("=== SISTEMA DE BÚSQUEDA DE MATRÍCULAS ===");
        
        // CORRECCIÓN: Arreglo inicializado correctamente con 10,000 espacios
        int[] matriculas = new int[10000];
        for (int i = 0; i < 10000; i++)
        {
            matriculas[i] = 100000 + i;
        }

        Console.Write("\nIngresa la matrícula a buscar (ej. 105000 o 999999): ");
        if (!int.TryParse(Console.ReadLine(), out int objetivo))
        {
            Console.WriteLine("Entrada no válida. Debes ingresar un número entero.");
            return;
        }

        int iterLineal, iterBinaria;

        // 2. Búsqueda Lineal - O(n)
        Stopwatch sw = Stopwatch.StartNew();
        int idxLineal = BusquedaLineal(matriculas, objetivo, out iterLineal);
        sw.Stop();
        Console.WriteLine($"\n[Búsqueda Lineal / Complejidad O(n)]");
        Console.WriteLine($"Resultado: {(idxLineal != -1 ? $"Encontrado en el índice {idxLineal}" : "No encontrado")}");
        Console.WriteLine($"Iteraciones necesarias: {iterLineal}");
        Console.WriteLine($"Tiempo del procesador: {sw.ElapsedMilliseconds} ms");

        // 3. Búsqueda Binaria - O(log n)
        sw.Restart();
        int idxBinaria = BusquedaBinaria(matriculas, objetivo, out iterBinaria);
        sw.Stop();
        Console.WriteLine($"\n[Búsqueda Binaria / Complejidad O(log n)]");
        Console.WriteLine($"Resultado: {(idxBinaria != -1 ? $"Encontrado en el índice {idxBinaria}" : "No encontrado")}");
        Console.WriteLine($"Iteraciones necesarias: {iterBinaria}");
        Console.WriteLine($"Tiempo del procesador: {sw.ElapsedMilliseconds} ms");
    }

    // Algoritmo de Búsqueda Lineal
    static int BusquedaLineal(int[] arreglo, int objetivo, out int iteraciones)
    {
        iteraciones = 0;
        for (int i = 0; i < arreglo.Length; i++)
        {
            iteraciones++;
            if (arreglo[i] == objetivo)
                return i; // Lo encontró
        }
        return -1; // No lo encontró
    }

    // Algoritmo de Búsqueda Binaria
    static int BusquedaBinaria(int[] arreglo, int objetivo, out int iteraciones)
    {
        iteraciones = 0;
        int izquierda = 0;
        int derecha = arreglo.Length - 1;

        while (izquierda <= derecha)
        {
            iteraciones++;
            int medio = izquierda + (derecha - izquierda) / 2;

            if (arreglo[medio] == objetivo)
                return medio; // Lo encontró

            if (arreglo[medio] < objetivo)
                izquierda = medio + 1; // Buscar en la mitad derecha
            else
                derecha = medio - 1; // Buscar en la mitad izquierda
        }
        return -1; // No lo encontró
    }
}