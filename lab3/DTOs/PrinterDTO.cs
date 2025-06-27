namespace lab3.DTOs;

public class PrinterDTO {
	public int id { get; set; }
	required public string label { get; set; }
	public int finished_jobs { get; set; }
}
