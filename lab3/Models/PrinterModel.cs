using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace lab3.Models;

[Table("printers")]
public class PrinterModel {
	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	[Column("id")]
	public int id { get; set; }

	[Required]
	[StringLength(32)]
	[Column("label")]
	public string label { get; set; } = string.Empty;

	[Required]
	[Column("finished_jobs")]
	public int finished_jobs { get; set; } = 0;

	// [InverseProperty("printer")]
	// public ICollection<PrintJobModel>? jobs { get; set; }
}
