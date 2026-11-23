using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace SistemaOficina.Models;

public class DashboardViewModel
{
    public int FuncionariosAtivos { get; set; }
    public decimal TotalPrevisto { get; set; }
    public decimal TotalPago { get; set; }
    public decimal TotalPendente { get; set; }
    public decimal TotalAtrasado { get; set; }
    public int Equipamentos { get; set; }
    public int EquipamentosEmManutencao { get; set; }
    public int PagamentosEmAberto { get; set; }
    public List<Pagamento> ProximosVencimentos { get; set; } = new();
}
