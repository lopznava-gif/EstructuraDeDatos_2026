using System;

// --- Módulo 3: Clase de prueba ---
class Alumno
{     
    public string Nombre { get; set; } = string.Empty; 
}

class Program
{
    // --- Módulo 1: Modificador ref ---
    static void Intercambiar(ref int a, ref int b) 
    {     
        int temp = a;     
        a = b;     
        b = temp;
    }

    // --- Módulo 2: Modificador out ---
    static int CalcularYValidar(int dividendo, int divisor, out int residuo) 
    {     
        residuo = dividendo % divisor;     
        return dividendo / divisor; 
    }

    static void Main(string[] args) 
    {     
        // --- Ejecución Módulo 1 ---
        Console.WriteLine("=== MÓDULO 1: MODIFICADOR REF ===");
        int x = 10, y = 25;
        Console.WriteLine($"Antes: x={x}, y={y}");     
        Intercambiar(ref x, ref y);     
        Console.WriteLine($"Después: x={x}, y={y}\n"); 

        // --- Ejecución Módulo 2 ---
        Console.WriteLine("=== MÓDULO 2: MODIFICADOR OUT ===");
        int cociente = CalcularYValidar(17, 5, out int resto);
        Console.WriteLine("Dividiendo 17 entre 5...");
        Console.WriteLine($"Cociente: {cociente}");     
        Console.WriteLine($"Residuo: {resto}\n"); 

        // --- Ejecución Módulo 3 ---
        Console.WriteLine("=== MÓDULO 3: REFERENCIAS DE OBJETOS ===");
        
        Alumno alumno1 = new Alumno { Nombre = "Dany" };     
        Alumno alumno2 = alumno1; // Solo copiamos la dirección de memoria

        Console.WriteLine($"Originalmente, alumno1 se llama: {alumno1.Nombre}");
        
        // ¡Atención! Cambiamos el nombre usando la SEGUNDA variable
        alumno2.Nombre = "3Treum";

        Console.WriteLine("Cambiamos el nombre en alumno2 a '3Treum'...");
        Console.WriteLine($"Ahora, alumno1 se llama: {alumno1.Nombre}"); // Imprime: 3Treum
    }
}