public string GerarLinhaCNAB(string schema, Dictionary<string, object> valores)
{
    var linha = new StringBuilder();
    var linhasSchema = schema.Split('\n');

    foreach (var linhaSchema in linhasSchema)
    {
        var partes = linhaSchema.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        
        if (partes.Length < 4) continue; // Ignora linhas mal formatadas

        var posicaoInicio = int.Parse(partes[0].Split('-')[0]);
        var posicaoFim = int.Parse(partes[0].Split('-')[1]);
        var nomeCampo = partes[1];
        var tamanho = posicaoFim - posicaoInicio + 1;
        var tipo = partes[3];
        var valoresPermitidos = partes.Length > 4 ? partes[4].Split(';') : null;
        
        var valorCampo = valores.ContainsKey(nomeCampo) ? valores[nomeCampo]?.ToString() ?? string.Empty : string.Empty;
        
        // Verifica se o valor está nos valores permitidos ou no intervalo
        if (valoresPermitidos != null && valoresPermitidos.Length > 0)
        {
            var valorValido = false;

            foreach (var valorPermitido in valoresPermitidos)
            {
                // Verifica se é um intervalo (por exemplo, "1-9")
                if (valorPermitido.Contains("-"))
                {
                    var limites = valorPermitido.Split('-');
                    if (limites.Length == 2 &&
                        int.TryParse(limites[0], out var limiteInferior) &&
                        int.TryParse(limites[1], out var limiteSuperior))
                    {
                        if (int.TryParse(valorCampo, out var valorNumerico))
                        {
                            if (valorNumerico >= limiteInferior && valorNumerico <= limiteSuperior)
                            {
                                valorValido = true;
                                break;
                            }
                        }
                    }
                }
                // Verifica se é um valor exato
                else if (valorPermitido == valorCampo)
                {
                    valorValido = true;
                    break;
                }
            }

            if (!valorValido)
            {
                throw new Exception($"Valor '{valorCampo}' inválido para o campo '{nomeCampo}'. Valores permitidos: {string.Join(", ", valoresPermitidos)}");
            }
        }

        // Formatação do campo
        if (tipo == "N")
        {
            valorCampo = valorCampo.PadLeft(tamanho, '0'); // Preenchido com zeros à esquerda
        }
        else if (tipo == "A")
        {
            valorCampo = valorCampo.PadRight(tamanho, ' '); // Preenchido com espaços à direita
        }

        // Ajusta o valor se ultrapassar o tamanho definido
        valorCampo = valorCampo.Length > tamanho ? valorCampo.Substring(0, tamanho) : valorCampo;

        linha.Append(valorCampo);
    }

    return linha.ToString();
}
