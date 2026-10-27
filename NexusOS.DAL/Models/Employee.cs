using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NexusOS.DAL.Models;

public partial class Employee
{
    [Key]
    public Guid Id { get; set; }

    public Guid? DepartmentId { get; set; }

    public Guid? JobTitleId { get; set; }

    public Guid? StoreId { get; set; }

    public Guid? CountryId { get; set; }

    public Guid? ProvinceId { get; set; }

    public Guid? WardId { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? EmployeeCode { get; set; }

    [StringLength(250)]
    public string? EmployeeName { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Gender { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? DateOfBirth { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? PhoneNumber { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? Email { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? IdentityNumber { get; set; }

    [StringLength(500)]
    public string? Address { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? HireDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? TerminationDate { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? EmploymentStatus { get; set; }

    public bool? IsMultiStore { get; set; }

    [StringLength(500)]
    [Unicode(false)]
    public string? Photo { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? HealthInsuranceNumber { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? SocialInsuranceNumber { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? BankAccountNumber { get; set; }

    [StringLength(250)]
    public string? BankName { get; set; }

    [StringLength(250)]
    public string? EducationLevel { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? TaxCode { get; set; }

    [StringLength(250)]
    public string? EmergencyContactName { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? EmergencyContactPhone { get; set; }

    public bool? IsDelete { get; set; }

    [StringLength(500)]
    public string? Note { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }
}
