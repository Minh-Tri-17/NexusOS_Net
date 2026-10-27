using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NexusOS.DAL.Models;

[Keyless]
public partial class VwRandomBinary
{
    [MaxLength(8000)]
    public byte[]? RandomBytes { get; set; }
}
