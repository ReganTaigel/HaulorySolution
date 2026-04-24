using HaulitCore.Application.Features.Reports;

namespace HaulitCore.Application.Interfaces.Services;

public interface IPdfPodGenerator
{
    byte[] GeneratePodPdf(PodReportDto dto);
}