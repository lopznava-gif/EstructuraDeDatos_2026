using System;

class Program 
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== Algoritmos Recursivos ===\n");

        // --- Factorial ---
        Console.Write("Ingresa un número para calcular su factorial: ");
        if (int.TryParse(Console.ReadLine(), out int numFactorial))
        {
            try
            {
                long resultado = CalcularFactorial(numFactorial);
                Console.WriteLine($"{numFactorial}! = {resultado}");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        // --- Fibonacci ---
        Console.Write("\nIngresa la posición de Fibonacci (n): ");
        if (int.TryParse(Console.ReadLine(), out int numFib))
        {
            try
            {
                long fib = GenerarFibonacci(numFib);
                Console.WriteLine($"Fibonacci({numFib}) = {fib}");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }

    // PROBLEMA 1: Función recursiva para calcular el Factorial
    static long CalcularFactorial(int n) 
    {
        // Validación de entrada
        if (n < 0)
            throw new ArgumentException("No existe factorial de negativos.");

        // Caso Base
        if (n == 0 || n == 1)
            return 1;

        // Caso Recursivo
        return n * CalcularFactorial(n - 1);
    }

    // PROBLEMA 2: Función recursiva para la secuencia de Fibonacci
    static long GenerarFibonacci(int n) 
    {
        // Validación de entrada
        if (n < 0)
            throw new ArgumentException("n debe ser un entero positivo.");

        // Caso Base 1
        if (n == 0) return 0;

        // Caso Base 2
        if (n == 1) return 1;

        // Caso Recursivo doble
        return GenerarFibonacci(n - 1) + GenerarFibonacci(n - 2);
    }
}