using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace SistemaOficina.Models;

public class PagamentoPorFuncionarioVM
{
    public string Funcionario { get; set; }
    public decimal Total { get; set; }
    public int Quantidade { get; set; }
    public decimal Media { get; set; }
    public decimal Maior { get; set; }
    public decimal Menor { get; set; }
}

public class PagamentoPorMesAnoVM
{
    public int Ano { get; set; }
    public int Mes { get; set; }
    public decimal Total { get; set; }
    public int Quantidade { get; set; }
    public decimal Media { get; set; }
}

public class PivotLinhaVM
{
    public string Funcionario { get; set; }
    public decimal[] Meses { get; set; } = new decimal[12];
    public decimal Total => Meses.Sum();
}


public class LoginViewModel
{
    [Required(ErrorMessage = "Informe o e-mail.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    [Display(Name = "E-mail")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Informe a senha.")]
    [DataType(DataType.Password)]
    public string Senha { get; set; }

    [Display(Name = "Lembrar-me")]
    public bool Lembrar { get; set; }
}

public static class Meses
{
    public static readonly string[] Nomes =
    {
        "Janeiro", "Fevereiro", "Março", "Abril", "Maio", "Junho",
        "Julho", "Agosto", "Setembro", "Outubro", "Novembro", "Dezembro"
    };
}

public class EquipamentoAgrupadoVM
{
    public string Grupo { get; set; }
    public int Quantidade { get; set; }
    public int EmManutencao { get; set; }
    public decimal Total { get; set; }
}
