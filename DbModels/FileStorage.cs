using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DbModels;

[Table("FileStorage")]
public partial class FileStorage
{
    [Key]
    [Column("FileStorageID")]
    public int FileStorageId { get; set; }

    public byte[] Data { get; set; } = null!;

    public int? CreateUser { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreateTime { get; set; }

    public int? UpdateUser { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdateTime { get; set; }

    public byte? ValidState { get; set; }
    public virtual ICollection<FileDescription> FileDescriptions { get; set; } = new List<FileDescription>();
}