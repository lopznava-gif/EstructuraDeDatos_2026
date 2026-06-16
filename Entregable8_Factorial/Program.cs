using System;
using System.Numerics; // ¡Librería obligatoria para alta precisión!

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== PARTE B: REFACTORIZACIÓN PROFESIONAL CON BIGINTEGER ===\n");
        
        // En Main — prueba con n = 100
        BigInteger resultado = FactorialProfesional(100);
        
        Console.WriteLine($"100! = {resultado}");
    }

    // --- Función Recursiva de Alta Precisión ---
    static BigInteger FactorialProfesional(BigInteger n) 
    {     
        // Caso Base: Usamos BigInteger.One en lugar de 1 para evitar conversiones implícitas
        if (n == 0 || n == 1)         
            return BigInteger.One;     
        
        // Caso Recursivo
        return n * FactorialProfesional(n - 1); 
    }
}