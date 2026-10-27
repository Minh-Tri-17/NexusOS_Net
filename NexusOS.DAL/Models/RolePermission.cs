using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NexusOS.DAL.Models;

public partial class RolePermission
{
    [Key]
    public Guid Id { get; set; }

    public Guid? RoleId { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? PermissionCode { get; set; }

    [StringLength(500)]
    public string? PermissionValue { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? PermissionScope { get; set; }

    public bool? IsGranted { get; set; }

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
