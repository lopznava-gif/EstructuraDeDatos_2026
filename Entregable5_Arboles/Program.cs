using System;

// --- 1. El Modelo de Datos: La Clase Nodo ---
public class Nodo 
{
    public int ID { get; set; }
    public string Dato { get; set; } = string.Empty;
    public Nodo? HijoIzquierdo { get; set; }
    public Nodo? HijoDerecho { get; set; }

    public Nodo(int id, string dato)
    {
        ID = id;
        Dato = dato;
    }
}

// --- 2. Programa Principal ---
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== ÁRBOLES BINARIOS DE BÚSQUEDA ===\n");
        
        Nodo raiz = new Nodo(10, "Raíz");
        raiz = InsertarNodo(raiz, new Nodo(5, "Izquierda"));
        raiz = InsertarNodo(raiz, new Nodo(15, "Derecha"));
        raiz = InsertarNodo(raiz, new Nodo(3, "Izquierda-Izquierda"));
        raiz = InsertarNodo(raiz, new Nodo(7, "Izquierda-Derecha"));
        
        Console.WriteLine("¡Nodos insertados con éxito en el árbol!\n");

        // --- BÚSQUEDA O(log n) ---
        Console.Write("Ingresa el ID del nodo a buscar (ej. 7, 15 o 3): ");
        if (int.TryParse(Console.ReadLine(), out int idTarget))
        {
            string? resultado = BuscarNodo(raiz, idTarget);
            if (resultado != null)
                Console.WriteLine($"\n¡Encontrado al instante! Dato: {resultado}");
            else
                Console.WriteLine("\nError: Nodo no encontrado en el árbol.");
        }
    }

    // Función Recursiva para Insertar Nodos
    public static Nodo InsertarNodo(Nodo? raiz, Nodo nuevoNodo) 
    { 
        if (raiz == null) return nuevoNodo; 
        
        if (nuevoNodo.ID < raiz.ID) 
            raiz.HijoIzquierdo = InsertarNodo(raiz.HijoIzquierdo, nuevoNodo); 
        else if (nuevoNodo.ID > raiz.ID) 
            raiz.HijoDerecho = InsertarNodo(raiz.HijoDerecho, nuevoNodo); 
        
        return raiz; 
    }

    // --- Función Recursiva para Buscar Nodos ---
    public static string? BuscarNodo(Nodo? raiz, int idTarget) 
    { 
        // CASO BASE 1: posición vacía (no se encontró)
        if (raiz == null) return null; 

        // CASO BASE 2: ¡encontrado!
        if (idTarget == raiz.ID) return raiz.Dato;

        // CASO RECURSIVO: decidir la dirección y descartar la otra mitad
        if (idTarget < raiz.ID) 
        { 
            // El target es menor -> busca a la izquierda
            return BuscarNodo(raiz.HijoIzquierdo, idTarget); 
        } 
        else 
        { 
            // El target es mayor -> busca a la derecha
            return BuscarNodo(raiz.HijoDerecho, idTarget); 
        } 
    }
}