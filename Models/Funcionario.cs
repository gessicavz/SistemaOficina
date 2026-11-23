using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaOficina.Models;

[Table("funcionario")]
public class Funcionario
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required(ErrorMessage = "Informe o nome.")]
    [StringLength(100)]
    [Display(Name = "Nome completo")]
    public string Nome { get; set; }

    [Required(ErrorMessage = "Informe o CPF.")]
    [RegularExpression(@"^\d{3}\.?\d{3}\.?\d{3}-?\d{2}$", ErrorMessage = "CPF inválido. Use o formato 000.000.000-00.")]
    [StringLength(14)]
    public string CPF { get; set; }

    [Required(ErrorMessage = "Informe o cargo.")]
    [StringLength(60)]
    public string Cargo { get; set; }

    [Required(ErrorMessage = "Informe o telefone.")]
    [StringLength(20)]
    public string Telefone { get; set; }

    [Required(ErrorMessage = "Informe a data de admissão.")]
    [DataNaoFutura(ErrorMessage = "A data de admissão não pode ser uma data futura.")]
    [DataType(DataType.Date)]
    [Display(Name = "Data de admissão")]
    public DateTime DataAdmissao { get; set; } = DateTime.Today;

    [Column(TypeName = "decimal(10,2)")]
    [Range(0.01, 9999999, ErrorMessage = "O salário deve ser maior que zero.")]
    [Display(Name = "Salário")]
    public decimal Salario { get; set; }

    public bool Ativo { get; set; } = true;

    public ICollection<Pagamento> Pagamentos { get; set; } = new List<Pagamento>();
    public ICollection<Equipamento> Equipamentos { get; set; } = new List<Equipamento>();
}
