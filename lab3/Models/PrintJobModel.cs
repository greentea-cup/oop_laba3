using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace lab3.Models;

[Table("print_jobs")]
public class PrintJobModel {
	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	[Column("id")]
	public int id { get; set; }

	[Required]
	[Column("printer_id")]
	public int printer_id { get; set; }

	[ForeignKey("printer_id")]
	public PrinterModel? printer { get; set; }

	[Required]
	[Column("status")]
	public Status status { get; set; }

	public enum Status: int {
		Started = 0,
		Finished = 1,
		Errored = -1,
	}
}
