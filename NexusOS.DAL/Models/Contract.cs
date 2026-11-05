using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NexusOS.DAL.Models;

public partial class Contract
{
    [Key]
    public Guid Id { get; set; }

    public Guid? EmployeeId { get; set; }

    public Guid? ContractTypeId { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? ContractCode { get; set; }

    [StringLength(250)]
    public string? ContractName { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? ContractStatus { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? StartDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? EndDate { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? BaseSalary { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? SalaryUnit { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? WorkingHoursPerWeek { get; set; }

    [Column("OTRate", TypeName = "decimal(5, 2)")]
    public decimal? Otrate { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? AllowanceAmount { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? InsuranceContribution { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? SignedDate { get; set; }

    [StringLength(250)]
    public string? SignedBy { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? ApprovalStatus { get; set; }

    public bool? IsDelete { get; set; }

    [StringLength(500)]
    public string? Note { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    [ForeignKey("ContractTypeId")]
    [InverseProperty("Contracts")]
    public virtual CatContractType? ContractType { get; set; }
}
