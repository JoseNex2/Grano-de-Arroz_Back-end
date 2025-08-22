using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Domain.DTO
{
    public class PasswordUpdateDTO
    {
        public int Id { get; set; }
        public string NewPassword { get; set; }
        public string CurrentPassword { get; set; }
    }
}
