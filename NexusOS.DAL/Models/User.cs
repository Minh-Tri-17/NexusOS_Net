using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NexusOS.DAL.Models;

public partial class User
{
    [Key]
    public Guid Id { get; set; }

    public Guid? StoreId { get; set; }

    public Guid? EmployeeId { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? UserCode { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string Username { get; set; } = null!;

    [StringLength(100)]
    [Unicode(false)]
    public string? Email { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? PhoneNumber { get; set; }

    [StringLength(1000)]
    [Unicode(false)]
    public string? PasswordHash { get; set; }

    [StringLength(1000)]
    [Unicode(false)]
    public string? PasswordSalt { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? UserStatus { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? UserType { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? LastLoginAt { get; set; }

    public int? FailedLoginCount { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? PasswordExpiredAt { get; set; }

    public bool? RequirePasswordChange { get; set; }

    public bool? TwoFactorEnabled { get; set; }

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
