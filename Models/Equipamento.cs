using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaOficina.Models;

public enum SituacaoEquipamento { Operando, Manutencao, Parado }

public static class EquipamentoExtensoes
{
    public static string Texto(this SituacaoEquipamento s) => s switch
    {
        SituacaoEquipamento.Operando => "Operando",
        SituacaoEquipamento.Manutencao => "Em manutenção",
        _ => "Parado"
    };
}

public static class TiposEquipamento
{
    public static readonly string[] Lista =
    {
        "Torno CNC", "Fresadora", "Prensa hidráulica", "Compressor de ar", "Esteira transportadora",
        "Gerador", "Motor elétrico", "Bomba industrial", "Caldeira", "Ponte rolante", "Soldadora", "Painel elétrico"
    };
}

[Table("equipamento")]
public class Equipamento
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required(ErrorMessage = "Informe o nome do equipamento.")]
    [StringLength(100)]
    [Display(Name = "Equipamento")]
    public string Nome { get; set; }

    [Required(ErrorMessage = "Informe o tipo.")]
    [StringLength(60)]
    public string Tipo { get; set; }

    [Required(ErrorMessage = "Informe o número de série.")]
    [StringLength(40)]
    [Display(Name = "Número de série")]
    public string NumeroSerie { get; set; }

    [StringLength(60)]
    public string Fabricante { get; set; }

    [Required(ErrorMessage = "Informe a localização (indústria/setor).")]
    [StringLength(100)]
    [Display(Name = "Localização")]
    public string Localizacao { get; set; }

    [Required(ErrorMessage = "Informe a data de aquisição.")]
    [DataNaoFutura(ErrorMessage = "A data de aquisição não pode ser futura.")]
    [DataType(DataType.Date)]
    [Display(Name = "Data de aquisição")]
    public DateTime DataAquisicao { get; set; } = DateTime.Today;

    [Column(TypeName = "decimal(12,2)")]
    [Range(0.01, 999999999, ErrorMessage = "O valor deve ser maior que zero.")]
    [Display(Name = "Valor do equipamento")]
    public decimal ValorAquisicao { get; set; }

    [DataNaoFutura(ErrorMessage = "A última manutenção não pode ser uma data futura.")]
    [DataType(DataType.Date)]
    [Display(Name = "Última manutenção")]
    public DateTime? DataUltimaManutencao { get; set; }

    [Display(Name = "Situação")]
    public SituacaoEquipamento Situacao { get; set; } = SituacaoEquipamento.Operando;

    [Display(Name = "Técnico responsável")]
    [Required(ErrorMessage = "Selecione o técnico responsável.")]
    public int FuncionarioId { get; set; }

    [ForeignKey(nameof(FuncionarioId))]
    public Funcionario Funcionario { get; set; }
}
