using lab3.Models;

namespace lab3.DTOs;

public class PrintJobDTO {
	public int id { get; set; }
	public int printer_id { get; set; }
	public PrintJobModel.Status status { get; set; }
}
