using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using lab3.DTOs;
using lab3.Data;
using lab3.Models;

namespace lab3.Controllers;

[Route("printers")]
[ApiController]
public class PrinterController : ControllerBase {
	private readonly MyDbContext _context;
	private readonly ILogger<PrinterController> _logger;
	private readonly Random _random;

    public PrinterController(MyDbContext context, ILogger<PrinterController> logger) {
		_context = context;
		_logger = logger;
		_random = new Random();
	}

	[HttpGet]
	public async Task<ActionResult<IEnumerable<PrinterDTO>>> GetPrinters() {
		return await _context.Printer
			.Select(p => PrinterToDTO(p))
			.ToListAsync();
	}

	[HttpGet("{id}")]
	public async Task<ActionResult<PrinterDTO>> GetPrinter(int id) {
		PrinterModel? p = await _context.Printer.FindAsync(id);
		if (p == null) return NotFound();
		return PrinterToDTO(p);
	}

	[HttpPost]
	public async Task<ActionResult<PrinterDTO>> AddPrinter(PrinterDTO printer) {
		PrinterModel p = new PrinterModel { label = printer.label };
		_context.Printer.Add(p);
		await _context.SaveChangesAsync();
		return CreatedAtAction(nameof(AddPrinter), PrinterToDTO(p));
	}

	[HttpPatch("{id}")]
	public async Task<ActionResult<PrinterDTO>> UpdatePrinter(int id, PrinterDTO printer) {
		PrinterModel? p = await _context.Printer.FindAsync(id);
		if (p == null) return NotFound();
		p.label = printer.label;
		await _context.SaveChangesAsync();
		return PrinterToDTO(p);
	}

	[HttpPost("{id}/finish_job/{job_id}")]
	public async Task<ActionResult> FinishPrintJob(int id, int job_id) {
		PrinterModel? p = await _context.Printer.FindAsync(id);
		PrintJobModel? j = await _context.PrintJob.FindAsync(job_id);
		if (p == null || j == null) return NotFound();
		if (p.id != j.printer_id) return BadRequest("Printer id and job printer id does not match");
		if (j.status == PrintJobModel.Status.Started) {
			// Random.Next(a, b) включает a и исключает b
			// Next(0, 2) == [0, 2) == 0 или 1
			PrintJobModel.Status newStatus = (_random.Next(0, 2) == 1) ?
				PrintJobModel.Status.Finished : PrintJobModel.Status.Errored;
			j.status = newStatus;
			if (newStatus == PrintJobModel.Status.Finished) p.finished_jobs++;
			await _context.SaveChangesAsync();
			if (newStatus == PrintJobModel.Status.Finished) return Ok("Job successfully finished");
			else return Problem("Job errored");
		}
		else return Conflict("This job is not in process and cannot be finished");
	}

	[HttpGet("{id}/jobs")]
	public async Task<ActionResult<IEnumerable<PrintJobDTO>>> GetJobs(int id) {
		// мой inversepropeerty пока не работает
		// да и хрен с ним
		// PrinterModel? p = await _context.Printer.FindAsync(id);
		// if (p == null) return NotFound();
		// if (p.jobs == null) return Ok(Enumerable.Empty<PrintJobDTO>());
		// return p.jobs.Select(PrintJobToDTO).ToList();
		return await _context.PrintJob.Where(j => j.printer_id == id).Select(j => PrintJobToDTO(j)).ToListAsync();
	}

	public async Task<ActionResult<IEnumerable<PrintJobDTO>>> _GetJobsByStatus(int id, PrintJobModel.Status status) {
		// PrinterModel? p = await _context.Printer.FindAsync(id);
		// if (p == null) return NotFound();
		// if (p.jobs == null) return Ok(Enumerable.Empty<PrintJobDTO>());
		// return p.jobs.Where(j => j.status == status).Select(PrintJobToDTO).ToList();
		return await _context.PrintJob.Where(j => j.printer_id == id && j.status == status).Select(j => PrintJobToDTO(j)).ToListAsync();
	}

	[HttpGet("{id}/active_jobs")]
	public async Task<ActionResult<IEnumerable<PrintJobDTO>>> GetActiveJobs(int id) {
		return await _GetJobsByStatus(id, PrintJobModel.Status.Started);
	}

	[HttpGet("{id}/finished_jobs")]
	public async Task<ActionResult<IEnumerable<PrintJobDTO>>> GetFinishedJobs(int id) {
		return await _GetJobsByStatus(id, PrintJobModel.Status.Finished);
	}

	[HttpGet("{id}/errored_jobs")]
	public async Task<ActionResult<IEnumerable<PrintJobDTO>>> GetErroredJobs(int id) {
		return await _GetJobsByStatus(id, PrintJobModel.Status.Errored);
	}

	[HttpPost("{id}/new_job")]
	public async Task<ActionResult<PrintJobDTO>> AddJob(int id, PrintJobDTO job) {
		PrinterModel? p = await _context.Printer.FindAsync(id);
		if (p == null) return NotFound();
		PrintJobModel j = new PrintJobModel {printer_id = id, status = PrintJobModel.Status.Started};
		_context.PrintJob.Add(j);
		await _context.SaveChangesAsync();
		PrintJobDTO newJob = PrintJobToDTO(j);
		return newJob;
	}

	[HttpDelete("{id}/cancel_job/{job_id}")]
	public async Task<ActionResult> CancelJob(int id, int job_id) {
		PrinterModel? p = await _context.Printer.FindAsync(id);
		PrintJobModel? j = await _context.PrintJob.FindAsync(job_id);
		if (p == null || j == null) return NotFound();
		if (p.id != j.printer_id) return BadRequest("Printer id and job printer id does not match");
		if (j.status == PrintJobModel.Status.Finished) return Conflict("Job is already finished");
		// errored job deleted is ok
		_context.PrintJob.Remove(j);
		await _context.SaveChangesAsync();
		return Ok("Job successfully deleted");
	}

	private static PrintJobDTO PrintJobToDTO(PrintJobModel j) {
		return new PrintJobDTO {id = j.id, printer_id = j.printer_id, status = j.status};
	}

	private static PrinterDTO PrinterToDTO(PrinterModel p) {
		return new PrinterDTO {id = p.id, label = p.label, finished_jobs = p.finished_jobs};
	}
}
