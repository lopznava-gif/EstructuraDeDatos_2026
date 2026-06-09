using System;
using System.Collections.Generic;
using System.Linq;

// Nuestro Modelo de Datos
public class Producto
{
    public int ID { get; set; }
    public string Nombre { get; set; }
    public double Precio { get; set; }
    public int Cantidad { get; set; }

    // Constructor para crear productos fácilmente
    public Producto(int id, string nombre, double precio, int cantidad)
    {
        ID = id;
        Nombre = nombre;
        Precio = precio;
        Cantidad = cantidad;
    }

    // Override para mostrar info estructurada en la consola
    public override string ToString()
    {
        return $"[{ID}] {Nombre} - ${Precio:F2} | Stock: {Cantidad}";
    }
}

class Program
{
    static void Main()
    {
        Console.WriteLine("=== SISTEMA DE INVENTARIO ===\n");

        List<Producto> inventario = new List<Producto> {
            new Producto(1, "Laptop Lenovo", 15999.00, 10),
            new Producto(2, "Mouse Inalámbrico", 349.00, 25),
            new Producto(3, "Teclado Mecánico", 899.00,  0),
            new Producto(4, "Monitor 24\"", 4500.00,  5),
            new Producto(5, "Audífonos Sony", 1200.00,  0)
        };
        inventario.Add(new Producto(6, "Webcam HD", 750.00, 12));
        var otroProducto = new Producto(7, "Hub USB-C", 450.00, 8);
        inventario.Add(otroProducto);

        Console.WriteLine($"Total en inventario: {inventario.Count} productos\n");

        // --- MAGIA DE LINQ ---

        // Consulta 1: Ordenar por precio descendente
        var porPrecio = inventario.OrderByDescending(p => p.Precio).ToList();
        Console.WriteLine("=== Productos por Precio (De mayor a menor) ===");
        foreach (var p in porPrecio) 
        {
            Console.WriteLine(p); 
        }

        Console.WriteLine(); // Espacio para que se vea ordenado

        // Consulta 2: Filtrar productos agotados
        var agotados = inventario.Where(p => p.Cantidad == 0).ToList();
        Console.WriteLine("=== Productos Agotados ===");
        if (agotados.Count == 0)
            Console.WriteLine("Sin productos agotados.");
        else
            agotados.ForEach(p => Console.WriteLine(p));

        // --- PASO 5: BÚSQUEDA INSTANTÁNEA CON DICCIONARIO ---
        Console.WriteLine("\n=== BÚSQUEDA RÁPIDA POR ID ===");
        
        // 1. Convertimos nuestra lista a un diccionario usando LINQ
        Dictionary<int, Producto> catalogo = inventario.ToDictionary(p => p.ID, p => p);

        // 2. Función de búsqueda
        Console.Write("Ingresa el ID del producto a buscar (ej. 4): ");
        if (int.TryParse(Console.ReadLine(), out int idBuscado))
        {
            // TryGetValue busca al instante usando O(1)
            if (catalogo.TryGetValue(idBuscado, out Producto encontrado))
            {
                Console.WriteLine($"\n¡Encontrado al instante! -> {encontrado}");
            }
            else
            {
                Console.WriteLine("\nError: Producto no encontrado en el catálogo.");
            }
        }
    }
}
