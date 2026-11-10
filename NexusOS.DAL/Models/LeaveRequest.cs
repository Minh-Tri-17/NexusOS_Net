using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NexusOS.DAL.Models;

public partial class LeaveRequest
{
    [Key]
    public Guid Id { get; set; }

    public Guid? EmployeeId { get; set; }

    public Guid? LeaveTypeId { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? LeaveRequestCode { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? StartDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? EndDate { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? TotalDays { get; set; }

    [StringLength(500)]
    public string? Reason { get; set; }

    [StringLength(500)]
    [Unicode(false)]
    public string? FileAttach { get; set; }

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
}
