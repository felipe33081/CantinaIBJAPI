using System;

public static class FormatDocumentNo
{
    /// <summary>
    /// Formatar uma string CNPJ
    /// </summary>
    /// <param name="CNPJ">string CNPJ sem formatacao</param>
    /// <returns>string CNPJ formatada</returns>
    /// <example>Recebe '99999999999999' Devolve '99.999.999/9999-99'</example>

    public static string FormatCNPJ(string CNPJ)
    {
        return Convert.ToUInt64(CNPJ).ToString(@"00\.000\.000\/0000\-00");
    }

    /// <summary>
    /// Formatar uma string CPF
    /// </summary>
    /// <param name="CPF">string CPF sem formatacao</param>
    /// <returns>string CPF formatada</returns>
    /// <example>Recebe '99999999999' Devolve '999.999.999-99'</example>

    public static string FormatCPF(string CPF)
    {
        return Convert.ToUInt64(CPF).ToString(@"000\.000\.000\-00");
    }
    /// <summary>
    /// Retira a Formatacao de uma string CNPJ/CPF
    /// </summary>
    /// <param name="Codigo">string Codigo Formatada</param>
    /// <returns>string sem formatacao</returns>
    /// <example>Recebe '99.999.999/9999-99' Devolve '99999999999999'</example>

    public static string Format(string cpfOrCnpj)
    {
        if (cpfOrCnpj.Length == 14)
        {
            return FormatCNPJ(cpfOrCnpj);
        }
        if (cpfOrCnpj.Length == 11)
        {
            return FormatCPF(cpfOrCnpj);
        }
        throw new InvalidCastException("Formato inválido para CPF/CNPJ");
    }
}