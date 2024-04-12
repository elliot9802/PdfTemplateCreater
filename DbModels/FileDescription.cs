using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DbModels;

[Table("FileDescription")]
public partial class FileDescription
{
    [Key]
    [Column("FileStorageID")]
    public int FileStorageId { get; set; }

    [Column("ChildFileStorageID")]
    public int? ChildFileStorageId { get; set; }

    /// <summary>
    /// Filtyp
    /// </summary>
    [Column("FileTypeID")]
    public int FileTypeId { get; set; }

    /// <summary>
    /// Filkategori
    /// </summary>
    [Column("FileCategoryID")]
    public int FileCategoryId { get; set; }

    /// <summary>
    /// Beskrivning
    /// </summary>
    [StringLength(255)]
    public string Description { get; set; } = null!;

    /// <summary>
    /// Storlek
    /// </summary>
    public int? FileSize { get; set; }

    public int? CreateUser { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreateTime { get; set; }

    /// <summary>
    /// Namn
    /// </summary>
    [StringLength(255)]
    public string? Name { get; set; }

    public int? UpdateUser { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdateTime { get; set; }

    public byte? ValidState { get; set; }

    [ForeignKey("ChildFileStorageId")]
    [InverseProperty("InverseChildFileStorage")]
    public virtual FileDescription? ChildFileStorage { get; set; }

    [InverseProperty("ChildFileStorage")]
    public virtual ICollection<FileDescription> InverseChildFileStorage { get; set; } = new List<FileDescription>();

    [ForeignKey("FileStorageId")]
    public virtual FileStorage FileStorage { get; set; } = new FileStorage();
}