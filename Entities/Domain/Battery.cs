using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess;

namespace Entities.Domain
{
    public class Battery
    {
        public int Id { get; set; }
        public string BatteryGDA { get; set; }
        public string? Ot { get; set; }
        public string Type { get; set; }
        public DateTime? SaleDate { get; set; }
        public DateTime DateRegistered { get; set; }
        public int? ClientId { get; set; }
        public Client Client { get; set; }
        public ICollection<Measurement> Measurements { get; set; } = new List<Measurement>();
    }
}
