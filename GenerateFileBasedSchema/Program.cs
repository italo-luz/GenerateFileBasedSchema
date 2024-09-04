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
            {"COD_VER", "1"},
            {"COD_FIN", "0"},
            {"DT_INI", "20230904"},
            {"DT_FIN", "20230904"},
            {"NOME", "Empresa X"},
            {"CNPJ", "12345678901234"},
            {"CPF", ""},
            {"UF", "SP"},
            {"IE", "12345678901234"},
            {"COD_MUN", "1234567"},
            {"IM", ""},
            {"SUFRAMA", ""},
            {"IND_PERFIL", "A"},
            {"IND_ATIV", "1"}
        }
    };

    var fileContent = GenerateFile(schema, data);
    Console.WriteLine(fileContent);
}
