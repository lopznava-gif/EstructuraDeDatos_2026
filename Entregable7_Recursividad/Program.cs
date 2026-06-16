using System;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== EJERCICIO B: SUMATORIA RECURSIVA ===");
        Console.Write("Introduce un número positivo: ");
        string? entrada = Console.ReadLine();

        // Validación profesional: Verifica que sea número y mayor a 0
        if (int.TryParse(entrada, out int numero) && numero > 0) 
        {     
            int resultado = SumarHasta(numero);     
            Console.WriteLine($"\nLa suma de 1 hasta {numero} es: {resultado}");
        } 
        else 
        {     
            Console.ForegroundColor = ConsoleColor.Red;     
            Console.WriteLine("\nError: Solo se aceptan enteros positivos.");     
            Console.ResetColor(); 
        }
    }

    // --- La Función Recursiva ---
    static int SumarHasta(int n) 
    {
        // CASO BASE: La suma de 1 número es 1 (Aquí se detiene la recursión)
        if (n == 1)         
            return 1;
        
        // CASO RECURSIVO: n más la suma de todo lo anterior
        return n + SumarHasta(n - 1);
    }
}