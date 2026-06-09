using System;

class Program
{
    static void Main()
    {
        Console.WriteLine("=== EL DEMONIO DE LA MEMORIA ===\n");

        // Prueba 1: Pasando un Valor Normal (Value Type)
        int miNumero = 5;
        Console.WriteLine($"[Valor] Antes de la función: {miNumero}");
        CambiarValor(miNumero);
        Console.WriteLine($"[Valor] Después de la función: {miNumero}\n");

        // Prueba 2: Pasando un Arreglo (Reference Type)
        int[] miArreglo = { 1, 2, 3 };
        // Imprimimos específicamente el primer elemento usando 
        Console.WriteLine($"[Referencia] Antes de la función (Primer elemento): {miArreglo[0]}");
        CambiarReferencia(miArreglo);
        Console.WriteLine($"[Referencia] Después de la función (Primer elemento): {miArreglo[0]}");
    }

    // Función 1: Intenta cambiar el valor de un entero a 100
    static void CambiarValor(int x)
    {
        x = 100;
    }

    // Función 2: Intenta cambiar el primer elemento de un arreglo a 100
    static void CambiarReferencia(int[] arr)
    {
        // ¡AQUÍ ESTÁ LA CORRECCIÓN EXACTA!
        arr[0] = 100;
    }
}
