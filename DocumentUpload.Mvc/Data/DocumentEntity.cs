using System;
using System.ComponentModel.DataAnnotations;

namespace DocumentUpload.Mvc.Data
{
    public class DocumentEntity
    {
        [Key]
        public Guid Id { get; set; }
        public string FileName { get; set; }
        public string Location { get; set; }
        public string DocumentName { get; set; }
    }
}
