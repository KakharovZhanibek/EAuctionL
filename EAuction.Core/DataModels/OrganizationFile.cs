using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EAuction.Core.DataModels
{
    public class OrganizationFile
    {
        public Guid Id { get; set; }
        public string FileName { get; set; }
        public string Extension { get; set; }
        public byte[] Content { get; set; }
        public DateTime CreatedAt { get; set; }

        public Guid OrganizationId { get; set; }
        public Organization Organization { get; set; }
    }
}
