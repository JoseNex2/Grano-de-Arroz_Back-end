using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Domain.DTO.Response
{
    public class ReportViewHistorical
    {
        public int Id { get; set; }
        public string ChipId { get; set; }
        public string ClientName { get; set; }
        public string ReportState { get; set; }
        public DateOnly ReportDate { get; set; }
        public string? TypeBattery { get; set;}
    }
}

