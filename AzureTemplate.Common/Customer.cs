namespace AzureTemplate.Common
{
    public class Customer
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Postal { get; set; }
        public DateTime? BirthDate { get; set; }
        public string Notes { get; set; }
        public Gender Gender { get; set; }
        public bool Active { get; set; }
        public string ImageBase64 { get; set; }
    }



    public class UploadCustomerFile
    {
        public string PrettyFileName { get; set; }
        public string MimeType { get; set; }
        public string BlobId { get; set; }
        public long FileSize { get; set; }
    }

    public class CustomerFile
    {
        public string Id { get; set; }

        public string PrettyFileName { get; set; }
        public string BlobId { get; set; }
        public string MimeType { get; set; }
        public long FileSize { get; set; }
        public Uri? ItemUri { get; set; }

        public string UploadedBy { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public string CustomerId { get; set; }
        public bool Editing { get; set; } = false;
    }

    public class SasUri
    {
        public Uri Sas { get; set; }
        public string BlobId { get; set; }
    }
}

