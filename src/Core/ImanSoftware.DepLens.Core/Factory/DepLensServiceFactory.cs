using ImanSoftware.DepLens.Abstractions.Services;
using ImanSoftware.DepLens.Core.Implementation;

namespace ImanSoftware.DepLens.Core.Factory;

public static class DepLensServiceFactory
{
    public static IDepLensService Create() => new DepLensService();

    public static IHtmlGraphReportService CreateHtmlReportGenerator() => new HtmlGraphReportService();

}
