using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NexusOS.DAL.Models;

[Table("Cat_Provinces")]
public partial class CatProvince
{
    [Key]
    public Guid Id { get; set; }

    public Guid? CountryCatId { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? ProvinceCode { get; set; }

    [StringLength(250)]
    public string ProvinceName { get; set; } = null!;

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
