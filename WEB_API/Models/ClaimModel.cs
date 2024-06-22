using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEB_API.Models
{
    [Table("Tx_Claim")]
    public class ClaimModel
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? ClaimId { get; set; }
        [Required, DisplayName("Patient Name")] 
        public string? PatientName { get; set; }
        [Required, DisplayName("Date of Service")]
        public DateTime? DateOfService { get; set; }
        [Required, DisplayName("Medical Provider")]
        public string? MedicalProvider { get; set; }
        [DisplayName("Diagnosis")]
        public string? Diagnosis { get; set; }
        [Required, DisplayName("Claim Amount")]
        public Decimal? ClaimAmount { get; set; }
        [DisplayName("Status")]
        public string? Status { get; set; }
    }
    public class EditClaimModel
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? ClaimId { get; set; }
        [Required, DisplayName("Patient Name")]
        public string? PatientName { get; set; }
        [Required, DisplayName("Date of Service")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? DateOfService { get; set; } = System.DateTime.Today;
        [Required, DisplayName("Medical Provider")]
        public string? MedicalProvider { get; set; }
        [DisplayName("Diagnosis")]
        public string? Diagnosis { get; set; }
        [Required, DisplayName("Claim Amount")]
        public Decimal? ClaimAmount { get; set; }
        [DisplayName("Status")]
        public string? Status { get; set; }
        public List<SelectListItem>? Statuses { get; set; }
    }

    public enum StatusEnum
    {
        Pending,
        Approved,
        Rejected
    }
}