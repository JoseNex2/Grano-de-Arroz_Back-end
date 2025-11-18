using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Domain.DTO
{
    public class ReportSearchDTO
    {
        public int Id { get; set; }
        public string ChipId { get; set; }
        public string ClientName { get; set; }
        public string ReportState { get; set; }
        public DateOnly ReportDate { get; set; }
    }
}
