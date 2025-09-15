namespace Utilities
{
    public interface ICsvService
    {
        Task<Dictionary<TimeOnly, float>> CsvToDictionary(StreamReader reader);
    }
    public class CsvService : ICsvService
    {
        public CsvService()
        {
        
        }

        public async Task<Dictionary<TimeOnly, float>> CsvToDictionary(StreamReader reader)
        {
            Dictionary<TimeOnly, float> dictionary = new Dictionary<TimeOnly, float>();
            string content = await reader.ReadToEndAsync();
            string[] lines = content.Split('\n');
            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                string[] parts = line.Split(',', 2);

                if (parts.Length != 2)
                    continue;

                if (TimeOnly.TryParse(parts[0].Trim(), out TimeOnly timeKey))
                {
                    //throw new ArgumentException($"Hora válida: {timeKey}");
                }
                else
                {
                    //throw new ArgumentException("Formato de hora inválido.");
                }

                if (float.TryParse(parts[1].Trim(), out float value))
                {
                    //throw new ArgumentException($"Hora válida: {timeKey}");
                }
                else
                {
                    //throw new ArgumentException("Formato de hora inválido.");
                }

                if (!dictionary.ContainsKey(timeKey))
                {
                    dictionary.Add(timeKey, value);
                }
                else
                {
                    //throw new ArgumentException($"Clave duplicada encontrada: {timeKey}");
                }
            }

            return dictionary;
        }
    }
}
