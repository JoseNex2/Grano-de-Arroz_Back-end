using DotNetEnv;

namespace Utilities
{
    public static class EnvironmentVariableLoaderService
    {
        public static void Initialize()
        {
            try
            {
                string envPath = "/secrets_extracted/dotnet-vars.env";
                int maxAttempts = 12;
                int attempts = 0;

                while (!File.Exists(envPath) && attempts < maxAttempts)
                {
                    Console.WriteLine($"Esperando por el archivo .env ({attempts + 1}/{maxAttempts})...");
                    Thread.Sleep(5000);
                    attempts++;
                }

                if (File.Exists(envPath))
                {
                    Console.WriteLine("Archivo .env encontrado, cargando variables...");
                    Env.Load(envPath);
                    Console.WriteLine("Variables cargadas");
                }
                else
                {
                    Console.WriteLine("No se encontró el archivo .env después de varios intentos.");
                    Console.WriteLine("La aplicación continuará con valores predeterminados o variables de entorno del sistema.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar el archivo .env: {ex.Message}");
                Console.WriteLine("La aplicación continuará con la configuración disponible.");
            }
        }
    }
}
