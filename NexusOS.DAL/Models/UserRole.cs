using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NexusOS.DAL.Models;

public partial class UserRole
{
    [Key]
    public Guid Id { get; set; }

    public Guid? UserId { get; set; }

    public Guid? RoleId { get; set; }

    public Guid? StoreId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? AssignedAt { get; set; }

    public Guid? AssignedBy { get; set; }

    public bool? IsActive { get; set; }

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
