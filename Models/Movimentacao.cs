using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaOficina.Models;

[Table("movimentacao")]
public class Movimentacao
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [StringLength(300)]
    [Display(Name = "Descrição")]
    public string Descricao { get; set; }

    [Required]
    [StringLength(30)]
    public string Tipo { get; set; }

    [Display(Name = "Data")]
    public DateTime DataMovimentacao { get; set; } = DateTime.Now;

    [StringLength(120)]
    [Display(Name = "Usuário")]
    public string Usuario { get; set; }
}
