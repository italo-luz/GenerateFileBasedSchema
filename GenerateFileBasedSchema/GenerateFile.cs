public class FieldSchema
{
    public int Index { get; set; }
    public string Name { get; set; }
    public char Type { get; set; } // 'C' for character, 'N' for numeric
    public int? Length { get; set; }
    public string Delimiter { get; set; }
    public List<string> AllowedValues { get; set; }

    public FieldSchema(int index, string name, char type, int? length, string delimiter, List<string> allowedValues)
    {
        Index = index;
        Name = name;
        Type = type;
        Length = length;
        Delimiter = delimiter;
        AllowedValues = allowedValues;
    }

    public static List<FieldSchema> ParseSchema(string schemaText)
    {
        var schema = new List<FieldSchema>();

        var lines = schemaText.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            var parts = line.Split('|');
            var index = int.Parse(parts[0].Trim());
            var name = parts[1].Trim();
            var type = parts[2].Trim()[0];
            var length = !string.IsNullOrWhiteSpace(parts[3]) ? (int?)int.Parse(parts[3].Replace("*", "").Trim()) : null;
            var delimiter = parts.Length > 4 ? parts[4].Trim() : "";
            var allowedValues = parts.Length > 5 && !string.IsNullOrWhiteSpace(parts[5]) 
                                ? parts[5].Split(';').ToList() 
                                : null;

            schema.Add(new FieldSchema(index, name, type, length, delimiter, allowedValues));
        }

        return schema;
    }


    public static string GenerateFile(List<FieldSchema> schema, List<Dictionary<string, string>> data)
    {
        var lines = new List<string>();

        foreach (var row in data)
        {
            var line = new List<string>();

            foreach (var field in schema)
            {
                var value = row.ContainsKey(field.Name) ? row[field.Name] : "";

                // Validação de valores permitidos
                if (field.AllowedValues != null && field.AllowedValues.Count > 0 && !field.AllowedValues.Contains(value))
                {
                    throw new ArgumentException($"Invalid value for {field.Name}: {value}");
                }

                // Formatação do valor
                if (field.Type == 'N')
                {
                    value = value.PadLeft(field.Length ?? 0, '0');
                }
                else if (field.Type == 'C' && field.Length.HasValue)
                {
                    value = value.PadRight(field.Length.Value);
                }

                line.Add(value.Substring(0, Math.Min(value.Length, field.Length ?? value.Length)) + field.Delimiter);
            }

            lines.Add(string.Join("", line));
        }

        return string.Join(Environment.NewLine, lines);
    }



}
