using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PruebaTecnicaCip.Web.Data;
using PruebaTecnicaCip.Web.Domain;
using PruebaTecnicaCip.Web.Features.Eligibility;
using PruebaTecnicaCip.Web.Integrations.Colegiados;

namespace PruebaTecnicaCip.Web.Pages;

public class IndexModel(
    ApplicationDbContext dbContext,
    IColegiadosClient colegiadosClient,
    IColegiadoEligibilityService eligibilityService,
    IWebHostEnvironment environment) : PageModel
{
    private const int EventId = 1;
    private const long MaxChildDniImageBytes = 2 * 1024 * 1024;
    private static readonly string[] AllowedImageExtensions = [".jpg", ".jpeg", ".png", ".webp"];

    [BindProperty]
    public RegistrationInput Input { get; set; } = new();

    [TempData]
    public string? StatusMessage { get; set; }

    [TempData]
    public string? StatusType { get; set; }

    public bool IsFull { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        IsFull = await dbContext.InstitutionalEvents
            .AsNoTracking()
            .AnyAsync(e => e.Id == EventId && e.ApprovedCount >= e.Capacity, cancellationToken);
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        ValidateChildDniImage(Input.ChildDniImage);

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var institutionalEvent = await dbContext.InstitutionalEvents
            .AsNoTracking()
            .SingleOrDefaultAsync(e => e.Id == EventId, cancellationToken);

        if (institutionalEvent is null)
        {
            ModelState.AddModelError(string.Empty, "El evento configurado no existe.");
            return Page();
        }

        if (institutionalEvent.ApprovedCount >= institutionalEvent.Capacity)
        {
            IsFull = true;
            ModelState.AddModelError(string.Empty, "El aforo del evento esta completo. No se aceptan nuevas inscripciones.");
            return Page();
        }

        var normalizedDni = Input.Dni.Trim();
        var alreadyRegistered = await dbContext.RegistrationRequests
            .AnyAsync(r => r.InstitutionalEventId == EventId && r.Dni == normalizedDni, cancellationToken);

        if (alreadyRegistered)
        {
            ModelState.AddModelError(nameof(Input.Dni), "Ya existe una solicitud registrada para este DNI.");
            return Page();
        }

        var colegiado = await colegiadosClient.FindByDniAsync(normalizedDni, cancellationToken);
        var eligibility = eligibilityService.Evaluate(colegiado);
        var childDniImagePath = await SaveChildDniImageAsync(Input.ChildDniImage!, normalizedDni, cancellationToken);
        var reviewedAt = eligibility.IsEligible ? (DateTime?)null : DateTime.UtcNow;

        var request = new RegistrationRequest
        {
            InstitutionalEventId = EventId,
            Dni = normalizedDni,
            FullName = Input.FullName.Trim(),
            Council = colegiado?.ConsejoDepartamental ?? "No registrado",
            ChildDniImagePath = childDniImagePath,
            Status = eligibility.IsEligible ? RegistrationStatus.Pending : RegistrationStatus.Rejected,
            RejectionReason = eligibility.Reason,
            CreatedAt = DateTime.UtcNow,
            ReviewedAt = reviewedAt
        };

        dbContext.RegistrationRequests.Add(request);
        await dbContext.SaveChangesAsync(cancellationToken);

        StatusType = eligibility.IsEligible ? "success" : "warning";
        StatusMessage = eligibility.IsEligible
            ? "Solicitud registrada correctamente. Queda pendiente de aprobacion."
            : $"Solicitud registrada como rechazada: {eligibility.Reason}";

        return RedirectToPage();
    }

    private void ValidateChildDniImage(IFormFile? childDniImage)
    {
        if (childDniImage is null)
        {
            return;
        }

        if (childDniImage.Length > MaxChildDniImageBytes)
        {
            ModelState.AddModelError(nameof(Input.ChildDniImage), "La imagen no debe superar 2 MB.");
        }

        var extension = Path.GetExtension(childDniImage.FileName);
        if (!AllowedImageExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(nameof(Input.ChildDniImage), "Solo se permiten imagenes JPG, PNG o WEBP.");
        }
    }

    private async Task<string> SaveChildDniImageAsync(IFormFile childDniImage, string dni, CancellationToken cancellationToken)
    {
        var uploadsDirectory = Path.Combine(environment.WebRootPath, "uploads", "child-dni");
        Directory.CreateDirectory(uploadsDirectory);

        var extension = Path.GetExtension(childDniImage.FileName).ToLowerInvariant();
        var fileName = $"{dni}-{Guid.NewGuid():N}{extension}";
        var physicalPath = Path.Combine(uploadsDirectory, fileName);

        await using var stream = System.IO.File.Create(physicalPath);
        await childDniImage.CopyToAsync(stream, cancellationToken);

        return $"/uploads/child-dni/{fileName}";
    }

    public sealed class RegistrationInput
    {
        [Required(ErrorMessage = "El DNI es obligatorio.")]
        [RegularExpression(@"^\d{8}$", ErrorMessage = "El DNI debe tener 8 digitos.")]
        public string Dni { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre completo es obligatorio.")]
        [StringLength(160, ErrorMessage = "El nombre completo no debe superar 160 caracteres.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "La imagen del DNI del menor es obligatoria.")]
        public IFormFile? ChildDniImage { get; set; }
    }
}
