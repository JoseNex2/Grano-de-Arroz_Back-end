using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Domain.DTO.Response
{
    public class ClientsSearchResponseDTO
    {
        public int TotalClients { get; set; }
        public IEnumerable<ClientViewDTO> Clients { get; set; }
    }
}
