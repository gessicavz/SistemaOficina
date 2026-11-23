using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaOficina.Models;

public enum SituacaoPagamento { Pendente, Pago, Atrasado }

[Table("pagamento")]
public class Pagamento
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Display(Name = "Funcionário")]
    [Required(ErrorMessage = "Selecione o funcionário.")]
    public int FuncionarioId { get; set; }

    [ForeignKey(nameof(FuncionarioId))]
    public Funcionario Funcionario { get; set; }

    [Required(ErrorMessage = "Informe a competência.")]
    [RegularExpression(@"^(0[1-9]|1[0-2])/\d{4}$", ErrorMessage = "Use o formato MM/AAAA, por exemplo 09/2026.")]
    [StringLength(20)]
    [Display(Name = "Competência (mês/ano)")]
    public string Competencia { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    [Range(0.01, 9999999, ErrorMessage = "O valor do pagamento deve ser maior que zero.")]
    public decimal Valor { get; set; }

    [Required(ErrorMessage = "Informe a data de vencimento.")]
    [DataType(DataType.Date)]
    [Display(Name = "Data de vencimento")]
    public DateTime DataVencimento { get; set; } = DateTime.Today;

    [DataType(DataType.Date)]
    [Display(Name = "Data do pagamento")]
    public DateTime? DataPagamento { get; set; }

    [Display(Name = "Forma de pagamento")]
    [StringLength(30)]
    public string FormaPagamento { get; set; }

    [Display(Name = "Situação")]
    public SituacaoPagamento Situacao { get; set; } = SituacaoPagamento.Pendente;

    [Display(Name = "Observações")]
    [StringLength(300)]
    public string Observacoes { get; set; }

    [NotMapped]
    public int DiasAtraso => Situacao != SituacaoPagamento.Pago && DataVencimento.Date < DateTime.Today
        ? (DateTime.Today - DataVencimento.Date).Days : 0;

    // Mês e ano extraídos da competência (MM/AAAA)
    [NotMapped]
    public int MesReferencia => ParteCompetencia(0);

    [NotMapped]
    public int AnoReferencia => ParteCompetencia(1);

    private int ParteCompetencia(int indice)
    {
        var partes = (Competencia ?? "").Split('/');
        return partes.Length == 2 && int.TryParse(partes[indice], out var valor) ? valor : 0;
    }
}
