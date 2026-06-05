using System;
using System.Globalization; // Permite el uso de InvariantCulture

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== CALCULADORA DE POLÍGONOS REGULARES ===");
        
        // 1. Llamamos a la función para elegir la figura
        int numeroLados = SeleccionarPoligono();
        
        // 2. Pedimos únicamente la medida del lado (la apotema se calcula automáticamente)
        double medidaLado = PedirLadoValido();
        
        // 3. Calculamos el área aplicando trigonometría interna
        double areaFinal = CalcularArea(numeroLados, medidaLado);
        
        // 4. Mostramos el resultado limitado a 2 decimales (:F2)
        Console.WriteLine($"\nEl área del polígono de {numeroLados} lados es: {areaFinal:F2}");
    }

    // FUNCIÓN 1: Muestra el menú de selección y valida que el polígono tenga 5 o más lados
    static int SeleccionarPoligono()
    {
        int lados = 0;
        bool esValido = false;
        do
        {
            Console.WriteLine("\nElige el polígono:");
            Console.WriteLine("5. Pentágono");
            Console.WriteLine("6. Hexágono");
            Console.WriteLine("7. Heptágono");
            Console.WriteLine("8. Octágono");
            Console.Write("Ingresa el número de lados de tu figura (5 o mayor): ");
            
            string entrada = Console.ReadLine();
            esValido = int.TryParse(entrada, out lados) && lados >= 5;
            
            if (!esValido)
            {
                Console.WriteLine("Error: Por favor ingresa un número entero válido (5 o mayor).");
            }
        } while (!esValido);
        
        return lados;
    }

    // FUNCIÓN 2: Solicita y valida que el lado sea un número decimal positivo
    static double PedirLadoValido()
    {
        Console.WriteLine("\n--- Ingreso de Medidas ---");
        double numeroDecimal;
        bool esValido = false;

        do
        {
            Console.Write("Ingresa la medida de uno de los lados: ");
            string entrada = Console.ReadLine();

            // CultureInfo.InvariantCulture asegura que el programa acepte el punto decimal (.) sin importar el idioma de la PC
            esValido = double.TryParse(entrada, NumberStyles.Any, CultureInfo.InvariantCulture, out numeroDecimal) && numeroDecimal > 0;

            if (!esValido)
            {
                Console.WriteLine("Error: Ingresa un número decimal positivo válido.");
            }

        } while (!esValido);

        return numeroDecimal;
    }

    // FUNCIÓN 3: Calcula el perímetro, la apotema real por trigonometría y finalmente el área
    static double CalcularArea(int lados, double medidaLado)
    {
        // 1. Cálculo del perímetro
        double perimetro = lados * medidaLado;
        
        // 2. Cálculo automático y matemáticamente exacto de la apotema usando radianes
        double anguloCentral = Math.PI / lados;
        double apotema = medidaLado / (2 * Math.Tan(anguloCentral));
        
        // 3. Aplicación de la fórmula general: (Perímetro * Apotema) / 2
        double area = (perimetro * apotema) / 2;
        return area;
    }
}