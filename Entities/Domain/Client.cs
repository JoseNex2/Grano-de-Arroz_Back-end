using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Org.BouncyCastle.Crypto.Utilities;

namespace DataAccess
{
    public class Client
    {
        public int Id { get; set; }
        public string NroGDA { get; set; }
        public string NationalId { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime SaleDate { get; set; }
        public DateTime DateRegistered { get; set; }

    }
}
