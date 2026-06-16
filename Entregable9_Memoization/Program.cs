using System;
using System.Diagnostics; 

class Program 
{     
    static void Main(string[] args)     
    {         
        Console.WriteLine("=== MÓDULO B: BANCO DE PRUEBAS FIBONACCI ===");
        Console.Write("Ingresa un número (35-43): ");
        string? input = Console.ReadLine();

        if (!int.TryParse(input, out int n) || n < 0) 
        {             
            Console.WriteLine("Error: ingresa un número positivo.");             
            return;         
        }

        Stopwatch sw = new Stopwatch();

        // --- Método Inseguro (Fuerza Bruta) ---        
        Console.WriteLine("\nCalculando con Fuerza Bruta...");
        sw.Restart();         
        long r1 = FibonacciInseguro(n);
        sw.Stop();         
        Console.WriteLine($"Inseguro: F({n}) = {r1}");         
        Console.WriteLine($"Tiempo: {sw.ElapsedMilliseconds} ms");

        // --- Método Pro (Memoization) ---
        // 1. Inicializamos la caché con -1 ("no calculado aún")
        Console.WriteLine("\nCalculando con Memoization (Caché)...");
        long[] cache = new long[n + 1];         
        for (int i = 0; i <= n; i++)             
            cache[i] = -1;

        // 2. Ejecutamos el método optimizado
        sw.Restart();         
        long r2 = FibonacciPro(n, cache);         
        sw.Stop();         
        Console.WriteLine($"Pro: F({n}) = {r2}");
        Console.WriteLine($"Tiempo: {sw.ElapsedMilliseconds} ms");     
    }

    // Función 1: Recursiva Base (Fuerza Bruta)
    public static long FibonacciInseguro(int n) 
    {     
        if (n == 0) return 0;     
        if (n == 1) return 1;
        return FibonacciInseguro(n - 1) + FibonacciInseguro(n - 2); 
    }

    // Función 2: Recursiva Optimizada (Memoization)
    public static long FibonacciPro(int n, long[] cache)
    {
        if (n == 0) return 0;
        if (n == 1) return 1;

        // ¿Ya lo calculamos antes? (Buscamos en la caché)
        if (cache[n] != -1)
            return cache[n]; // ¡Retorno inmediato, ahorramos millones de cálculos!

        // Calcular, guardar en caché y retornar
        cache[n] = FibonacciPro(n - 1, cache) + FibonacciPro(n - 2, cache);
        return cache[n];
    }
}