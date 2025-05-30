using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AzureTemplate.Common;

namespace AzureTemplate.Database
{
    public class Customer
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Postal { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? Notes { get; set; }

        public Gender? Gender { get; set; }
        public bool? Active { get; set; }

        public string? ImageBase64 { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public DateTime? UpdateOn { get; set; }

        public string OwnerId { get; set; }

        public List<CustomerFile> CustomerFiles { get; set; } = new List<CustomerFile>();
    }

    
    public class CustomerFile
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string BlobId { get; set; }
        public string PrettyFileName { get; set; }
        public string MimeType { get; set; }
        public long FileSize { get; set; }

        public string UploadedBy { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;


        public string CustomerId { get; set; }
        public Customer Customer { get; set; }
    }

    

    public class SystemSetting
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string? SendGridKey { get; set; }
        public string? SendGridSystemEmailAddress { get; set; }
        public bool RegistrationEnabled { get; set; } = true;
        public string? EmailDomainRestriction { get; set; }
    }
}




