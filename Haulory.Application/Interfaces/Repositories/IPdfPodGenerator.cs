using Haulory.Application.Features.Reports;

namespace Haulory.Application.Interfaces.Services;

public interface IPdfPodGenerator
{
    byte[] GeneratePodPdf(PodReportDto dto);
}