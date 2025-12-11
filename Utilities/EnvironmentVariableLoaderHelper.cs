using DotNetEnv;

namespace Utilities
{
    public static class EnvironmentVariableLoaderHelper
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
                    Thread.Sleep(5000);
                    attempts++;
                }

                if (File.Exists(envPath))
                {
                    Env.Load(envPath);
                }
                else
                {

                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}
