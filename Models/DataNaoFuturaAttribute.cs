using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace SistemaOficina.Models;

public class DataNaoFuturaAttribute : ValidationAttribute
{
    public DataNaoFuturaAttribute() : base("A data não pode ser futura.") { }

    public override bool IsValid(object value) =>
        value == null || (value is DateTime data && data.Date <= DateTime.Today);
}
