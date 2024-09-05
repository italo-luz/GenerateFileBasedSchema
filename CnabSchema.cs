public class CnabField
{
    public int StartPosition { get; set; }
    public int Length { get; set; }
    public char Type { get; set; } // 'N' for numeric, 'A' for alphanumeric

    public CnabField(int startPosition, int length, char type)
    {
        StartPosition = startPosition;
        Length = length;
        Type = type;
    }

    public string FormatValue(string value)
    {
        if (Type == 'N')
        {
            return value.PadLeft(Length, '0');
        }
        else if (Type == 'A')
        {
            return value.PadRight(Length, ' ');
        }

        return value;
    }
}

public class CnabRecord
{
    public Dictionary<string, CnabField> Fields { get; set; } = new Dictionary<string, CnabField>();

    public string GenerateLine(Dictionary<string, string> data)
    {
        char[] line = new char[240]; // Linha de 240 posições
        Array.Fill(line, ' '); // Preencher com espaços por padrão

        foreach (var field in Fields)
        {
            var fieldName = field.Key;
            var fieldDefinition = field.Value;
            var fieldValue = data.ContainsKey(fieldName) ? data[fieldName] : "";
            var formattedValue = fieldDefinition.FormatValue(fieldValue);

            // Inserir o valor formatado na linha na posição correta
            formattedValue.CopyTo(0, line, fieldDefinition.StartPosition - 1, fieldDefinition.Length);
        }

        return new string(line);
    }
}

public static class CnabSchemaParser
{
    public static CnabRecord ParseSchema(string schemaText)
    {
        var record = new CnabRecord();
        var lines = schemaText.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var parts = line.Split('|');
            var fieldName = parts[1].Trim();
            var fieldType = parts[2].Trim()[0]; // 'N' or 'A'
            var fieldLength = int.Parse(parts[3].Trim());
            var startPosition = int.Parse(parts[4].Trim());

            record.Fields.Add(fieldName, new CnabField(startPosition, fieldLength, fieldType));
        }

        return record;
    }
}

public static class CnabGenerator
{
    public static List<string> GenerateCnabFile(CnabRecord record, List<Dictionary<string, string>> data)
    {
        var lines = new List<string>();

        foreach (var row in data)
        {
            var line = record.GenerateLine(row);
            lines.Add(line);
        }

        return lines;
    }
}

public static void Main(string[] args)
{
    // Definindo o schema do CNAB 240 em formato de texto
    string schemaText = @"
01|CodigoBanco|N|3|1
02|LoteServico|N|4|4
03|TipoRegistro|N|1|8
04|NomeEmpresa|A|30|73
05|Agencia|N|5|18
06|NumeroConta|N|12|23
07|ValorDebito|N|15|120";

    // Parser para converter o schema em um registro CNAB
    var record = CnabSchemaParser.ParseSchema(schemaText);

    // Dados para o arquivo CNAB
    var data = new List<Dictionary<string, string>>()
    {
        new Dictionary<string, string>()
        {
            {"CodigoBanco", "001"},
            {"LoteServico", "0001"},
            {"TipoRegistro", "0"},
            {"NomeEmpresa", "Empresa Teste"},
            {"Agencia", "12345"},
            {"NumeroConta", "123456789012"},
            {"ValorDebito", "0000000012345"} // Valor de débito R$ 123,45
        }
    };

    // Gerar o arquivo CNAB
    var cnabFile = CnabGenerator.GenerateCnabFile(record, data);

    // Exibir resultado
    foreach (var line in cnabFile)
    {
        Console.WriteLine(line);
    }
}
