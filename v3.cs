public class FieldSchema
{
    public int Index { get; set; }
    public string Name { get; set; }
    public char Type { get; set; } // 'C' for character, 'N' for numeric
    public int? Length { get; set; }
    public List<string> AllowedValues { get; set; }

    public FieldSchema(int index, string name, char type, int? length, List<string> allowedValues)
    {
        Index = index;
        Name = name;
        Type = type;
        Length = length;
        AllowedValues = allowedValues;
    }
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
        var length = (parts[3].Trim() != "-" && !string.IsNullOrWhiteSpace(parts[3])) ? (int?)int.Parse(parts[3].Replace("*", "").Trim()) : null;
        var allowedValues = parts.Length > 5 && !string.IsNullOrWhiteSpace(parts[5]) 
                            ? parts[5].Split(';').ToList() 
                            : null;

        schema.Add(new FieldSchema(index, name, type, length, allowedValues));
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
                value = value.PadLeft(field.Length ?? 0, '0'); // Alinhar à esquerda com zeros
            }
            else if (field.Type == 'C' && field.Length.HasValue)
            {
                value = value.PadRight(field.Length.Value); // Alinhar à direita com espaços
            }

            // Truncar valores que excedem o tamanho permitido
            if (field.Length.HasValue && value.Length > field.Length.Value)
            {
                value = value.Substring(0, field.Length.Value);
            }

            line.Add(value);
        }

        lines.Add(string.Join("|", line));
    }

    return string.Join(Environment.NewLine, lines);
}

public static void Main(string[] args)
{
    string schemaText = @"
01|REG|C|4*|-|0000
02|COD_VER|N|3*|-
03|COD_FIN|N|1|-|0;1
04|DT_INI|N|8*|-
05|DT_FIN|N|8*|-
06|NOME|C|100|-
07|CNPJ|C|14*|-
08|CPF|C|11*|-
09|UF|C|2*|-
10|IE|C|14|-
11|COD_MUN|N|7*|-
12|IM|C|-|-
13|SUFRAMA|C|9*|-
14|IND_PERFIL|C|1|-|A;B;C
15|IND_ATIV|N|1|-|0;1";

    var schema = ParseSchema(schemaText);

    var data = new List<Dictionary<string, string>>()
    {
        new Dictionary<string, string>()
        {
            {"REG", "0000"},
            {"COD_VER", "014"},
            {"COD_FIN", "0"},
            {"DT_INI", "20230101"},
            {"DT_FIN", "20230131"},
            {"NOME", "Empresa Teste LTDA"},
            {"CNPJ", "12345678000195"},
            {"CPF", ""},
            {"UF", "SP"},
            {"IE", "123456789012"},
            {"COD_MUN", "3550308"},
            {"IM", ""},
            {"SUFRAMA", ""},
            {"IND_PERFIL", "A"},
            {"IND_ATIV", "1"}
        }
    };

    var fileContent = GenerateFile(schema, data);
    Console.WriteLine(fileContent);
}
